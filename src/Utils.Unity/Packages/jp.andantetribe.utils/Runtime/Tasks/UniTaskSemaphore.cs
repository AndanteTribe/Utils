#if ENABLE_UNITASK
#nullable enable

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using AndanteTribe.Utils.Unity.Tasks.Internal;
using Cysharp.Threading.Tasks;
using UnityEngine.Assertions;

namespace AndanteTribe.Utils.Unity.Tasks
{
    /// <summary>
    /// UniTask版<see cref="SemaphoreSlim"/>.
    /// </summary>
    /// <example>
    /// <code>
    /// <![CDATA[
    /// using System;
    /// using AndanteTribe.Utils.Unity.Tasks;
    /// using Cysharp.Threading.Tasks;
    /// using UnityEngine;
    ///
    /// public class SemaphoreExample : MonoBehaviour
    /// {
    ///     private async UniTaskVoid Start()
    ///     {
    ///         // 同時に最大2つまで処理を許可するセマフォ
    ///         var semaphore = new UniTaskSemaphore(2, 2);
    ///
    ///         // 5つのワーカーを並列開始するが、同時実行は最大2つ
    ///         var tasks = new UniTask[5];
    ///         for (int i = 0; i < 5; i++)
    ///         {
    ///             int idx = i;
    ///             tasks[i] = WorkerAsync(idx, semaphore);
    ///         }
    ///
    ///         await UniTask.WhenAll(tasks);
    ///     }
    ///
    ///     private async UniTask WorkerAsync(int id, UniTaskSemaphore sem)
    ///     {
    ///         // WaitScopeAsync は解放用の IDisposable ハンドルを返すので using で自動解放できる
    ///         using (await sem.WaitScopeAsync())
    ///         {
    ///             Debug.Log($"Start {id}");
    ///             await UniTask.Delay(TimeSpan.FromSeconds(1));
    ///             Debug.Log($"End {id}");
    ///         }
    ///     }
    /// }
    /// ]]>
    /// </code>
    /// </example>
    [DebuggerDisplay("Current Count = {CurrentCount}")]
    public sealed class UniTaskSemaphore : IDisposable
    {
        /// <summary>
        /// 通過できる残りタスク数（空き数）.
        /// </summary>
        /// <seealso cref="SemaphoreSlim.CurrentCount"/>
        public uint CurrentCount { get; private set; }

        private readonly uint _maxCount;

        private UniTaskNode<bool>? _asyncHead;

        private UniTaskNode<bool>? _asyncTail;

        private bool _isDisposed;

        /// <summary>
        /// Initialize a new instance of the <see cref="UniTaskSemaphore"/> class.
        /// </summary>
        /// <param name="initialCount">同時に許可されるタスクの初期数.</param>
        /// <param name="maxCount">同時に許可されるタスクの最大数.</param>
        /// <exception cref="ArgumentOutOfRangeException">initialCountがmaxCountより大きい、またはmaxCountが0である.</exception>
        /// <seealso cref="SemaphoreSlim(int)"/>
        /// <seealso cref="SemaphoreSlim(int,int)"/>
        public UniTaskSemaphore(uint initialCount, uint maxCount = uint.MaxValue)
        {
            if (initialCount > maxCount)
            {
                throw new ArgumentOutOfRangeException(nameof(initialCount), initialCount, "The initialCount argument must be non-negative and less than or equal to the maximumCount.");
            }

            if (maxCount == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxCount), maxCount, "The maximumCount argument must be a positive number. If a maximum is not required, use the constructor without a maxCount parameter.");
            }

