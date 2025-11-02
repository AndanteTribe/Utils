using System.Buffers;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace AndanteTribe.Utils;

/// <summary>
/// 構造体簡易リスト.
/// </summary>
/// <param name="capacity">初期容量.</param>
/// <typeparam name="T">要素の型.</typeparam>
public struct ValueList<T>(int capacity) : IReadOnlyCollection<T>
{
    private T[] _items =  ArrayPool<T>.Shared.Rent(Math.Max(1, capacity));

    /// <summary>
    /// 要素数.
    /// </summary>
    public int Count { get; private set; }

    /// <inheritdoc />
    public ValueList() : this(16)
    {
    }

    /// <summary>
    /// 要素を追加します.
    /// </summary>
    /// <param name="item"></param>
    public void Add(T item)
    {
        if (Count >= _items.Length)
        {
            var newItems = ArrayPool<T>.Shared.Rent(_items.Length * 2);
            _items.AsSpan().CopyTo(newItems);
            ArrayPool<T>.Shared.Return(_items);
            _items = newItems;
        }
        _items[Count++] = item;
    }

    /// <summary>
    /// 配列として取得し、配列プールに返却する.
    /// </summary>
    /// <returns></returns>
    public readonly T[] ToArrayAndClear()
    {
        var result = AsSpan().ToArray();
        ArrayPool<T>.Shared.Return(_items);
        return result;
    }

    /// <summary>
    /// 配列プールに返却する.
    /// </summary>
    /// <param name="segment"></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Clear(in ArraySegment<T> segment)
    {
        if (segment.Array is { Length: > 0 })
        {
            ArrayPool<T>.Shared.Return(segment.Array);
        }
    }

    /// <summary>
    /// 配列プールに返却する.
    /// </summary>
    /// <param name="memory"></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Clear(in ReadOnlyMemory<T> memory)
    {
        if (memory.Length > 0 && MemoryMarshal.TryGetArray(memory, out var segment))
        {
            ArrayPool<T>.Shared.Return(segment.Array!);
        }
    }

    /// <summary>
    /// <see cref="ArraySegment{T}"/>として取得します.
    /// </summary>
    /// <returns></returns>
    public readonly ArraySegment<T> AsSegment() => new(_items, 0, Count);

    /// <summary>
    /// <see cref="Span{T}"/>として取得します.
    /// </summary>
    /// <returns></returns>
    public readonly ReadOnlySpan<T> AsSpan() => new ReadOnlySpan<T>(_items, 0, Count);

    /// <summary>
    /// <see cref="Memory{T}"/>として取得します.
    /// </summary>
    /// <returns></returns>
    public readonly ReadOnlyMemory<T> AsMemory() => new ReadOnlyMemory<T>(_items, 0, Count);

    /// <summary>
    /// 列挙子を取得します.
    /// </summary>
    /// <returns></returns>
    public readonly MemoryExtensions.Enumerator<T> GetEnumerator() => AsMemory().GetEnumerator();

    /// <inheritdoc />
    readonly IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();

    /// <inheritdoc />
    readonly IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}