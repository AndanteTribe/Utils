using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using AndanteTribe.Utils.Internal;

namespace AndanteTribe.Utils;

/// <summary>
/// 文字列フォーマットのヘルパー.
/// </summary>
public static class FormatHelper
{
    /// <summary>
    /// フォーマット文字列を解析します.
    /// </summary>
    /// <param name="format"></param>
    /// <returns></returns>
    /// <exception cref="FormatException"></exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (string[] literal, (int index, string format)[] embed) AnalyzeFormat([StringSyntax(StringSyntaxAttribute.CompositeFormat)] in string format)
    {
        return AnalyzeFormat(format.AsSpan());
    }

    /// <summary>
    /// フォーマット文字列を解析します.
    /// </summary>
    /// <param name="format"></param>
    /// <returns></returns>
    /// <exception cref="FormatException"></exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (string[] literal, (int index, string format)[] embed) AnalyzeFormat(in ReadOnlySpan<char> format)
    {
        var literals = new ValueList<string>();
        var indices = new ValueList<(int index, string format)>();

        try
        {
            var pos = 0;
            while ((uint)pos < (uint)format.Length)
            {
                var remainder = format.Slice(pos);
                var countUntilNextBrace = remainder.IndexOfAny('{', '}');
                if (countUntilNextBrace < 0)
                {
                    literals.Add(remainder.ToString());
                    break;
                }

                if (remainder[countUntilNextBrace] == '}')
                {
                    throw new FormatException("開き中括弧がありません。");
                }

                literals.Add(remainder.Slice(0, countUntilNextBrace).ToString());

                pos += countUntilNextBrace;
                pos++;

                var endBrace = format.Slice(pos).IndexOf('}');
                if (endBrace < 0)
                {
                    throw new FormatException("閉じ中括弧がありません。");
                }

                var inside = format.Slice(pos, endBrace);
                var split = inside.IndexOf(':');
                var i = split < 0 ? inside : inside.Slice(0, split);
                var f = split < 0 ? ReadOnlySpan<char>.Empty : inside.Slice(split + 1);

                foreach (var c in i)
                {
                    if (!IsAsciiDigit(c))
                    {
                        throw new FormatException($"インデックス '{i}' が整数ではありません。");
                    }
                }

                indices.Add((int.Parse(i), f.ToString()));
                pos += endBrace + 1;
            }

            return (literals.Count == 0 ? [] : literals.AsSpan().ToArray(), indices.Count == 0 ? [] : indices.AsSpan().ToArray());
        }
        finally
        {
            literals.Clear();
            indices.Clear();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static bool IsAsciiDigit(char c) => (uint)(c - '0') <= (uint)('9' - '0');
    }
}