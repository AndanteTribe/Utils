#if ENABLE_UGUI && ENABLE_UNITASK
#nullable enable

using System;
using System.Buffers;
using System.Threading;
using AndanteTribe.Utils.Unity.Tasks.Internal;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AndanteTribe.Utils.Unity.UI
{
    /// <summary>
    /// タップエフェクトを表示するグラフィックコンポーネント.
    /// </summary>
    /// <remarks>
    /// レイヤーを高くすることをすすめる.
    /// </remarks>
    public class TapEffect : Graphic
    {
        private const int MaxCountDefault = 10;
        private Vector3[] _records = Array.Empty<Vector3>();
        private int _recordsCount;

        private GraphicsBuffer _graphicsBuffer = null!;
        private int _recordsID;
        private int _countID;
        private int _durationID;
        private Rect _screen;

        [SerializeReference]
        private IObjectReference<Material> _material = null!;

        [SerializeField, Tooltip("タップエフェクトの最大発生数")]
        private uint _maxCount = MaxCountDefault;

        /// <summary>
        /// タップエフェクトの最大発生数.
        /// </summary>
        public uint MaxCount
        {
            get => (uint)_graphicsBuffer.count;
            set
            {
                _maxCount = value;
                _graphicsBuffer.SetCounterValue(value);
            }
        }

        [SerializeField, Tooltip("タップエフェクトの持続時間")]
        private float _lifetime = 0.5f;

        /// <summary>
        /// タップエフェクトの持続時間.
        /// </summary>
        public float Lifetime
        {
            get => _lifetime;
            set => _lifetime = value;
        }

        /// <inheritdoc />
        public override bool raycastTarget
        {
            get => false;
            set
            {
            }
        }

        protected TapEffect() => useLegacyMeshGeneration = false;

        protected override void Awake()
        {
            base.Awake();

#if UNITY_EDITOR
            if (!UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }
#endif
            _graphicsBuffer = new(GraphicsBuffer.Target.Structured, MaxCountDefault, sizeof(float) * 3);
            _recordsID = Shader.PropertyToID("_Records");
            _countID = Shader.PropertyToID("_Count");
            _durationID = Shader.PropertyToID("_Duration");
            _screen = new Rect(0, 0, Screen.width, Screen.height);
            ArrayPool<Vector3>.Shared.Grow(ref _records, MaxCountDefault);
            LoadMaterialAsync(destroyCancellationToken).Forget();

            async UniTaskVoid LoadMaterialAsync(CancellationToken cancellationToken)
            {
                material = new Material(await _material.LoadAsync(cancellationToken));
            }
        }

        protected override void Start()
        {
            base.Start();

#if ENABLE_INPUT_SYSTEM
#if UNITY_EDITOR
            if (!UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }
#endif
            var module = (UnityEngine.InputSystem.UI.InputSystemUIInputModule)(EventSystem.current.currentInputModule == null
                ? EventSystem.current.GetComponent<BaseInputModule>() : EventSystem.current.currentInputModule);
            var callback = new Action<UnityEngine.InputSystem.InputAction.CallbackContext>(_ => OnLeftClickAsync().Forget());
            module.leftClick.action.performed += callback;
            destroyCancellationToken.UnsafeRegister(static state =>
            {
                var (module, callback) = (StateTuple<UnityEngine.InputSystem.UI.InputSystemUIInputModule, Action<UnityEngine.InputSystem.InputAction.CallbackContext>>)state!;
                if (module != null)
                {
                    module.leftClick.action.performed -= callback;
                }
            }, StateTuple.Create(module, callback));
#endif
        }

        private async UniTaskVoid OnLeftClickAsync()
        {
            if (_recordsCount >= MaxCount)
            {
                return;
            }

#if ENABLE_INPUT_SYSTEM
            var pointer = UnityEngine.InputSystem.Pointer.current;
            if (pointer == null)
            {
                return;
            }
            var screenPos = UnityEngine.InputSystem.Pointer.current.position.value;
#else
            var screenPos = (Vector2)Input.mousePosition;
#endif

            var normalizedPos = Rect.PointToNormalized(_screen, screenPos);
            var record = new Vector3(normalizedPos.x, normalizedPos.y, Time.time);
            ArrayPool<Vector3>.Shared.Grow(ref _records, _recordsCount + 1);
            _records[_recordsCount++] = record;

            await UniTask.Delay(TimeSpan.FromSeconds(_lifetime), cancellationToken: destroyCancellationToken);
            var index = _records.AsSpan(0, _recordsCount).IndexOf(record);
            _records[index] = _records[--_recordsCount];
        }

        private void Update()
        {
#if UNITY_EDITOR
            if (!UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }
#endif
            // update material
            if (material != null)
            {
                material.SetInt(_countID, _recordsCount);

                if (_recordsCount > 0)
                {
                    _graphicsBuffer.SetData(_records);
                    material.SetFloat(_durationID, _lifetime);
                }

                // d3d12: Fragment Shader "UI/TapRippleEffect" requires a buffer (SRV) "_Records" at index 0, but none provided. Skipping draw calls to avoid crashing.
                material.SetBuffer(_recordsID, _graphicsBuffer);
            }

#if !ENABLE_INPUT_SYSTEM
            if (Input.GetMouseButtonDown(0))
            {
                OnLeftClickAsync().Forget();
            }
#endif
        }

        /// <inheritdoc />
        protected override void OnDestroy()
        {
            base.OnDestroy();

#if UNITY_EDITOR
            if (!UnityEditor.EditorApplication.isPlaying && UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }
#endif
            _graphicsBuffer.Dispose();
            _material.Dispose();
            ArrayPool<Vector3>.Shared.Return(_records);

            if (material != null)
            {
                Destroy(material);
            }
        }
    }
}

#endif