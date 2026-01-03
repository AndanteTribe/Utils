#if ENABLE_UNITASK && ENABLE_LITMOTION
#nullable enable

using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;
using UnityEngine;

namespace AndanteTribe.Utils.Unity.Tasks
{
    /// <summary>
    /// 汎用トースト通知の表示実装クラス.
    /// </summary>
    [Serializable]
    public class ToastControllerCore
    {
        [SerializeField, Min(0), Tooltip("縦に並んだトースト間の間隔.")]
        private int _spacingY = 10;

        [SerializeField, Min(0), Tooltip("トーストの表示・非表示アニメーションにかける時間.")]
        private float _animDuration = 0.15f;

        [SerializeField, Min(0), Tooltip("トースト表示時間.")]
        private float _displayDuration = 1.0f;

        [SerializeField, Min(0), Tooltip("最初のトーストだけ通常より長く表示する場合の追加表示時間.")]
        private float _firstExtraDisplayDuration = 0.3f;

        [SerializeField, Min(0), Tooltip("連続して表示する場合の待機時間.")]
        private float _consecutiveWaitDuration = 0.5f;

        /// <summary>
        /// タイムスタンプ取得用.
        /// </summary>
        private readonly TimeProvider _timeProvider = null!;

        /// <summary>
        /// トーストの最大表示数.
        /// </summary>
        private readonly uint _maxToastCount;

        /// <summary>
        /// 表示しているトースト数を制御するセマフォ.
        /// </summary>
        private readonly UniTaskSemaphore _visibleSemaphore = null!;

        /// <summary>
        /// 最上部のトーストを制御するセマフォ.
        /// </summary>
        private readonly UniTaskSemaphore _topSemaphore = null!;

        /// <summary>
        /// 直前にトースト呼び出しを行ったタイムスタンプ.
        /// </summary>
        private long _lastToastTimestamp;

        /// <summary>
        /// 直前にトースト表示が完了したタイムスタンプ.
        /// </summary>
        private long _lastToastCompletedTimestamp;

        /// <summary>
        /// 現時点の連続トースト抑制加算時間.
        /// </summary>
        private TimeSpan _consecutiveAccumulatedTime;

        /// <summary>
        /// 直前に表示したトースト.
        /// </summary>
        /// <remarks>
        /// 現時点、表示されているトーストがなければnullになる想定.
        /// </remarks>
        private RectTransform? _previousToast;

        /// <summary>
        /// Initialize a new instance of <see cref="ToastControllerCore"/>.
        /// </summary>
        /// <param name="timeProvider">タイムスタンプ取得用の時間抽象.</param>
        /// <param name="maxToastCount">トーストの最大表示数.</param>
        public ToastControllerCore(TimeProvider timeProvider, uint maxToastCount)
        {
            _timeProvider = timeProvider;
            _maxToastCount = maxToastCount;
            _visibleSemaphore = new UniTaskSemaphore(maxToastCount, maxToastCount);
            _topSemaphore = new UniTaskSemaphore(0, maxToastCount - 1);
        }

        // シリアライズのためのデフォルトコンストラクタ.
        private ToastControllerCore()
        {
        }

