#if ENABLE_UNITASK && ENABLE_PARTICLESYSTEM
#nullable enable

using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace AndanteTribe.Utils.Tasks
{
    public sealed class AsyncTapEffect : Modules.TapEffect
    {
        /// <summary>
        /// Initialize a new instance of <see cref="AsyncTapEffect"/>.
        /// </summary>
        /// <param name="reference"></param>
        /// <param name="effectCam"></param>
        public AsyncTapEffect(IObjectReference<ParticleSystem> reference, Camera effectCam) : base(reference, effectCam)
        {
        }

#if !ENABLE_INPUT_SYSTEM
        protected override void SubscribeOnLeftClick()
            => ObserveLeftClickAsync(DisposableToken).Forget();
#endif

#if ENABLE_INPUT_SYSTEM
        protected override void OnLeftClick(UnityEngine.InputSystem.InputAction.CallbackContext _)
        {
            var screenPos = UnityEngine.InputSystem.Pointer.current?.position.ReadValue() ?? Vector2.zero;
            if (screenPos == Vector2.zero)
            {
                return;
            }

            SpawnEffectAsync(screenPos, DisposableToken).Forget();
        }
#else
        private async UniTask ObserveLeftClickAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    var screenPos = Input.mousePosition;

                    await SpawnEffectAsync(screenPos, cancellationToken);
                }

                await UniTask.NextFrame(cancellationToken);
            }
        }
#endif

        private async UniTask SpawnEffectAsync(Vector2 screenPos, CancellationToken cancellationToken)
        {
            var effect = await Reference.LoadAsync(cancellationToken);
            SpawnEffect(screenPos, effect);
        }
    }
}

#endif