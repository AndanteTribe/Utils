#if ENABLE_ADDRESSABLES && ENABLE_UNITASK
#nullable enable

using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using AndanteTribe.Utils.Unity.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace AndanteTribe.Utils.Unity.Addressable
{
    /// <summary>
    /// 汎用シーン遷移実装.
    /// </summary>
    public class SceneControllerCore<TEnum> where TEnum : unmanaged, Enum
    {
        // Systemシーンをシングルロードすることを「再起動」と定義する.
        private const string DefaultSceneName = "System";

        private readonly Func<TEnum, string> _getSceneName;
        private readonly List<SceneInfo> _activeScenes = new(4);
        private readonly UniTaskSemaphore _semaphore = new(1, 1);

        /// <summary>
        /// 現在のシーン名.
        /// </summary>
        public TEnum CurrentScene { get; private set; }

        /// <summary>
        /// Initialize a new instance of <see cref="SceneControllerCore{TEnum}"/>.
        /// </summary>
        /// <param name="getSceneName">シーン名取得関数.</param>
        public SceneControllerCore(Func<TEnum, string>? getSceneName = null)
        {
            _getSceneName = getSceneName ?? (static enumValue => enumValue.ToString());
        }

        /// <summary>
        /// シーンを非同期でロードします.
        /// </summary>
        /// <param name="sceneName"></param>
        /// <param name="progress"></param>
        /// <param name="cancellationToken"></param>
        public async UniTask LoadAsync(TEnum sceneName, IProgress<float>? progress, CancellationToken cancellationToken)
        {
            var array = ArrayPool<SceneInfo>.Shared.Rent(_activeScenes.Count);
            _activeScenes.AsSpan().CopyTo(array);
            var activeScenes = new ReadOnlyMemory<SceneInfo>(array, 0, _activeScenes.Count);

            try
            {
                await _semaphore.WaitAsync(cancellationToken);

                foreach (var flag in sceneName)
                {
                    if (!CurrentScene.HasBitFlags(flag))
                    {
                        // HACK: UniTask側でprogressがnullかどうか判定しているので、ここで判定する必要なし.
                        var scene = await Addressables.LoadSceneAsync(_getSceneName(flag), LoadSceneMode.Additive)
                            .ToUniTask(progress!, cancellationToken: cancellationToken, autoReleaseWhenCanceled: true);
                        var info = new SceneInfo(flag, scene);
                        _activeScenes.Add(info);
                    }
                }

                var bag = new UniTaskBag();
                foreach (var info in activeScenes)
                {
                    if (!sceneName.HasBitFlags(info.SceneName))
                    {
                        bag.Add(Addressables.UnloadSceneAsync(info.SceneInstance)
                            .ToUniTask(progress!, cancellationToken: cancellationToken, autoReleaseWhenCanceled: true));
                        _activeScenes.Remove(info);
                    }
                }

                await bag.BuildAsync();

                CurrentScene = sceneName;
            }
            catch (Exception)
            {
                // ロードに失敗した場合は現時点でロードされているシーンを再計算する.
                CurrentScene = AggregateFlags(_activeScenes.AsSpan());
                throw;

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                static TEnum AggregateFlags(ReadOnlySpan<SceneInfo> infos)
                {
                    var activeScenes = (Span<TEnum>)stackalloc TEnum[infos.Length];
                    for (var i = 0; i < infos.Length; i++)
                    {
                        activeScenes[i] = infos[i].SceneName;
                    }

                    return activeScenes.AggregateFlags();
                }
            }
            finally
            {
                ArrayPool<SceneInfo>.Shared.Return(array);
                _semaphore.Release();
            }
        }

        /// <summary>
        /// すべてのシーンをアンロードします.
        /// </summary>
        /// <param name="progress"></param>
        /// <param name="cancellationToken"></param>
        public async UniTask UnloadAllAsync(IProgress<float>? progress, CancellationToken cancellationToken)
        {
            using var _ = await _semaphore.WaitScopeAsync(cancellationToken);
            await UnloadAllCoreAsync(progress, cancellationToken);
        }

        /// <summary>
        /// アプリを再起動します.
        /// </summary>
        /// <param name="progress"></param>
        /// <param name="forceImmediate"></param>
        /// <param name="cancellationToken"></param>
        public async UniTask RestartAsync(IProgress<float>? progress, bool forceImmediate, CancellationToken cancellationToken)
        {
            if (forceImmediate)
            {
                await UnloadAllCoreAsync(progress, cancellationToken);
                await SceneManager.LoadSceneAsync(DefaultSceneName)!.ToUniTask(progress!, cancellationToken: cancellationToken);
            }
            else
            {
                using var _ = await _semaphore.WaitScopeAsync(cancellationToken);
                await UnloadAllCoreAsync(progress, cancellationToken);
                await SceneManager.LoadSceneAsync(DefaultSceneName)!.ToUniTask(progress!, cancellationToken: cancellationToken);
            }
        }

        private async UniTask UnloadAllCoreAsync(IProgress<float>? progress, CancellationToken cancellationToken)
        {
            var bag = new UniTaskBag();
            try
            {
                foreach (var info in _activeScenes)
                {
                    bag.Add(Addressables.UnloadSceneAsync(info.SceneInstance)
                        .ToUniTask(progress!, cancellationToken: cancellationToken, autoReleaseWhenCanceled: true));
                }
                _activeScenes.Clear();

                await bag.BuildAsync();
            }
            finally
            {
                CurrentScene = default;
            }
        }

        private readonly struct SceneInfo : IEquatable<SceneInfo>
        {
            public readonly TEnum SceneName;

            public readonly SceneInstance SceneInstance;

            public SceneInfo(TEnum sceneName, SceneInstance sceneInstance)
            {
                SceneName = sceneName;
                SceneInstance = sceneInstance;
            }

            /// <inheritdoc />
            public bool Equals(SceneInfo other) => EqualityComparer<TEnum>.Default.Equals(SceneName, other.SceneName);

            /// <inheritdoc />
            public override int GetHashCode() => EqualityComparer<TEnum>.Default.GetHashCode(SceneName);
        }
    }
}

#endif