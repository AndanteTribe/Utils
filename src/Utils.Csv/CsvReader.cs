using System.Globalization;
using System.Runtime.CompilerServices;

namespace AndanteTribe.Utils.Csv;

/// <summary>
/// CSVリーダー.
/// </summary>
/// <remarks>
/// 1. ヘッダー対応済み
/// 2. #コメント行対応済み(行頭が#の行はスキップ)
/// 3. #コメント列対応済み(#で始まるヘッダーはスキップ e.g. #Comment)
/// </remarks>
public sealed partial class CsvReader : IDisposable
{
    private readonly StreamReader _reader;
    private readonly string[] _header;

    /// <summary>
    /// csvのヘッダー.
    /// </summary>
    public ReadOnlySpan<string> Header => _header.AsSpan();

    private string _line;
    private int _column;
    private int _consumed;

    /// <summary>
    /// Initialize a new instance of <see cref="CsvReader"/>.
    /// </summary>
    /// <param name="stream"></param>
    /// <exception cref="InvalidDataException"></exception>
    public CsvReader(Stream stream) : this(new StreamReader(stream))
    {
    }

    /// <summary>
    /// Initialize a new instance of <see cref="CsvReader"/>.
    /// </summary>
    /// <param name="reader"></param>
    /// <exception cref="InvalidDataException"></exception>
    public CsvReader(StreamReader reader)
    {
        _reader = reader;
        _line = "";
        _column = 0;
        _consumed = 0;
        _header = ReadHeaderLine(_reader).ToArray();
    }

    /// <summary>
    /// 改行まで読み込む. コメント行(#で始まる行)はスキップする.
    /// </summary>
    /// <returns></returns>
    public bool ReadLine()
    {
        var line = "";
        do
        {
            // HACK: EOF判定まわりで、末尾に改行コードがない行読み込みをするとReadLineAsyncだとスタックしてしまう.
            if (string.IsNullOrWhiteSpace(line = _reader.ReadLine()))
            {
                return false;
            }
        } while (line.StartsWith('#')); // #で始まる行はスキップ

        _line = line;
        _column = 0;
        _consumed = 0;
        return true;
    }

    /// <summary>
    /// <see cref="bool"/>値を読み込む.
    /// </summary>
    /// <returns>結果.</returns>
    public bool ReadBool()
    {
        var value = ReadRawValue();
        // 0,1 or True/False
        return int.TryParse(value, out var intBool) ? Convert.ToBoolean(intBool) : bool.Parse(value);
    }

    /// <summary>
    /// <see cref="char"/>値を読み込む.
    /// </summary>
    /// <returns>結果.</returns>
    /// <exception cref="InvalidDataException">無効な値.長さが1未満など.</exception>
    public char ReadChar()
    {
        var value = ReadRawValue();
        return value.Length == 1 ? value[0] : throw new InvalidDataException($"Invalid char value: {value.ToString()}");
    }

    /// <summary>
    /// <see cref="sbyte"/>値を読み込む.
    /// </summary>
    /// <returns>結果.</returns>
    public sbyte ReadSbyte() => sbyte.Parse(ReadRawValue(), provider: CultureInfo.InvariantCulture);

    /// <summary>
    /// <see cref="byte"/>値を読み込む.
    /// </summary>
    /// <returns>結果.</returns>
    public byte ReadByte() => byte.Parse(ReadRawValue(), provider: CultureInfo.InvariantCulture);

    /// <summary>
    /// <see cref="short"/>値を読み込む.
    /// </summary>
    /// <returns></returns>
    public short ReadShort() => short.Parse(ReadRawValue(), provider: CultureInfo.InvariantCulture);

    /// <summary>
    /// <see cref="ushort"/>値を読み込む.
    /// </summary>
    /// <returns>結果.</returns>
    public ushort ReadUshort() => ushort.Parse(ReadRawValue(), provider: CultureInfo.InvariantCulture);

    /// <summary>
    /// <see cref="int"/>値を読み込む.
    /// </summary>
    /// <returns>結果.</returns>
    public int ReadInt() => int.Parse(ReadRawValue(), provider: CultureInfo.InvariantCulture);

    /// <summary>
    /// <see cref="uint"/>値を読み込む.
    /// </summary>
    /// <returns>結果.</returns>
    public uint ReadUint() => uint.Parse(ReadRawValue(), provider: CultureInfo.InvariantCulture);

    /// <summary>
    /// <see cref="long"/>値を読み込む.
    /// </summary>
    /// <returns>結果.</returns>
    public long ReadLong() => long.Parse(ReadRawValue(), provider: CultureInfo.InvariantCulture);

    /// <summary>
    /// <see cref="ulong"/>値を読み込む.
    /// </summary>
    /// <returns>結果.</returns>
    public ulong ReadUlong() => ulong.Parse(ReadRawValue(), provider: CultureInfo.InvariantCulture);

