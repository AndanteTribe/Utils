using System.Runtime.CompilerServices;

namespace AndanteTribe.Utils;

/// <summary>
/// 文字列を扱うための静的メソッド群を提供します.
/// </summary>
public static partial class StringUtils
{
    /// <summary>
    /// 内部で<see cref="DefaultInterpolatedStringHandler"/>を使用した<see cref="string.Join(string,IEnumerable{string})"/>.
    /// </summary>
    /// <param name="separator">区切り文字列.</param>
    /// <param name="values">連結する文字列のコレクション.</param>
    /// <returns>連結された文字列.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="values"/>がnullの場合にスローされます.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string Join(in string? separator, IEnumerable<string?> values) => Join(separator.AsSpan(), values);

    /// <summary>
    /// 内部で<see cref="DefaultInterpolatedStringHandler"/>を使用した<see cref="string.Join{T}(char,IEnumerable{T})"/>.
    /// </summary>
    /// <param name="separator">区切り文字.</param>
    /// <param name="values">連結する文字列のコレクション.</param>
    /// <returns>連結された文字列.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="values"/>がnullの場合にスローされます.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string Join(in char separator, IEnumerable<string?> values) => Join(stackalloc char[]{ separator }, values);

    /// <summary>
    /// 内部で<see cref="DefaultInterpolatedStringHandler"/>を使用したstring.Join.
    /// </summary>
    /// <param name="separator">区切り文字列.</param>
    /// <param name="values">連結する文字列のコレクション.</param>
    /// <returns>連結された文字列.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="values"/>がnullの場合にスローされます.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string Join(in ReadOnlySpan<char> separator, IEnumerable<string?> values)
    {
        if (values == null)
        {
            throw new ArgumentNullException(nameof(values));
        }

        using var en = values.GetEnumerator();
        if (!en.MoveNext())
        {
            return "";
        }

        var firstValue = en.Current;

        if (!en.MoveNext())
        {
            return firstValue ?? "";
        }

        var result = new DefaultInterpolatedStringHandler();

        if (firstValue != null)
        {
            result.AppendLiteral(firstValue);
        }

        do
        {
            result.AppendFormatted(separator);
            if (en.Current != null)
            {
                result.AppendLiteral(en.Current);
            }
        }
        while (en.MoveNext());

        return result.ToString();
    }
}