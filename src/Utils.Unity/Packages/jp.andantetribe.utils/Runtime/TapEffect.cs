#nullable enable

using System;
using System.Threading;
using UnityEngine;
using UnityEngine.EventSystems;

namespace AndanteTribe.Utils
{
    public class TapEffect : IDisposable
    {
        protected readonly IObjectReference<ParticleSystem> Reference;
        protected readonly Camera EffectCam;

        private readonly CancellationTokenSource _disposableTokenSource = new();

        public bool IsDisposed => _disposableTokenSource.IsCancellationRequested;

        protected CancellationToken DisposableToken => _disposableTokenSource.Token;
        protected static BaseInputModule CurrentInputModule => EventSystem.current.currentInputModule;

        /// <summary>
        /// Initialize a new instance of <see cref="TapEffect"/>.
        /// </summary>
        /// <param name="reference"></param>
        /// <param name="effectCam"></param>
        public TapEffect(IObjectReference<ParticleSystem> reference, Camera effectCam)
        {
            Reference = reference;
            EffectCam = effectCam;

            SubscribeOnLeftClick();
        }

        protected virtual void SubscribeOnLeftClick()
        {
#if ENABLE_INPUT_SYSTEM
            var inputSystemModule = (UnityEngine.InputSystem.UI.InputSystemUIInputModule)CurrentInputModule;
            inputSystemModule.leftClick.action.performed += OnLeftClick;
#else
            CurrentInputModule.StartCoroutine(ObserveLeftClick());
#endif
        }

#if ENABLE_INPUT_SYSTEM
        private void OnLeftClick(UnityEngine.InputSystem.InputAction.CallbackContext _)
        {
            var screenPos = UnityEngine.InputSystem.Pointer.current?.position.ReadValue() ?? Vector2.zero;
            if (screenPos == Vector2.zero)
            {
                return;
            }

            var effect = Reference.LoadAsync(_disposableTokenSource.Token).GetAwaiter().GetResult();
            SpawnEffect(screenPos, effect);
        }
#else
        private System.Collections.IEnumerator ObserveLeftClick()
        {
            while (!IsDisposed)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    var screenPos = Input.mousePosition;

                    var effect = Reference.LoadAsync(DisposableTokenSource.Token).GetAwaiter().GetResult();
                    SpawnEffect(screenPos, effect);
                }

                yield return null;
            }
        }
#endif

        protected void SpawnEffect(in Vector2 screenPos, ParticleSystem effect)
        {
            effect.transform.position = EffectCam.ScreenToWorldPoint((Vector3)screenPos + EffectCam.transform.forward * 10);
            effect.Emit(1);
        }

        public void Dispose()
        {
            if (!IsDisposed)
            {
                Reference.Dispose();
                _disposableTokenSource.Cancel();
                _disposableTokenSource.Dispose();
            }
        }
    }
}