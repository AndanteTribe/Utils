using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

namespace AndanteTribe.Utils.Modules
{
    public class ShaderTapEffect : CancellationDisposable
    {
        // エフェクト情報を保持する構造体
        private struct TapEffectData
        {
            public Vector2 Position;
            public float StartTime;
            public float Progress;
            public bool IsActive;
        }

        protected readonly RectTransform CanvasRect;
        protected readonly Image EffectImage;
        protected readonly BaseInputModule CurrentInputModule;
        protected readonly float Duration = 0.5f;

        // エフェクトデータ配列（固定長）
        private readonly TapEffectData[] _effectsData;
        private readonly int _maxEffects;

        // シェーダープロパティID
        private static readonly int s_positionsID = Shader.PropertyToID("_TapPositions");
        private static readonly int s_progressesID = Shader.PropertyToID("_TapProgresses");
        private static readonly int s_countID = Shader.PropertyToID("_TapCount");

        private Vector4[] _positions;
        private float[] _progresses;
        private Coroutine _updateCoroutine;

        public ShaderTapEffect(RectTransform canvasRect, Image effectImage, Material effectMaterial,
            BaseInputModule currentInputModule = null, float duration = 0.5f, int maxEffects = 10)
        {
            CanvasRect = canvasRect;
            EffectImage = effectImage;
            CurrentInputModule = currentInputModule ?? EventSystem.current.currentInputModule;
            Duration = duration;
            _maxEffects = maxEffects;

            // 効率化のためにエフェクトデータを初期化
            _effectsData = new TapEffectData[_maxEffects];
            _positions = new Vector4[_maxEffects];
            _progresses = new float[_maxEffects];

            // 1つのマテリアルインスタンスを使用
            EffectImage.material = new Material(effectMaterial);
            EffectImage.enabled = true;
        }

        public void Start()
        {
            SubscribeOnLeftClick();
            _updateCoroutine = CurrentInputModule.StartCoroutine(UpdateEffects());
        }

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
                if (self._updateCoroutine != null)
                    inputSystemModule.StopCoroutine(self._updateCoroutine);
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

            CreateEffect(screenPos);
        }
#else
        private System.Collections.IEnumerator ObserveLeftClick()
        {
            while (!IsDisposed)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    var screenPos = Input.mousePosition;
                    CreateEffect(screenPos);
                }

                yield return null;
            }
        }
#endif

        protected void CreateEffect(in Vector2 screenPos)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                CanvasRect, screenPos, null, out var localPoint);

            // 使用可能なスロットを探す
            int index = -1;
            for (int i = 0; i < _maxEffects; i++)
            {
                if (!_effectsData[i].IsActive)
                {
                    index = i;
                    break;
                }
            }

            // 空きがない場合は最も古いエフェクトを上書き
            if (index == -1)
            {
                float oldestTime = float.MaxValue;
                for (int i = 0; i < _maxEffects; i++)
                {
                    if (_effectsData[i].StartTime < oldestTime)
                    {
                        oldestTime = _effectsData[i].StartTime;
                        index = i;
                    }
                }
            }

            // 新しいエフェクトデータを設定
            _effectsData[index] = new TapEffectData
            {
                Position = localPoint,
                StartTime = Time.time,
                Progress = 0f,
                IsActive = true
            };

            // シェーダーに渡すデータを更新
            UpdateShaderData();
        }

        private IEnumerator UpdateEffects()
        {
            while (!IsDisposed)
            {
                bool dataChanged = false;
                float currentTime = Time.time;

                // すべてのアクティブなエフェクトを更新
                for (int i = 0; i < _maxEffects; i++)
                {
                    if (_effectsData[i].IsActive)
                    {
                        float elapsed = currentTime - _effectsData[i].StartTime;
                        float progress = Mathf.Clamp01(elapsed / Duration);

                        _effectsData[i].Progress = progress;

                        // 完了したエフェクトを非アクティブにする
                        if (progress >= 1.0f)
                        {
                            var data = _effectsData[i];
                            data.IsActive = false;
                            _effectsData[i] = data;
                        }

                        dataChanged = true;
                    }
                }

                // データが変更された場合のみシェーダーを更新
                if (dataChanged)
                {
                    UpdateShaderData();
                }

                yield return null;
            }
        }

        private void UpdateShaderData()
        {
            int activeCount = 0;

            // アクティブなエフェクトの位置とプログレスを配列に設定
            for (int i = 0; i < _maxEffects; i++)
            {
                if (_effectsData[i].IsActive)
                {
                    // シェーダーに渡すためにUV座標に変換
                    Vector2 normalizedPos = new Vector2(
                        (_effectsData[i].Position.x + CanvasRect.rect.width/2) / CanvasRect.rect.width,
                        (_effectsData[i].Position.y + CanvasRect.rect.height/2) / CanvasRect.rect.height
                    );
                    Debug.Log(normalizedPos);

                    _positions[activeCount] = new Vector4(normalizedPos.x, normalizedPos.y, 0, 0);
                    _progresses[activeCount] = _effectsData[i].Progress;
                    activeCount++;
                }
            }

            // シェーダーにデータを渡す
            EffectImage.material.SetVectorArray(s_positionsID, _positions);
            EffectImage.material.SetFloatArray(s_progressesID, _progresses);
            EffectImage.material.SetInt(s_countID, activeCount);
        }

        public override void Dispose()
        {
            if (!IsDisposed)
            {
                if (EffectImage != null && EffectImage.material != null)
                {
                    Object.Destroy(EffectImage.material);
                }

                if (_updateCoroutine != null)
                {
                    CurrentInputModule.StopCoroutine(_updateCoroutine);
                    _updateCoroutine = null;
                }
            }
            base.Dispose();
        }
    }
}