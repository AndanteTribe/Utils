#if ENABLE_PARTICLESYSTEM
#nullable enable

using UnityEngine;
using UnityEngine.EventSystems;

namespace AndanteTribe.Utils.Modules
{
    public class TapEffect : CancellationDisposable
    {
        protected readonly IObjectReference<ParticleSystem> Reference;
        protected readonly Camera EffectCam;
        protected readonly BaseInputModule CurrentInputModule;

        /// <summary>
        /// Initialize a new instance of <see cref="TapEffect"/>.
        /// </summary>
        /// <param name="reference"></param>
        /// <param name="effectCam"></param>
        /// <param name="currentInputModule"></param>
        public TapEffect(IObjectReference<ParticleSystem> reference, Camera effectCam, BaseInputModule? currentInputModule = null)
        {
            Reference = reference;
            EffectCam = effectCam;
            CurrentInputModule = currentInputModule ?? EventSystem.current.currentInputModule;
        }

        public void Start() => SubscribeOnLeftClick();

        protected virtual void SubscribeOnLeftClick()
        {
#if ENABLE_INPUT_SYSTEM
            var inputSystemModule = (UnityEngine.InputSystem.UI.InputSystemUIInputModule)CurrentInputModule;
            inputSystemModule.leftClick.action.performed += OnLeftClick;
            DisposableToken.Register(static obj =>
            {
                var self = (TapEffect)obj;
                var inputSystemModule = (UnityEngine.InputSystem.UI.InputSystemUIInputModule)self.CurrentInputModule;
                inputSystemModule.leftClick.action.performed -= self.OnLeftClick;
            }, this);
#else
            CurrentInputModule.StartCoroutine(ObserveLeftClick());
#endif
        }

#if ENABLE_INPUT_SYSTEM
        protected virtual void OnLeftClick(UnityEngine.InputSystem.InputAction.CallbackContext _)
        {
            var screenPos = UnityEngine.InputSystem.Pointer.current?.position.ReadValue() ?? Vector2.zero;
            if (screenPos == Vector2.zero)
            {
                return;
            }

            var effect = Reference.LoadAsync(DisposableToken).GetAwaiter().GetResult();
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

                    var effect = Reference.LoadAsync(DisposableToken).GetAwaiter().GetResult();
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

        /// <inheritdoc/>
        public override void Dispose()
        {
            if (!IsDisposed)
            {
                Reference.Dispose();
            }
            base.Dispose();
        }
    }
}

#endif