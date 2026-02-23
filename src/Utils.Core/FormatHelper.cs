using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

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
        var literalsBuffer = Array.Empty<string>();
        var indicesBuffer = Array.Empty<(int index, string format)>();
        var (literalCount, indexCount) = (0, 0);

        try
        {
            var pos = 0;
            while ((uint)pos < (uint)format.Length)
            {
                var remainder = format.Slice(pos);
                var countUntilNextBrace = remainder.IndexOfAny('{', '}');
                if (countUntilNextBrace < 0)
                {
                    ArrayPool<string>.Shared.Grow(ref literalsBuffer, literalCount + 1);
                    literalsBuffer[literalCount++] = remainder.ToString();
                    break;
                }

                if (remainder[countUntilNextBrace] == '}')
                {
                    throw new FormatException("開き中括弧がありません。");
                }

                ArrayPool<string>.Shared.Grow(ref literalsBuffer, literalCount + 1);
                literalsBuffer[literalCount++] = remainder[..countUntilNextBrace].ToString();

                pos += countUntilNextBrace;
                pos++;

                var endBrace = format[pos..].IndexOf('}');
                if (endBrace < 0)
                {
                    throw new FormatException("閉じ中括弧がありません。");
                }

                var inside = format.Slice(pos, endBrace);
                var split = inside.IndexOf(':');
                var i = split < 0 ? inside : inside[..split];
                var f = split < 0 ? ReadOnlySpan<char>.Empty : inside[(split + 1)..];

                foreach (var c in i)
                {
                    if (!IsAsciiDigit(c))
                    {
                        throw new FormatException($"インデックス '{i}' が整数ではありません。");
                    }
                }

                ArrayPool<(int index, string format)>.Shared.Grow(ref indicesBuffer, indexCount + 1);
                indicesBuffer[indexCount++] = (int.Parse(i), f.ToString());
                pos += endBrace + 1;
            }

            return (literalCount == 0 ? [] : literalsBuffer.AsSpan(0, literalCount).ToArray(), indexCount == 0 ? [] : indicesBuffer.AsSpan(0, indexCount).ToArray());
        }
        finally
        {
            ArrayPool<string>.Shared.Return(literalsBuffer);
            ArrayPool<(int index, string format)>.Shared.Return(indicesBuffer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static bool IsAsciiDigit(char c) => (uint)(c - '0') <= (uint)('9' - '0');
    }
}