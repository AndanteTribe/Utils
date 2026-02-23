#if ENABLE_UNITASK
#nullable enable

using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Threading;
using AndanteTribe.Utils.Unity.Tasks.Internal;
using Cysharp.Threading.Tasks;

namespace AndanteTribe.Utils.Unity.Tasks
{
    /// <summary>
    /// 複数の<see cref="UniTask{T}"/>を集め、それら全てが完了した時に完成するバッグ.
    /// </summary>
    /// <remarks>
    /// 複数の<see cref="UniTask{T}"/>を並列待機する挙動は<see cref="UniTask.WhenAll(UniTask{T}[])"/>と同じ.
    /// </remarks>
    /// <example>
    /// <code>
    /// <![CDATA[
    /// using System;
    /// using AndanteTribe.Utils.Unity;
    /// using UnityEngine;
    ///
    /// public class Example : MonoBehaviour
    /// {
    ///     private void Start()
    ///     {
    ///         // リストを使った場合
    ///         // var list = new List<UniTask>();
    ///         // for (int i = 0; i < 10; i++)
    ///         // {
    ///         //     list.Add(UniTask.Delay(TimeSpan.FromSeconds(i)));
    ///         // }
    ///         // await UniTask.WhenAll(list);
    ///
    ///         // UniTaskBagを使った場合
    ///         var bag = new UniTaskBag();
    ///         for (int i = 0; i < 10; i++)
    ///         {
    ///            bag.Add(UniTask.Delay(TimeSpan.FromSeconds(i)));
    ///         }
    ///         await bag.BuildAsync();
    ///     }
    /// }
    /// ]]>
    /// </code>
    /// </example>
    public sealed class UniTaskBag<T> : IUniTaskSource<T[]>
    {
        private T[] _result = Array.Empty<T>();
        private int _completeCount;
        private UniTaskCompletionSourceCore<T[]> _core; // don't reset(called after GetResult, will invoke TrySetException.)

        private UniTask<T>[] _tasks = Array.Empty<UniTask<T>>();
        private int _tasksLength;

        /// <summary>
        /// <see cref="UniTask{T}"/>をバッグに追加する.
        /// </summary>
        /// <param name="task">追加する<see cref="UniTask{T}"/>.</param>
        /// <exception cref="InvalidOperationException"><see cref="BuildAsync"/>が呼び出された後に追加しようとした場合.</exception>
        public void Add(UniTask<T> task)
        {
            if (_core.Version != 0)
            {
                throw new InvalidOperationException("Cannot add tasks after BuildAsync is called.");
            }

            if (_tasksLength >= _tasks.Length)
            {
                if (_tasksLength > 0)
                {
                    var newTasks = ArrayPool<UniTask<T>>.Shared.Rent(_tasksLength * 2);
                    _tasks.AsSpan().CopyTo(newTasks);
                    ArrayPool<UniTask<T>>.Shared.Return(_tasks, clearArray: true);
                    _tasks = newTasks;
                }
                else if (_tasksLength == 0)
                {
                    _tasks = ArrayPool<UniTask<T>>.Shared.Rent(4);
                }
            }
            _tasks[_tasksLength++] = task;
        }

        /// <summary>
        /// 追加されたすべての<see cref="UniTask{T}"/>が完了したときに完了する<see cref="UniTask{T[]}"/>を構築して返す.
        /// </summary>
        public UniTask<T[]> BuildAsync()
        {
            TaskTracker.TrackActiveTask(this, 3);

            if (_tasksLength == 0)
            {
                _core.TrySetResult(_result);
                return new UniTask<T[]>(this, _core.Version); // GetResult will clean up.
            }

            this._result = new T[_tasksLength];

            for (int i = 0; i < _tasksLength; i++)
            {
                UniTask<T>.Awaiter awaiter;
                try
                {
                    awaiter = _tasks[i].GetAwaiter();
                }
                catch (Exception ex)
                {
                    _core.TrySetException(ex);
                    continue;
                }

                if (awaiter.IsCompleted)
                {
                    TryInvokeContinuation(this, awaiter, i);
                }
                else
                {
                    awaiter.SourceOnCompleted(static state =>
                    {
                        var (self, awaiter, i) = (StateTuple<UniTaskBag<T>, UniTask<T>.Awaiter, int>)state;
                        TryInvokeContinuation(self, awaiter, i);
                    }, StateTuple.Create(this, awaiter, i));
                }
            }

            return new UniTask<T[]>(this, _core.Version);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void TryInvokeContinuation(UniTaskBag<T> self, in UniTask<T>.Awaiter awaiter, int i)
        {
            try
            {
                self._result[i] = awaiter.GetResult();
            }
            catch (Exception ex)
            {
                self._core.TrySetException(ex);
                return;
            }

            if (Interlocked.Increment(ref self._completeCount) == self._result.Length)
            {
                self._core.TrySetResult(self._result);
            }
        }

        T[] IUniTaskSource<T[]>.GetResult(short token)
        {
            TaskTracker.RemoveTracking(this);
            GC.SuppressFinalize(this);
            ArrayPool<UniTask<T>>.Shared.Return(_tasks, clearArray: true);
            return _core.GetResult(token);
        }

        void IUniTaskSource.GetResult(short token)
        {
            ((IUniTaskSource<T[]>)this).GetResult(token);
        }

        UniTaskStatus IUniTaskSource.GetStatus(short token)
        {
            return _core.GetStatus(token);
        }

        UniTaskStatus IUniTaskSource.UnsafeGetStatus()
        {
            return _core.UnsafeGetStatus();
        }

        void IUniTaskSource.OnCompleted(Action<object> continuation, object state, short token)
        {
            _core.OnCompleted(continuation, state, token);
        }
    }

