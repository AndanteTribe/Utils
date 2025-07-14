using System.Runtime.CompilerServices;

namespace AndanteTribe.Utils;

/// <summary>
/// <see cref="System.Collections.Generic"/>または<see cref="System.Linq"/>周りの追加拡張メソッド群.
/// </summary>
public static class CollectionsExtensions
{
    /// <summary>
    /// <see cref="List{T}"/>を<see cref="Span{T}"/>に変換します.
    /// </summary>
    /// <param name="list"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Span<T> AsSpan<T>(this List<T> list) =>
        System.Runtime.InteropServices.CollectionsMarshal.AsSpan(list);
}