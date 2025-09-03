using System.Buffers;
using System.Collections;

namespace AndanteTribe.Utils;

/// <summary>
/// 構造体簡易リスト.
/// </summary>
/// <param name="capacity">初期容量.</param>
/// <typeparam name="T">要素の型.</typeparam>
public struct ValueList<T>(int capacity) : IReadOnlyCollection<T>
{
    private T[] _items = ArrayPool<T>.Shared.Rent(capacity);

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
        if (_items.Length != 0)
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
    }

    /// <summary>
    /// 内部配列を<see cref="ArraySegment{T}"/>として取得し、クリアします.
    /// </summary>
    /// <returns></returns>
    public ArraySegment<T> GetSegmentAndClear()
    {
        var segment = new ArraySegment<T>(_items, 0, Count);
        _items = [];
        Count = 0;
        return segment;
    }

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