    /// <summary>
    /// 複数の<see cref="UniTask"/>を集め、それら全てが完了した時に完成するバッグ.
    /// </summary>
    /// <remarks>
    /// 複数の<see cref="UniTask"/>を並列待機する挙動は<see cref="UniTask.WhenAll(UniTask[])"/>と同じ.
    /// </remarks>
    public sealed class UniTaskBag : IUniTaskSource
    {
        private int _completeCount;
        private int _tasksLength;
        private UniTaskCompletionSourceCore<AsyncUnit> _core; // don't reset(called after GetResult, will invoke TrySetException.)

        private UniTask[] _tasks = Array.Empty<UniTask>();

        /// <summary>
        /// <see cref="UniTask"/>をバッグに追加する.
        /// </summary>
        /// <param name="task">追加する<see cref="UniTask"/>.</param>
        /// <exception cref="InvalidOperationException"><see cref="BuildAsync"/>が呼び出された後に追加しようとした場合.</exception>
        public void Add(UniTask task)
        {
            if (_core.Version != 0)
            {
                throw new InvalidOperationException("Cannot add tasks after BuildAsync is called.");
            }

            if (_tasksLength >= _tasks.Length)
            {
                if (_tasksLength > 0)
                {
                    var newTasks = ArrayPool<UniTask>.Shared.Rent(_tasksLength * 2);
                    _tasks.AsSpan().CopyTo(newTasks);
                    ArrayPool<UniTask>.Shared.Return(_tasks, clearArray: true);
                    _tasks = newTasks;
                }
                else if (_tasksLength == 0)
                {
                    _tasks = ArrayPool<UniTask>.Shared.Rent(4);
                }
            }
            _tasks[_tasksLength++] = task;
        }

        /// <summary>
        /// 追加されたすべての<see cref="UniTask"/>が完了したときに完了する<see cref="UniTask"/>を構築して返す.
        /// </summary>
        public UniTask BuildAsync()
        {
            TaskTracker.TrackActiveTask(this, 3);

            if (_tasksLength == 0)
            {
                _core.TrySetResult(AsyncUnit.Default);
                return new UniTask(this, _core.Version); // GetResult will clean up.
            }

            for (int i = 0; i < _tasksLength; i++)
            {
                UniTask.Awaiter awaiter;
                try
                {
                    awaiter = _tasks[i].GetAwaiter();
                }
                catch (Exception ex)
                {
                    _core.TrySetException(ex);
                    continue;
                }

                if (awaiter.IsCompleted)
                {
                    TryInvokeContinuation(this, awaiter);
                }
                else
                {
                    awaiter.SourceOnCompleted(static state =>
                    {
                        var (self, awaiter) = (StateTuple<UniTaskBag, UniTask.Awaiter>)state;
                        TryInvokeContinuation(self, awaiter);
                    }, StateTuple.Create(this, awaiter));
                }
            }

            return new UniTask(this, _core.Version);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void TryInvokeContinuation(UniTaskBag self, in UniTask.Awaiter awaiter)
        {
            try
            {
                awaiter.GetResult();
            }
            catch (Exception ex)
            {
                self._core.TrySetException(ex);
                return;
            }

            if (Interlocked.Increment(ref self._completeCount) == self._tasksLength)
            {
                self._core.TrySetResult(AsyncUnit.Default);
            }
        }

        void IUniTaskSource.GetResult(short token)
        {
            TaskTracker.RemoveTracking(this);
            GC.SuppressFinalize(this);
            ArrayPool<UniTask>.Shared.Return(_tasks, clearArray: true);
            _core.GetResult(token);
        }

        UniTaskStatus IUniTaskSource.GetStatus(short token)
        {
            return _core.GetStatus(token);
        }

        UniTaskStatus IUniTaskSource.UnsafeGetStatus()
        {
            return _core.UnsafeGetStatus();
        }

        void IUniTaskSource.OnCompleted(Action<object> continuation, object state, short token)
        {
            _core.OnCompleted(continuation, state, token);
        }
    }
}

#endif