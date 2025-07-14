#if !NET5_0_OR_GREATER

using System.Runtime.CompilerServices;

namespace System.Runtime.InteropServices;

/// <summary>
/// An unsafe class that provides a set of methods to access the underlying data representations of collections.
/// </summary>
public static class CollectionsMarshal
{
    /// <summary>
    /// Get a <see cref="Span{T}"/> view over a <see cref="List{T}"/>'s data.
    /// Items should not be added or removed from the <see cref="List{T}"/> while the <see cref="Span{T}"/> is in use.
    /// </summary>
    /// <param name="list">The list to get the data view over.</param>
    /// <typeparam name="T">The type of the elements in the list.</typeparam>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Span<T> AsSpan<T>(List<T>? list) =>
        list == null ? default : Unsafe.As<List<T>, ListDummy<T>>(ref list).Items.AsSpan(0, list.Count);

    private sealed class ListDummy<T>
    {
        public T[] Items = null!;
    }
}

#endif