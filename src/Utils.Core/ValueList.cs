using System.Buffers;
using System.Collections;
using System.Collections.Generic;

namespace AndanteTribe.Utils;

/// <summary>
/// 構造体簡易リスト.
/// </summary>
/// <typeparam name="T">要素の型.</typeparam>
public struct ValueList<T> : IReadOnlyCollection<T>, IDisposable
{
    private T[] _items;

    /// <summary>
    /// 要素数.
    /// </summary>
    public int Count { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValueList{T}"/> struct.
    /// </summary>
    /// <param name="capacity">初期容量.</param>
    public ValueList(int capacity)
    {
        _items = ArrayPool<T>.Shared.Rent(capacity);
        Count = 0;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValueList{T}"/> struct with default capacity.
    /// </summary>
    public ValueList() : this(16)
    {
    }

    /// <summary>
    /// 要素を追加します.
    /// </summary>
    /// <param name="item"></param>
    public void Add(T item)
    {
        if (_items == null)
        {
            _items = ArrayPool<T>.Shared.Rent(16);
        }
        
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
    public readonly ArraySegment<T> AsSegment() => _items != null ? new ArraySegment<T>(_items, 0, Count) : new ArraySegment<T>();

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
        if (_items != null && _items.Length > 0)
        {
            ArrayPool<T>.Shared.Return(_items);
            _items = null!;
            Count = 0;
        }
    }
}