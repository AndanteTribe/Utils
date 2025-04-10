#if ENABLE_R3
#nullable enable

using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using R3;

namespace AndanteTribe.Utils.R3
{
    /// <summary>
    /// <see cref="ReactiveProperty{T}"/>の拡張メソッド.
    /// </summary>
    public static partial class ObservableExtensions
    {
        /// <summary>
        /// 指定した<see cref="ReactiveProperty{T}"/>の値が変更されるまで待機します.
        /// </summary>
        /// <param name="source">監視対象の<see cref="ReactiveProperty{T}"/>.</param>
        /// <param name="predicate">条件を満たすかどうかを判定する処理.</param>
        /// <param name="cancellationToken">キャンセルトークン.</param>
        /// <typeparam name="T">監視対象の<see cref="ReactiveProperty{T}"/>の型.</typeparam>
        /// <returns>監視対象の<see cref="ReactiveProperty{T}"/>の値が変更されたことを示す<see cref="Task"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task WaitValueChangedAsync<T>(this ReadOnlyReactiveProperty<T> source, Func<T, bool> predicate, in CancellationToken cancellationToken = default)
        {
            return source.Skip(1).FirstAsync(predicate, cancellationToken);
        }

        /// <summary>
        /// 指定した<see cref="ReactiveProperty{T}"/>の値が変更されるまで待機します.
        /// </summary>
        /// <param name="source">監視対象の<see cref="ReactiveProperty{T}"/>.</param>
        /// <param name="cancellationToken">キャンセルトークン.</param>
        /// <typeparam name="T">監視対象の<see cref="ReactiveProperty{T}"/>の型.</typeparam>
        /// <returns>監視対象の<see cref="ReactiveProperty{T}"/>の値が変更されたことを示す<see cref="Task"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task WaitValueChangedAsync<T>(this ReadOnlyReactiveProperty<T> source, in CancellationToken cancellationToken = default)
        {
            return source.Skip(1).FirstAsync(static _ => true, cancellationToken);
        }
    }
}
#endif