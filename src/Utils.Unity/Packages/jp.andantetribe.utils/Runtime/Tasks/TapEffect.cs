#if ENABLE_UGUI && ENABLE_UNITASK
#nullable enable

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using Unity.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Pool;
using UnityEngine.UI;
#if ENABLE_VCONTAINER
using IStartable = VContainer.Unity.IStartable;
#endif

namespace AndanteTribe.Utils.Tasks
{
    public class TapEffect : IDisposable
#if ENABLE_VCONTAINER
    , IStartable
#endif
    {
        private const int MaxCountDefault = 10;

        private readonly int _recordsID = Shader.PropertyToID("_Records");
        private readonly int _countID = Shader.PropertyToID("_Count");
        private readonly int _durationID = Shader.PropertyToID("_Duration");
        private readonly Rect _screen = new(0, 0, Screen.width, Screen.height);

        private readonly IObjectReference<Material> _material;
        private readonly Image _image;
        private readonly CancellationTokenSource _cancellationDisposable;
        private readonly List<Vector3> _records;
        private readonly GraphicsBuffer _graphicsBuffer = new(GraphicsBuffer.Target.Structured, MaxCountDefault, sizeof(float) * 3);

        /// <summary>
        /// タップエフェクトの最大発生数
        /// </summary>
        public uint MaxCount
        {
            get
            {
                _cancellationDisposable.ThrowIfDisposed(this);
                return (uint)_graphicsBuffer.count;
            }
            set
            {
                _cancellationDisposable.ThrowIfDisposed(this);
                _graphicsBuffer.SetCounterValue(value);
            }
        }

        /// <summary>
        /// タップエフェクトの持続時間
        /// </summary>
        public TimeSpan Duration { get; set; } = TimeSpan.FromSeconds(0.5f);

        /// <summary>
        /// Initialize a new instance of the <see cref="TapEffect"/> class.
        /// </summary>
        /// <param name="material"></param>
        /// <param name="image"></param>
        public TapEffect(IObjectReference<Material> material, Image image)
        {
            _material = material;
            _image = image;
            _cancellationDisposable = CancellationTokenSource.CreateLinkedTokenSource(image.destroyCancellationToken);
            _records = ListPool<Vector3>.Get();
            if (_records.Capacity < MaxCountDefault)
            {
                _records.Capacity = MaxCountDefault;
            }
            _cancellationDisposable.Token.UnsafeRegister(static state =>
            {
                var self = (TapEffect)state!;
                self._material.Dispose();
                self._graphicsBuffer.Dispose();
                ListPool<Vector3>.Release(self._records);
            }, this);
        }

        /// <summary>
        /// EntryPoint.
        /// </summary>
        public void Start()
        {
            _cancellationDisposable.ThrowIfDisposed(this);
            var module = EventSystem.current.currentInputModule ?? EventSystem.current.GetComponent<BaseInputModule>();

#if ENABLE_INPUT_SYSTEM
            var isModule = (UnityEngine.InputSystem.UI.InputSystemUIInputModule)module;
            var callback = new Action<UnityEngine.InputSystem.InputAction.CallbackContext>(_ => OnLeftClick());
            isModule.leftClick.action.performed += callback;
            _cancellationDisposable.Token.UnsafeRegister(static state =>
            {
                var pair = ((UnityEngine.InputSystem.UI.InputSystemUIInputModule module, Action<UnityEngine.InputSystem.InputAction.CallbackContext> callback))state!;
                if (pair.module != null)
                {
                    pair.module.leftClick.action.performed -= pair.callback;
                }
            }, (isModule, callback));
#else
            UniTask.Void(static async self =>
            {
                while (!self._cancellationDisposable.IsCancellationRequested)
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        self.OnLeftClick();
                    }

                    await UniTask.Yield();
                }
            }, this);
#endif

            // マテリアルのロードと設定項目の更新
            UniTask.Void(static async self =>
            {
                var cancellationToken = self._cancellationDisposable.Token;
                var material = self._image.material = new Material(await self._material.LoadAsync(cancellationToken));

                while (!self._cancellationDisposable.IsCancellationRequested)
                {
                    var count = self._records.Count;
                    material.SetInt(self._countID, count);

                    if (count > 0)
                    {
                        var records = new NativeArray<Vector3>(count, Allocator.Temp);
                        self._records.AsSpan().CopyTo(records);

                        var graphicsBuffer = self._graphicsBuffer;
                        graphicsBuffer.SetData(records);
                        material.SetBuffer(self._recordsID, graphicsBuffer);
                        material.SetFloat(self._durationID, (float)self.Duration.TotalSeconds);
                    }

                    await UniTask.Yield();
                }
            }, this);
        }

        // 左クリック（クリック or タップ）されたときに呼び出される処理
        private void OnLeftClick()
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

            UniTask.Void(static async args =>
            {
                await UniTask.Delay(args.self.Duration, cancellationToken: args.self._cancellationDisposable.Token);
                args.self._records.Remove(args.record);
            }, (self: this, record));
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _cancellationDisposable.ThrowIfDisposed(this);
            _cancellationDisposable.Cancel();
            _cancellationDisposable.Dispose();

            if (_image != null && _image.material != null)
            {
                UnityEngine.Object.Destroy(_image.material);
                _image.material = null;
            }
        }
    }
}

#endif