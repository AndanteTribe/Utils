#nullable enable

using System.Threading;
using AndanteTribe.Utils.Unity;
using AndanteTribe.Utils.Unity.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace AndanteTribe.Utils.Sample
{
    public class ToastPresenter : MonoBehaviour
    {
        [SerializeField]
        private ToastControllerCore _core = new ToastControllerCore(R3.UnityTimeProvider.Update, 3);

        [SerializeReference]
        private IObjectReference<ShortToastView> _shortToastReference = null!;

        private GameObjectPool<ShortToastView> _shortToastPool = null!;

        private void Awake()
        {
            _shortToastPool = new GameObjectPool<ShortToastView>(transform, _shortToastReference, (int)_core.MaxToastCount);
            _shortToastPool.PreallocateAsync((int)_core.MaxToastCount, destroyCancellationToken).Forget();
        }

        /// <summary>
        /// 短文トーストを表示します.
        /// </summary>
        /// <param name="message">一行を想定したテキスト.</param>
        /// <param name="cancellationToken"></param>
        public UniTask ShowShortToastAsync(string message, CancellationToken cancellationToken)
        {
            return _core.ShowAsync(message, _shortToastPool, static (model, firstPosY, view, _) =>
            {
                view.Setup(model, firstPosY);
                return UniTask.CompletedTask;
            }, cancellationToken);
        }

        [Button("テスト1", "Hello, World!")]
        [Button("テスト2", "This toast is designed for text up to 19 characters.")]
        private void DebugShowShortToast(string message)
        {
#if UNITY_EDITOR
            if (!UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }
#endif
            ShowShortToastAsync(message, destroyCancellationToken).Forget();
        }

        private void OnDestroy()
        {
            _shortToastPool.Dispose();
        }
    }
}