            CurrentCount = initialCount;
            _maxCount = maxCount;
        }

        /// <summary>
        /// 非同期でセマフォを待機します.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public UniTask WaitAsync(in CancellationToken cancellationToken = default)
        {
            return WaitAsync(Timeout.Infinite, cancellationToken).AsUniTask();
        }

        /// <summary>
        /// 非同期でセマフォを待機し、解放用ハンドルを取得します.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async UniTask<Handle> WaitScopeAsync(CancellationToken cancellationToken = default)
        {
            await WaitAsync(Timeout.Infinite, cancellationToken);
            return new Handle(this);
        }

        /// <summary>
        /// 非同期でセマフォを待機します.
        /// </summary>
        /// <param name="timeout"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>正常にセマフォを取得できた場合はtrue、タイムアウトした場合はfalse.</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public UniTask<bool> WaitAsync(in TimeSpan timeout, in CancellationToken cancellationToken = default)
        {
            var totalMilliseconds = (long)timeout.TotalMilliseconds;
            if (totalMilliseconds < -1 || totalMilliseconds > int.MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(timeout), timeout, "The timeout must represent a value between -1 and Int32.MaxValue, inclusive.");
            }

            return WaitAsync((int)timeout.TotalMilliseconds, cancellationToken);
        }

        /// <summary>
        /// 非同期でセマフォを待機します.
        /// </summary>
        /// <param name="millisecondsTimeout"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>正常にセマフォを取得できた場合はtrue、タイムアウトした場合はfalse.</returns>
        /// <exception cref="ObjectDisposedException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public UniTask<bool> WaitAsync(in int millisecondsTimeout, in CancellationToken cancellationToken = default)
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(nameof(UniTaskSemaphore));
            }

            if (millisecondsTimeout < -1)
            {
                throw new ArgumentOutOfRangeException(nameof(millisecondsTimeout), millisecondsTimeout, "The timeout must represent a value between -1 and Int32.MaxValue, inclusive.");
            }

            if (cancellationToken.IsCancellationRequested)
            {
                return UniTask.FromCanceled<bool>(cancellationToken);
            }

            if (CurrentCount > 0)
            {
                CurrentCount--;
                return UniTask.FromResult(true);
            }

            if (millisecondsTimeout == 0)
            {
                return UniTask.FromResult(false);
            }

            Assert.IsTrue(CurrentCount == 0, "CurrentCount should never be negative");
            var asyncWaiter = CreateAndAddAsyncWaiter();
            return millisecondsTimeout == Timeout.Infinite && !cancellationToken.CanBeCanceled
                ? asyncWaiter.WaitAsync(Timeout.Infinite)
                : WaitUntilCountOrTimeoutAsync(asyncWaiter, millisecondsTimeout, cancellationToken);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private UniTaskNode<bool> CreateAndAddAsyncWaiter()
        {
            var node = UniTaskNode<bool>.Create();

            if (_asyncHead == null)
            {
                Assert.IsTrue(_asyncTail == null, "If head is null, so too should be tail");
                _asyncHead = node;
                _asyncTail = node;
            }
            else
            {
                Assert.IsFalse(_asyncTail == null, "If head is not null, neither should be tail");
                _asyncTail!.Next = node;
                node.Prev = _asyncTail;
                _asyncTail = node;
            }

            return node;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool RemoveAsyncWaiter(UniTaskNode<bool> node)
        {
            Assert.IsFalse(node == null, "Expected non-null task");

            var wasInList = _asyncHead == node || node!.Prev != null;

            if (node!.Next != null)
            {
                node.Next.Prev = node.Prev;
            }
            if (node.Prev != null)
            {
                node.Prev.Next = node.Next;
            }
            if (_asyncHead == node)
            {
                _asyncHead = node.Next;
            }
            if (_asyncTail == node)
            {
                _asyncTail = node.Prev;
            }
            Assert.IsTrue((_asyncHead, _asyncTail) is (null, null) or (not null, not null), "Head is null iff tail is null");

            node.Next = node.Prev = null;

            return wasInList;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async UniTask<bool> WaitUntilCountOrTimeoutAsync(UniTaskNode<bool> asyncWaiter, int millisecondsTimeout, CancellationToken cancellationToken)
        {
            Assert.IsFalse(asyncWaiter == null, "Waiter should have been constructed");

            using var registration = cancellationToken.UnsafeRegister(static self => ((UniTaskNode<bool>)self!).TrySetCanceled(), asyncWaiter);

            try
            {
                await asyncWaiter!.WaitAsync(millisecondsTimeout);
                return true;
            }
            catch (Exception)
            {
                if (RemoveAsyncWaiter(asyncWaiter!))
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    return false;
                }

                throw;
            }
        }

        /// <summary>
        /// セマフォを解放します.
        /// </summary>
        /// <param name="releaseCount">解放するセマフォの数.</param>
        /// <returns></returns>
        /// <exception cref="ObjectDisposedException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="SemaphoreFullException"></exception>
        public uint Release(uint releaseCount = 1)
        {
            if (_isDisposed)
            {
#if UNITY_EDITOR
                if (!UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
                {
                    goto ForceRelease;
                }
#endif
                throw new ObjectDisposedException(nameof(UniTaskSemaphore));
            }

ForceRelease:
            if (releaseCount < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(releaseCount), releaseCount, "The releaseCount argument must be greater than zero.");
            }

            var currentCount = CurrentCount;
            var returnCount = currentCount;

            if (_maxCount - currentCount < releaseCount)
            {
                throw new SemaphoreFullException();
            }

            currentCount += releaseCount;

            if (_asyncHead != null)
            {
                Assert.IsFalse(_asyncTail == null, "tail should not be null if head isn't null");
                while (currentCount > 0 && _asyncHead != null)
                {
                    --currentCount;

                    var waiterTask = _asyncHead;
                    RemoveAsyncWaiter(waiterTask);
                    waiterTask.TrySetResult(true);
                }
            }
            CurrentCount = currentCount;

            return returnCount;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// Dispose pattern implementation.
        /// </summary>
        /// <param name="disposing"></param>
        public void Dispose(bool disposing)
        {
            if (disposing)
            {
                _isDisposed = true;
                _asyncHead = null;
                _asyncTail = null;
            }
        }

        /// <summary>
        /// セマフォの解放用ハンドル.
        /// </summary>
        public readonly struct Handle : IDisposable
        {
            private readonly UniTaskSemaphore _semaphore;

            internal Handle(UniTaskSemaphore semaphore) => _semaphore = semaphore;

            void IDisposable.Dispose() => _semaphore.Release();
        }
    }
}

#endif