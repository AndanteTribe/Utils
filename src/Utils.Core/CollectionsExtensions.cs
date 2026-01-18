using System.Buffers;
using System.Runtime.CompilerServices;

namespace AndanteTribe.Utils;

/// <summary>
/// <see cref="System.Collections.Generic"/>または<see cref="System.Linq"/>または<see cref="System.Buffers.ArrayPool{T}"/>周りの追加拡張メソッド群.
/// </summary>
public static class CollectionsExtensions
{
    /// <summary>
    /// <see cref="List{T}"/>を<see cref="Span{T}"/>に変換します.
    /// </summary>
    /// <example>
    /// <code>
    /// <![CDATA[
    /// using System.Runtime.CompilerServices;
    ///
    /// public class Example
    /// {
    ///    public static void Main(string[] args)
    ///    {
    ///        List<int> numbers = new List<int> { 10, 20, 30, 40, 50 };
    ///
    ///        // List<int> を Span<int> に変換
    ///        Span<int> numbersSpan = numbers.AsSpan();
    ///
    ///        // List<T>よりも高速にループ処理可能
    ///        foreach (var item in numbersSpan)
    ///        {
    ///             Console.WriteLine(item);
    ///        }
    ///
    ///    }
    /// }
    /// ]]>
    /// </code>
    /// </example>
    /// <param name="list"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Span<T> AsSpan<T>(this List<T> list) =>
        System.Runtime.InteropServices.CollectionsMarshal.AsSpan(list);

    /// <summary>
    /// <see cref="ArrayPool{T}"/>から借りた配列を返却するためのハンドルを取得します.
    /// </summary>
    /// <remarks>
    /// 基本的に<see langword="using"/>ステートメントと組み合わせて使用します.
    /// </remarks>
    /// <example>
    /// <code>
    /// <![CDATA[
    /// using System;
    /// using System.Buffers;
    ///
    /// public class Example
    /// {
    ///    public static void Main()
    ///    {
    ///        // ArrayPool<int>.Shared を直接使って Rent する例
    ///        using (var handle = ArrayPool<int>.Shared.Rent(10, out int[] array))
    ///        {
    ///            // 必要サイズだけ Span を作る（借りた配列は必ず minimumLength 以上の長さがある）
    ///            var span = array.AsSpan(0, 10);
    ///            for (int i = 0; i < span.Length; i++)
    ///            {
    ///                span[i] = i;
    ///            }
    ///            Console.WriteLine(string.Join(",", span.ToArray()));
    ///        } // スコープを抜けると Dispose が呼ばれて配列が返却される
    ///
    ///    }
    /// }
    /// ]]>
    /// </code>
    /// </example>
    /// <param name="pool"></param>
    /// <param name="minimumLength"></param>
    /// <param name="array"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Handle<T> Rent<T>(this ArrayPool<T> pool, int minimumLength, out T[] array) =>
        new(pool, array = pool.Rent(minimumLength));

    /// <summary>
    /// <see cref="ArrayPool{T}"/>から借りた配列を返却するためのハンドル.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public readonly struct Handle<T> : IDisposable
    {
        private readonly ArrayPool<T> _pool;
        private readonly T[] _array;

        internal Handle(ArrayPool<T> pool, T[] array)
        {
            _pool = pool;
            _array = array;
        }

        /// <inheritdoc/>
        void IDisposable.Dispose() => _pool.Return(_array);
    }
}