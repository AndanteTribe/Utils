#if ENABLE_UNITASK
#nullable enable

using System;
using System.Diagnostics;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace AndanteTribe.Utils.Unity.Tasks.Internal
{
    /// <summary>
    /// <see cref="AutoResetUniTaskCompletionSource{T}"/>の類似実装で、リスト構造プールに対応したもの.
    /// <see cref="TimeoutController"/>を使ったハイパフォーマンスのタイムアウト制御にも対応.
    /// </summary>
    internal sealed class UniTaskNode<T> : IUniTaskSource<T>, ITaskPoolNode<UniTaskNode<T>>, IPromise<T>
    {
        private static TaskPool<UniTaskNode<T>> s_pool;
        private UniTaskNode<T>? _poolNextNode;
        ref UniTaskNode<T> ITaskPoolNode<UniTaskNode<T>>.NextNode => ref _poolNextNode!;

        private readonly TimeoutController _timeoutController = new TimeoutController();
        private CancellationToken _timeoutToken;
        private CancellationTokenRegistration _timeoutRegistration;
        private CancellationToken _externalCancellationToken;
        private CancellationTokenRegistration _externalCancellationRegistration;

        private UniTaskCompletionSourceCore<T> _core;
        private short _version;

        public UniTaskNode<T>? Prev { get; set; }
        public UniTaskNode<T>? Next { get; set; }

        static UniTaskNode()
        {
            TaskPool.RegisterSizeGetter(typeof(UniTaskNode<T>), () => s_pool.Size);
        }

        private UniTaskNode()
        {
        }

        [DebuggerHidden]
        public static UniTaskNode<T> Create()
        {
            if (!s_pool.TryPop(out var result))
            {
                result = new UniTaskNode<T>();
            }
            result._version = result._core.Version;
            TaskTracker.TrackActiveTask(result, 2);
            return result;
        }

        [DebuggerHidden]
        public UniTask<T> WaitAsync(in int millisecondsTimeout, in CancellationToken cancellationToken = default)
        {
            if (cancellationToken.CanBeCanceled)
            {
                _externalCancellationToken = cancellationToken;
                _externalCancellationRegistration = cancellationToken.UnsafeRegister(static state =>
                {
                    var self = (UniTaskNode<T>)state!;
                    self.TrySetCanceled(self._externalCancellationToken);
                }, this);
            }

            if (millisecondsTimeout != Timeout.Infinite)
            {
                _timeoutToken = _timeoutController.Timeout(millisecondsTimeout);
                _timeoutRegistration = _timeoutToken.UnsafeRegister(static state =>
                {
                    var self = (UniTaskNode<T>)state!;
                    self.TrySetCanceled(self._timeoutToken);
                }, this);
            }

            return new UniTask<T>(this, _core.Version);
        }

        [DebuggerHidden]
        public bool TrySetResult(T result)
        {
            return _version == _core.Version && _core.TrySetResult(result);
        }

        [DebuggerHidden]
        public bool TrySetCanceled(CancellationToken cancellationToken = default)
        {
            return _version == _core.Version && _core.TrySetCanceled(cancellationToken);
        }

        [DebuggerHidden]
        public bool TrySetException(Exception exception)
        {
            return _version == _core.Version && _core.TrySetException(exception);
        }

        [DebuggerHidden]
        public T GetResult(short token)
        {
            try
            {
                return _core.GetResult(token);
            }
            finally
            {
                TryReturn();
            }
        }

        [DebuggerHidden]
        void IUniTaskSource.GetResult(short token)
        {
            GetResult(token);
        }

        [DebuggerHidden]
        public UniTaskStatus GetStatus(short token)
        {
            return _core.GetStatus(token);
        }

        [DebuggerHidden]
        public UniTaskStatus UnsafeGetStatus()
        {
            return _core.UnsafeGetStatus();
        }

        [DebuggerHidden]
        public void OnCompleted(Action<object> continuation, object state, short token)
        {
            _core.OnCompleted(continuation, state, token);
        }

        [DebuggerHidden]
        private bool TryReturn()
        {
            TaskTracker.RemoveTracking(this);
            _core.Reset();
            _timeoutToken = CancellationToken.None;
            _timeoutRegistration.Dispose();
            if (!_timeoutController.IsTimeout())
            {
                _timeoutController.Reset();
            }
            _externalCancellationToken = CancellationToken.None;
            _externalCancellationRegistration.Dispose();
            Prev = null;
            Next = null;
            return s_pool.TryPush(this);
        }
    }
}

#endif