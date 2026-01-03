#if ENABLE_UGUI && ENABLE_UNITASK
#nullable enable

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Unity.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Pool;
using UnityEngine.UI;

namespace AndanteTribe.Utils.Unity.Tasks
{
    public class TapEffect : IInitializable
    {
        private const int MaxCountDefault = 10;

        private readonly int _recordsID = Shader.PropertyToID("_Records");
        private readonly int _countID = Shader.PropertyToID("_Count");
        private readonly int _durationID = Shader.PropertyToID("_Duration");
        private readonly Rect _screen = new(0, 0, Screen.width, Screen.height);

        private readonly IObjectReference<Material> _material;
        private readonly Image _image;
        private readonly List<Vector3> _records;
        private readonly GraphicsBuffer _graphicsBuffer = new(GraphicsBuffer.Target.Structured, MaxCountDefault, sizeof(float) * 3);

        /// <summary>
        /// タップエフェクトの最大発生数
        /// </summary>
        public uint MaxCount
        {
            get => (uint)_graphicsBuffer.count;
            init => _graphicsBuffer.SetCounterValue(value);
        }

        /// <summary>
        /// タップエフェクトの持続時間
        /// </summary>
        public TimeSpan Duration { get; init; } = TimeSpan.FromSeconds(0.5f);

        /// <summary>
        /// Initialize a new instance of the <see cref="TapEffect"/> class.
        /// </summary>
        /// <param name="material"></param>
        /// <param name="image"></param>
        public TapEffect(IObjectReference<Material> material, Image image)
        {
            _material = material;
            _image = image;
            _records = ListPool<Vector3>.Get();
            if (_records.Capacity < MaxCountDefault)
            {
                _records.Capacity = MaxCountDefault;
            }
        }

        /// <summary>
        /// EntryPoint.
        /// </summary>
        public async ValueTask InitializeAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var module = EventSystem.current.currentInputModule == null
                ? EventSystem.current.GetComponent<BaseInputModule>() : EventSystem.current.currentInputModule;

            cancellationToken.UnsafeRegister(static state =>
            {
                var self = (TapEffect)state!;
                self._material.Dispose();
                self._graphicsBuffer.Dispose();
                ListPool<Vector3>.Release(self._records);

                if (self._image != null && self._image.material != null)
                {
                    UnityEngine.Object.Destroy(self._image.material);
                    self._image.material = null!;
                }
            }, this);

#if ENABLE_INPUT_SYSTEM
            var isModule = (UnityEngine.InputSystem.UI.InputSystemUIInputModule)module;
            var callback = new Action<UnityEngine.InputSystem.InputAction.CallbackContext>(_ => OnLeftClickAsync(cancellationToken).Forget());
            isModule.leftClick.action.performed += callback;
            cancellationToken.UnsafeRegister(static state =>
            {
                var (module, callback) = (StateTuple<UnityEngine.InputSystem.UI.InputSystemUIInputModule, Action<UnityEngine.InputSystem.InputAction.CallbackContext>>)state!;
                if (module != null)
                {
                    module.leftClick.action.performed -= callback;
                }
            }, StateTuple.Create(isModule, callback));
#endif

            // マテリアルのロード
            var material = _image.material = new Material(await _material.LoadAsync(cancellationToken));

            while (!cancellationToken.IsCancellationRequested)
            {
                UpdateMaterial(material);

#if !ENABLE_INPUT_SYSTEM
                if (Input.GetMouseButtonDown(0))
                {
                    OnLeftClickAsync(cancellationToken).Forget();
                }
#endif

                await UniTask.Yield();
            }
        }

        /// <summary>
        /// 左クリック（クリック or タップ）されたときに呼び出される処理
        /// </summary>
        /// <param name="cancellationToken"></param>
        private async UniTaskVoid OnLeftClickAsync(CancellationToken cancellationToken)
        {
            if (_records.Count >= MaxCount)
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
            _records.Add(record);

            await UniTask.Delay(Duration, cancellationToken: cancellationToken);
            _records.Remove(record);
        }

        /// <summary>
        /// マテリアルの更新処理
        /// </summary>
        /// <param name="material"></param>
        private void UpdateMaterial(Material material)
        {
            var count = _records.Count;
            material.SetInt(_countID, count);

            if (count > 0)
            {
                var records = new NativeArray<Vector3>(count, Allocator.Temp);
                _records.AsSpan().CopyTo(records);

                _graphicsBuffer.SetData(records);
                material.SetBuffer(_recordsID, _graphicsBuffer);
                material.SetFloat(_durationID, (float)Duration.TotalSeconds);
            }
        }
    }
}

#endif