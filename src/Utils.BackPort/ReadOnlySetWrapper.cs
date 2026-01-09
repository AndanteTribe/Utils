using System.Collections;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace System.Collections.Generic;

#if !NET5_0_OR_GREATER

/// <summary>
/// Provides a read-only wrapper for a HashSet.
/// </summary>
/// <typeparam name="T">The type of elements in the set.</typeparam>
public sealed class ReadOnlySetWrapper<T> : IReadOnlySet<T>
{
    private readonly HashSet<T> _set;

    public ReadOnlySetWrapper(HashSet<T> set)
    {
        _set = set;
    }

    public int Count => _set.Count;

    public bool Contains(T item) => _set.Contains(item);

    public bool IsProperSubsetOf(IEnumerable<T> other) => _set.IsProperSubsetOf(other);

    public bool IsProperSupersetOf(IEnumerable<T> other) => _set.IsProperSupersetOf(other);

    public bool IsSubsetOf(IEnumerable<T> other) => _set.IsSubsetOf(other);

    public bool IsSupersetOf(IEnumerable<T> other) => _set.IsSupersetOf(other);

    public bool Overlaps(IEnumerable<T> other) => _set.Overlaps(other);

    public bool SetEquals(IEnumerable<T> other) => _set.SetEquals(other);

    public IEnumerator<T> GetEnumerator() => _set.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

#endif
