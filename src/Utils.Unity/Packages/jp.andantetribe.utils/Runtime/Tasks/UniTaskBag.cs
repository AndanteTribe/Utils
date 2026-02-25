#if ENABLE_UNITASK
#nullable enable

using System;
using System.Buffers;
using Cysharp.Threading.Tasks;

namespace AndanteTribe.Utils.Unity.Tasks
{
    /// <summary>
    /// 複数の<see cref="UniTask"/>を集め、それら全てが完了した時に完成するバッグ.
    /// </summary>
    /// <remarks>
    /// 複数の<see cref="UniTask"/>を並列待機する挙動は<see cref="UniTask.WhenAll(UniTask[])"/>と同じ.
    /// </remarks>
    /// <example>
    /// <code>
    /// <![CDATA[
    /// using System;
    /// using AndanteTribe.Utils.Unity;
    /// using Cysharp.Threading.Tasks;
    /// using UnityEngine;
    ///
    /// public class Example : MonoBehaviour
    /// {
    ///     private async UniTaskVoid Start()
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
    ///         await using (var bag = new UniTaskBag())
    ///         {
    ///             for (int i = 0; i < 10; i++)
    ///             {
    ///                 bag.Add(UniTask.Delay(TimeSpan.FromSeconds(i)));
    ///             }
    ///         }
    ///     }
    /// }
    /// ]]>
    /// </code>
    /// </example>
    public struct UniTaskBag : IUniTaskAsyncDisposable
    {
        private UniTask[]? _tasks;
        private int _count;

        /// <summary>
        /// <see cref="UniTask"/>をバッグに追加する.
        /// </summary>
        /// <param name="task">追加する<see cref="UniTask"/>.</param>
        public void Add(UniTask task)
        {
            if (_tasks == null)
            {
                _tasks = ArrayPool<UniTask>.Shared.Rent(1);
            }
            else
            {
                ArrayPool<UniTask>.Shared.Grow(ref _tasks, _count + 1);
            }

            _tasks[_count++] = task;
        }

        /// <inheritdoc />
        public UniTask DisposeAsync()
        {
            if (_tasks == null)
            {
                return UniTask.CompletedTask;
            }

            try
            {
                return UniTask.WhenAll(_tasks);
            }
            finally
            {
                _tasks.AsSpan(0, _count).Clear();
                ArrayPool<UniTask>.Shared.Return(_tasks);
            }
        }
    }
}

#endif
