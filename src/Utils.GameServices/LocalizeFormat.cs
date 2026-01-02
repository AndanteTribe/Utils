using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using MessagePack;

namespace AndanteTribe.Utils.GameServices;

/// <summary>
/// ローカライズ用のフォーマット形式キャッシュ.
/// </summary>
[MessagePackObject(AllowPrivate = true)]
public partial record LocalizeFormat
{
    [Key(0)]
    private readonly string[] _literal;

    [Key(1)]
    private readonly (int index, string format)[] _embed;

    [Key(2)]
    internal readonly int _literalLength;

    /// <summary>
    /// リテラル部分の配列.
    /// </summary>
    [IgnoreMember]
    public ReadOnlySpan<string> Literal => _literal.AsSpan();

    /// <summary>
    /// 埋め込み部分の配列.
    /// </summary>
    [IgnoreMember]
    public ReadOnlySpan<(int index, string format)> Embed => _embed.AsSpan();

    /// <summary>
    /// Initialize a new instance of <see cref="LocalizeFormat"/>.
    /// </summary>
    /// <param name="literal"></param>
    /// <param name="embed"></param>
    /// <param name="literalLength"></param>
    internal LocalizeFormat(string[] literal, (int index, string format)[] embed, int literalLength)
    {
        _literal = literal;
        _embed = embed;
        _literalLength = literalLength;
    }

    /// <summary>
    /// フォーマット文字列を解析して<see cref="LocalizeFormat"/>を生成します.
    /// </summary>
    /// <param name="format"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LocalizeFormat Parse([StringSyntax(StringSyntaxAttribute.CompositeFormat)]in string format)
    {
        var (literal, embed) = FormatHelper.AnalyzeFormat(format.AsSpan());
        var literalLength = 0;
        foreach (var l in literal.AsSpan())
        {
            literalLength += l.Length;
        }
        return new LocalizeFormat(literal, embed, literalLength);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        if (_embed.Length == 0)
        {
            return _literal[0];
        }

        // -----以降、デバッグ用-----
        // 元のフォーマット文字列を表示
        var initialBuffer = (Span<char>)stackalloc char[256];
        // デバッグ用途なので_literalLength分の指定が雑
        var sb = new DefaultInterpolatedStringHandler(_literalLength, _embed.Length, null, initialBuffer);

        for (var i = 0; i < _embed.Length; i++)
        {
            sb.AppendLiteral(_literal[i]);
            sb.AppendLiteral("{");
            sb.AppendFormatted(_embed[i].index);
            if (!string.IsNullOrEmpty(_embed[i].format))
            {
                sb.AppendLiteral(":");
                sb.AppendLiteral(_embed[i].format);
            }
            sb.AppendLiteral("}");
        }

        if (_literal.Length > _embed.Length)
        {
            sb.AppendLiteral(_literal[^1]);
        }
        return sb.ToStringAndClear();
    }
}