        private async UniTask ShowAsync<TModel, TView>(
            TModel model,
            GameObjectPool<TView> pool,
            Func<TModel, float, TView, CancellationToken, UniTask> toastSetup,
            CancellationToken cancellationToken) where TView : ToastViewBase
        {
            // 最大表示数を超える分は待機.
            using var _ = await _visibleSemaphore.WaitScopeAsync(cancellationToken);

            // visibleSemaphoreの空き枠から、何個目のトーストかを取得.
            var spawnIndex = _maxToastCount - _visibleSemaphore.CurrentCount - 1;

            // 連続表示抑制処理.
            await ConsecutiveWaitAsync(cancellationToken);

            // プールから取り出して表示.
            using var handle = await pool.RentScopeAsync(cancellationToken);
            var toast = handle.Instance;
            var width = toast.RectTransform.rect.width;

            // 直前のトースト完了アニメーションが終わっていなければ待機.
            var remainAnimDuration = TimeSpan.FromSeconds(_animDuration) - _timeProvider.GetElapsedTime(_lastToastCompletedTimestamp);
            if (remainAnimDuration > TimeSpan.Zero)
            {
                await UniTask.Delay(remainAnimDuration, cancellationToken: cancellationToken);
            }

            // 直前に表示されたトーストからのY座標を計算してセットアップ.
            var (firstPosY, previousHeight) = GetFirstPosYAndPreviousHeight(_previousToast);
            await toastSetup(model, firstPosY, toast, cancellationToken);

            // 直前のトーストを更新.
            _previousToast = toast.RectTransform;

            // 一番上のトーストかどうか.
            var isTopToast = previousHeight == 0;

            await LMotion.Create(-width, 0, _animDuration).BindToAnchoredPositionX(toast.RectTransform).ToUniTask(cancellationToken);

            // 自分が最上部トーストになるまで続行.
            for (var i = spawnIndex; i > 0 && !isTopToast; i--)
            {
                await _topSemaphore.WaitAsync(cancellationToken);

                var currentPosY = toast.RectTransform.anchoredPosition.y;
                await LMotion.Create(currentPosY, GetNextPosY(currentPosY, previousHeight), _animDuration)
                    .BindToAnchoredPositionY(toast.RectTransform).ToUniTask(cancellationToken);
            }

            // 表示時間.
            var viewDuration = TimeSpan.FromSeconds(_displayDuration + (spawnIndex == 0 ? _firstExtraDisplayDuration : 0));
            await UniTask.Delay(viewDuration, cancellationToken: cancellationToken);

            await LMotion.Create(0, -width, _animDuration).BindToAnchoredPositionX(toast.RectTransform).ToUniTask(cancellationToken);

            // 最後のトーストだったら_previousToastをクリア.
            if (_visibleSemaphore.CurrentCount == _maxToastCount - 1)
            {
                _previousToast = null;
            }

            // 最上部トーストの完了を後続のトーストに通知.
            var nonTopReleaseCount = _maxToastCount - 1 - _visibleSemaphore.CurrentCount;
            if (nonTopReleaseCount > 0)
            {
                _topSemaphore.Release(nonTopReleaseCount);
            }

            // トースト完了タイムスタンプを更新.
            _lastToastCompletedTimestamp = _timeProvider.GetTimestamp();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async UniTask ConsecutiveWaitAsync(CancellationToken cancellationToken)
        {
            var threshold = TimeSpan.FromSeconds(_consecutiveWaitDuration);
            var elapsed = _timeProvider.GetElapsedTime(_lastToastTimestamp, _lastToastTimestamp = _timeProvider.GetTimestamp());
            var diff = threshold - elapsed;
            if (diff > TimeSpan.Zero)
            {
                var consecutiveTime = _consecutiveAccumulatedTime == TimeSpan.Zero ? diff : threshold;
                _consecutiveAccumulatedTime += consecutiveTime;
                await UniTask.Delay(_consecutiveAccumulatedTime, cancellationToken: cancellationToken);
                _consecutiveAccumulatedTime -= consecutiveTime;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private (float firstPosY, float previousHeight) GetFirstPosYAndPreviousHeight(RectTransform? previousToast)
        {
            if (previousToast == null)
            {
                return (0, 0);
            }
            var previousHeight = previousToast.rect.height;
            return (previousToast.anchoredPosition.y - previousHeight - _spacingY, previousHeight);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private float GetNextPosY(float currentPosY, float previousHeight)
        {
            return currentPosY + previousHeight + _spacingY;
        }

        /// <summary>
        /// トーストのビュー基底クラス.
        /// </summary>
        [RequireComponent(typeof(RectTransform))]
        public abstract class ToastViewBase : MonoBehaviour
        {
            private RectTransform? _rectTransform;

            public RectTransform RectTransform
            {
                get
                {
                    if (_rectTransform == null)
                    {
                        _rectTransform = (RectTransform)transform;
                    }
                    return _rectTransform;
                }
            }
        }
    }
}

#endif