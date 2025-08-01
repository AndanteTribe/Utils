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

}