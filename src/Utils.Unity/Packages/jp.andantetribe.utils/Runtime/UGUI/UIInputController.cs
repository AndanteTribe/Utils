#if ENABLE_UGUI
#nullable enable

using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace AndanteTribe.Utils.UGUI
{
    /// <summary>
    /// uGUIの全入力を一時的に剥奪するなどの入力制御を行う.
    /// </summary>
    public sealed class UIInputController
    {
        private sealed class EmptyBaseInput : BaseInput
        {
            public override string compositionString => "";
            public override IMECompositionMode imeCompositionMode
            {
                get => IMECompositionMode.Auto;
                set { }
            }
            public override Vector2 compositionCursorPos
            {
                get => Vector2.zero;
                set { }
            }
            public override bool mousePresent => false;
            public override bool GetMouseButtonDown(int _) => false;
            public override bool GetMouseButtonUp(int _) => false;
            public override bool GetMouseButton(int _) => false;
            public override Vector2 mousePosition => Vector2.zero;
            public override Vector2 mouseScrollDelta => Vector2.zero;
            public override bool touchSupported => false;
            public override int touchCount => 0;
            public override Touch GetTouch(int _) => default;
            public override float GetAxisRaw(string _) => 0;
            public override bool GetButtonDown(string _) => false;
        }

        private readonly BaseInput _emptyInput
            = EventSystem.current.TryGetComponent<EmptyBaseInput>(out var input) ? input : EventSystem.current.gameObject.AddComponent<EmptyBaseInput>();

        private uint _refCount;

        /// <summary>
        /// 入力を一時的に剥奪します.
        /// </summary>
        /// <returns>入力を復元するためのハンドル.</returns>
        public Handle Deactivate()
        {
            if (_refCount == 0)
            {
                EventSystem.current.currentInputModule.inputOverride = _emptyInput;
            }
            _refCount++;
            return new Handle(this);
        }

        /// <summary>
        /// 入力の剥奪を制御するハンドル構造体.
        /// </summary>
        /// <remarks>
        /// 基本的にusingスコープで囲んで使用.
        /// </remarks>
        public readonly struct Handle : IDisposable
        {
            private readonly UIInputController _controller;

            internal Handle(UIInputController controller) => _controller = controller;

            void IDisposable.Dispose()
            {
                _controller._refCount--;
                if (_controller._refCount == 0)
                {
                    EventSystem.current.currentInputModule.inputOverride = null;
                }
            }
        }
    }
}

#endif