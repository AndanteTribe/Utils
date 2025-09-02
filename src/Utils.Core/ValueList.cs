using System.Buffers;
using System.Collections;

namespace AndanteTribe.Utils;

/// <summary>
/// 構造体簡易リスト.
/// </summary>
/// <param name="capacity">初期容量.</param>
/// <typeparam name="T">要素の型.</typeparam>
public struct ValueList<T>(int capacity) : IReadOnlyCollection<T>, IDisposable
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
    /// <see cref="ArraySegment{T}"/>として取得します.
    /// </summary>
    /// <returns></returns>
    public readonly ArraySegment<T> AsSegment() => new ArraySegment<T>(_items, 0, Count);

    /// <summary>
    /// 列挙子を取得します.
    /// </summary>
    /// <returns></returns>
    public readonly ArraySegment<T>.Enumerator GetEnumerator() => AsSegment().GetEnumerator();

    /// <inheritdoc />
    readonly IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();

    /// <inheritdoc />
    readonly IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <inheritdoc />
    void IDisposable.Dispose()
    {
        if (_items.Length > 0)
        {
            ArrayPool<T>.Shared.Return(_items);
            _items = [];
            Count = 0;
        }
    }
}