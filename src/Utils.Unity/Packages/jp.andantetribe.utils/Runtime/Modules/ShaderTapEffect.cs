using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

namespace AndanteTribe.Utils.Modules
{
    public class ShaderTapEffect : CancellationDisposable
    {
        private struct TapEffectData
        {
            public Vector2 Position;
            public float StartTime;
            public float Progress;
            public bool IsActive;
        }

        private readonly RectTransform _canvasRect;
        private readonly Image _effectImage;
        private readonly BaseInputModule _currentInputModule;
        private readonly float _duration;

        private readonly TapEffectData[] _effectsData;
        private readonly int _maxEffects;
        private readonly Vector4[] _positions;
        private readonly float[] _progresses;

        private static readonly int s_positionsID = Shader.PropertyToID("_TapPositions");
        private static readonly int s_progressesID = Shader.PropertyToID("_TapProgresses");
        private static readonly int s_countID = Shader.PropertyToID("_TapCount");

        private Coroutine _updateCoroutine;

        /// <summary>
        /// Initialize a new instance of  <see cref="ShaderTapEffect"/>.
        /// </summary>
        /// <param name="canvasRect"></param>
        /// <param name="effectImage"></param>
        /// <param name="effectMaterial"></param>
        /// <param name="currentInputModule"></param>
        /// <param name="duration"></param>
        /// <param name="maxEffects"></param>
        public ShaderTapEffect(RectTransform canvasRect, Image effectImage, Material effectMaterial,
            BaseInputModule currentInputModule = null, float duration = 0.5f, int maxEffects = 10)
        {
            _canvasRect = canvasRect;
            _effectImage = effectImage;
            _currentInputModule = currentInputModule ?? EventSystem.current.currentInputModule;
            _duration = duration;
            _maxEffects = maxEffects;

            _effectsData = new TapEffectData[_maxEffects];
            _positions = new Vector4[_maxEffects];
            _progresses = new float[_maxEffects];

            _effectImage.material = new Material(effectMaterial);
            _effectImage.enabled = true;
        }

        public void Start()
        {
            SubscribeOnLeftClick();
            _updateCoroutine = _currentInputModule.StartCoroutine(UpdateEffects());
        }

        protected virtual void SubscribeOnLeftClick()
        {
#if ENABLE_INPUT_SYSTEM
            var inputSystemModule = (UnityEngine.InputSystem.UI.InputSystemUIInputModule)_currentInputModule;
            inputSystemModule.leftClick.action.performed+= OnLeftClick;
            Debug.Log("TapEffect: Subscribed to left click action.");
            DisposableToken.Register(static obj =>
            {
                var self = (ShaderTapEffect)obj;
                if (self._currentInputModule != null)
                {
                    var inputSystemModule = (UnityEngine.InputSystem.UI.InputSystemUIInputModule)self._currentInputModule;
                    inputSystemModule.leftClick.action.performed -= self.OnLeftClick;
                    Debug.Log("TapEffect: Subscribed to left click action.");
                }
            }, this);
#else
            CurrentInputModule.StartCoroutine(ObserveLeftClick());
#endif
        }

#if ENABLE_INPUT_SYSTEM
        protected virtual void OnLeftClick(UnityEngine.InputSystem.InputAction.CallbackContext context)
        {
            var screenPos = UnityEngine.InputSystem.Pointer.current?.position.ReadValue() ?? Vector2.zero;
            if (screenPos != Vector2.zero)
            {
                CreateEffect(screenPos);
            }
        }
#else
        private System.Collections.IEnumerator ObserveLeftClick()
        {
            while (!IsDisposed)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    CreateEffect(Input.mousePosition);
                }
                yield return null;
            }
        }
#endif

        protected void CreateEffect(in Vector2 screenPos)
        {
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _canvasRect, screenPos, null, out var localPoint))
                return;

            int index = FindAvailableSlot();
            _effectsData[index] = new TapEffectData
            {
                Position = localPoint,
                StartTime = Time.time,
                Progress = 0f,
                IsActive = true
            };

            UpdateShaderData();
        }

        private int FindAvailableSlot()
        {
            for (int i = 0; i < _maxEffects; i++)
            {
                if (!_effectsData[i].IsActive)
                    return i;
            }

            int oldestIndex = 0;
            float oldestTime = _effectsData[0].StartTime;
            for (int i = 1; i < _maxEffects; i++)
            {
                if (_effectsData[i].StartTime < oldestTime)
                {
                    oldestTime = _effectsData[i].StartTime;
                    oldestIndex = i;
                }
            }
            return oldestIndex;
        }

        private IEnumerator UpdateEffects()
        {
            while (!IsDisposed)
            {
                bool dataChanged = UpdateEffectProgress();
                if (dataChanged)
                {
                    UpdateShaderData();
                }
                yield return null;
            }
        }

        private bool UpdateEffectProgress()
        {
            bool dataChanged = false;
            float currentTime = Time.time;

            for (int i = 0; i < _maxEffects; i++)
            {
                if (!_effectsData[i].IsActive) continue;

                float elapsed = currentTime - _effectsData[i].StartTime;
                float progress = Mathf.Clamp01(elapsed / _duration);

                var data = _effectsData[i];
                data.Progress = progress;

                if (progress >= 1.0f)
                {
                    data.IsActive = false;
                }

                _effectsData[i] = data;
                dataChanged = true;
            }

            return dataChanged;
        }

        private void UpdateShaderData()
        {
            int activeCount = 0;

            for (int i = 0; i < _maxEffects; i++)
            {
                if (!_effectsData[i].IsActive) continue;

                Vector2 normalizedPos = NormalizePosition(_effectsData[i].Position);
                _positions[activeCount] = new Vector4(normalizedPos.x, normalizedPos.y, 0, 0);
                _progresses[activeCount] = _effectsData[i].Progress;
                activeCount++;
            }

            var material = _effectImage.material;
            material.SetVectorArray(s_positionsID, _positions);
            material.SetFloatArray(s_progressesID, _progresses);
            material.SetInt(s_countID, activeCount);
        }

        private Vector2 NormalizePosition(Vector2 localPos)
        {
            var rect = _canvasRect.rect;
            return new Vector2(
                (localPos.x + rect.width * 0.5f) / rect.width,
                (localPos.y + rect.height * 0.5f) / rect.height
            );
        }

        public override void Dispose()
        {
            if (!IsDisposed)
            {
                if (_effectImage?.material != null)
                {
                    Object.Destroy(_effectImage.material);
                }

                if (_updateCoroutine != null && _currentInputModule != null)
                {
                    _currentInputModule.StopCoroutine(_updateCoroutine);
                    _updateCoroutine = null;
                }
            }
            base.Dispose();
        }
    }
}