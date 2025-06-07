using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

namespace AndanteTribe.Utils.Modules
{
    public class ShaderTapEffect : CancellationDisposable
    {
        protected readonly RectTransform CanvasRect;
        protected readonly Image EffectImage;
        protected readonly BaseInputModule CurrentInputModule;
        protected readonly Material EffectMaterial;
        protected readonly float Duration = 0.5f;

        public ShaderTapEffect(RectTransform canvasRect, Image effectImage, Material effectMaterial,
            BaseInputModule currentInputModule = null, float duration = 0.5f)
        {
            CanvasRect = canvasRect;
            EffectImage = effectImage;
            EffectMaterial = effectMaterial;
            CurrentInputModule = currentInputModule ?? EventSystem.current.currentInputModule;
            Duration = duration;

            EffectImage.enabled = false;
            EffectImage.material = new Material(EffectMaterial);
        }

        public void Start() => SubscribeOnLeftClick();

        protected virtual void SubscribeOnLeftClick()
        {
#if ENABLE_INPUT_SYSTEM
            var inputSystemModule = (UnityEngine.InputSystem.UI.InputSystemUIInputModule)CurrentInputModule;
            inputSystemModule.leftClick.action.performed += OnLeftClick;
            DisposableToken.Register(static obj =>
            {
                var self = (ShaderTapEffect)obj;
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

            ShowEffect(screenPos);
        }
#else
        private System.Collections.IEnumerator ObserveLeftClick()
        {
            while (!IsDisposed)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    var screenPos = Input.mousePosition;
                    ShowEffect(screenPos);
                }

                yield return null;
            }
        }
#endif

        protected void ShowEffect(in Vector2 screenPos)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                CanvasRect, screenPos, null, out var localPoint);

            EffectImage.rectTransform.anchoredPosition = localPoint;
            EffectImage.enabled = true;

            EffectImage.material.SetFloat("_Progress", 0);

            CurrentInputModule.StartCoroutine(AnimateEffect());
        }

        private IEnumerator AnimateEffect()
        {
            float startTime = Time.time;
            float progress = 0;

            while (progress < 1.0f)
            {
                progress = (Time.time - startTime) / Duration;
                EffectImage.material.SetFloat("_Progress", progress);
                yield return null;
            }

            EffectImage.enabled = false;
        }

        public override void Dispose()
        {
            if (!IsDisposed)
            {
                if (EffectImage != null && EffectImage.material != null)
                {
                    Object.Destroy(EffectImage.material);
                }
            }
            base.Dispose();
        }
    }
}