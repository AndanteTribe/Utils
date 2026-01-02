#if ENABLE_UGUI
#nullable enable

using System;
using UnityEngine.EventSystems;

namespace AndanteTribe.Utils.Unity.UI
{
    /// <summary>
    /// uGUIの全入力を一時的に剥奪するなどの入力制御を行う.
    /// </summary>
    public sealed class UIBlocker
    {
        private readonly EventSystem _eventSystem = EventSystem.current;
        private uint _refCount;

        /// <summary>
        /// 入力を一時的に剥奪します.
        /// </summary>
        /// <returns>入力を復元するためのハンドル.</returns>
        public Handle Disable()
        {
            if (_refCount == 0)
            {
                _eventSystem.enabled = false;
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
            private readonly UIBlocker _controller;

            internal Handle(UIBlocker controller) => _controller = controller;

            void IDisposable.Dispose()
            {
                _controller._refCount--;
                if (_controller._refCount == 0)
                {
                    _controller._eventSystem.enabled = true;
                }
            }
        }
    }
}

#endif