    /// <summary>
    /// <see cref="float"/>値を読み込む.
    /// </summary>
    /// <returns>結果.</returns>
    public float ReadFloat() => float.Parse(ReadRawValue(), provider: CultureInfo.InvariantCulture);

    /// <summary>
    /// <see cref="double"/>値を読み込む.
    /// </summary>
    /// <returns>結果.</returns>
    public double ReadDouble() => double.Parse(ReadRawValue(), provider: CultureInfo.InvariantCulture);

    /// <summary>
    /// <see cref="decimal"/>値を読み込む.
    /// </summary>
    /// <returns>結果.</returns>
    public decimal ReadDecimal() => decimal.Parse(ReadRawValue(), provider: CultureInfo.InvariantCulture);

    /// <summary>
    /// <see cref="DateTime"/>値を読み込む.
    /// </summary>
    /// <returns>結果.</returns>
    public DateTime ReadDateTime() => DateTime.Parse(ReadRawValue(), CultureInfo.InvariantCulture);

    /// <summary>
    /// <see cref="DateTimeOffset"/>値を読み込む.
    /// </summary>
    /// <returns>結果.</returns>
    public DateTimeOffset ReadDateTimeOffset() => DateTimeOffset.Parse(ReadRawValue(), CultureInfo.InvariantCulture);

    /// <summary>
    /// <see cref="TimeSpan"/>値を読み込む.
    /// </summary>
    /// <returns>結果.</returns>
    public TimeSpan ReadTimeSpan() => TimeSpan.Parse(ReadRawValue(), CultureInfo.InvariantCulture);

    /// <summary>
    /// <see cref="Guid"/>値を読み込む.
    /// </summary>
    /// <returns>結果.</returns>
    public Guid ReadGuid() => Guid.Parse(ReadRawValue());

    /// <summary>
    /// <see cref="Version"/>値を読み込む.
    /// </summary>
    /// <returns>結果.</returns>
    public Version ReadVersion() => Version.Parse(ReadRawValue());

    /// <summary>
    /// <see cref="Uri"/>値を読み込む.
    /// </summary>
    /// <param name="uriKind">URI文字列が相対URI、絶対URI、または不確定であるかを指定する.</param>
    /// <returns>結果.</returns>
    public Uri ReadUri(UriKind uriKind = UriKind.Absolute) => new Uri(ReadRawValue().ToString(), uriKind);

    /// <summary>
    /// 文字列を読み込む.
    /// </summary>
    /// <returns>結果.</returns>
    public string ReadString() => ReadRawValue().ToString();

    /// <summary>
    /// 生の値を読み込む.
    /// </summary>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlySpan<char> ReadRawValue()
    {
        // Headerの#で始まる列はスキップする
        var start = 0;
        var size = 0;
        do
        {
            start = _consumed;
            size = GetRawValue(_line, ref _consumed).Length;
        } while (_column < Header.Length && Header[_column++].StartsWith('#'));
        return _line.AsSpan(start, size);
    }

    /// <summary>
    /// 生の値をピークする.
    /// </summary>
    /// <returns></returns>
    public ReadOnlySpan<char> PeekRawValue()
    {
        var tempConsumed = _consumed;
        var tempColumn = _column;

        // Headerの#で始まる列はスキップする
        var start = 0;
        var size = 0;
        do
        {
            start = tempConsumed;
            size = GetRawValue(_line, ref tempConsumed).Length;
        } while (tempColumn < Header.Length && Header[tempColumn++].StartsWith('#'));
        return _line.AsSpan(start, size);
    }

    /// <inheritdoc />
    void IDisposable.Dispose() => _reader.Dispose();

    /// <summary>
    /// ヘッダー行を読み込む.
    /// </summary>
    /// <param name="stream"></param>
    /// <returns></returns>
    /// <exception cref="InvalidDataException"></exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IEnumerable<string> ReadHeaderLine(Stream stream) => ReadHeaderLine(new StreamReader(stream));

    /// <summary>
    /// ヘッダー行を読み込む.
    /// </summary>
    /// <param name="reader"></param>
    /// <returns></returns>
    /// <exception cref="InvalidDataException"></exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IEnumerable<string> ReadHeaderLine(StreamReader reader)
    {
        var line = "";
        do
        {
            if (string.IsNullOrWhiteSpace(line = reader.ReadLine()))
            {
                throw new InvalidDataException("Header line is empty.");
            }
        } while (line.StartsWith('#')); // #で始まる行はスキップ

        var index = 0;

        while (index < line.Length)
        {
            var s = GetRawValue(line, ref index);
            yield return s.IsEmpty ? "" : s.ToString();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ReadOnlySpan<char> GetRawValue(ReadOnlySpan<char> line, ref int i)
    {
        var size = line[i..].IndexOf(',');
        size = size == -1 ? line.Length - i : size;
        var temp = line.Slice(i, size).Trim();
        i += size + (i + size < line.Length && line[i + size] == ',' ? 1 : 0);
        return temp;
    }
}