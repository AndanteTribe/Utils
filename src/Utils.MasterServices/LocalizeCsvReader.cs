using System.Reflection;
using AndanteTribe.Utils.Csv;
using AndanteTribe.Utils.GameServices;

namespace AndanteTribe.Utils.MasterServices;

/// <summary>
/// ローカライズ対応CSVリーダー.
/// </summary>
public class LocalizeCsvReader : CsvReader
{
    private static readonly RuntimeTypeHandle s_stringTypeHandle = typeof(string).TypeHandle;
    private static readonly RuntimeTypeHandle s_localizeFormatTypeHandle = typeof(LocalizeFormat).TypeHandle;

    /// <summary>
    /// 最大言語数.
    /// </summary>
    public uint MaxLanguageCount { get; init; } = 1;

    /// <summary>
    /// Initialize a new instance of <see cref="LocalizeCsvReader"/>.
    /// </summary>
    /// <param name="stream"></param>
    public LocalizeCsvReader(Stream stream) : base(stream)
    {
    }

    /// <summary>
    /// Initialize a new instance of <see cref="LocalizeCsvReader"/>.
    /// </summary>
    /// <param name="reader"></param>
    public LocalizeCsvReader(StreamReader reader) : base(reader)
    {
    }

    /// <summary>
    /// 指定した言語インデックスのローカライズ文字列を読み込みます.
    /// </summary>
    /// <param name="languageIndex">言語インデックス(1以上).</param>
    /// <returns>ローカライズ文字列.</returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public string ReadLocalizedString(uint languageIndex)
    {
        if (languageIndex > MaxLanguageCount)
        {
            throw new ArgumentOutOfRangeException(nameof(languageIndex));
        }

        // skip other language columns
        for (var i = 0; i < languageIndex; i++)
        {
            _ = ReadRawValue();
        }

        var value = ReadString();

        // skip other language columns
        for (var i = languageIndex + 1; i < MaxLanguageCount; i++)
        {
            _ = ReadRawValue();
        }

        return value;
    }

    /// <summary>
    /// 指定したプロパティがローカライズ対応オブジェクトかどうかを判定し、対応していれば値を取得します.
    /// </summary>
    /// <param name="info"></param>
    /// <param name="languageIndex"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public bool TryGetLocalizeObject(PropertyInfo info, uint languageIndex, out object? value)
    {
        var type = info.PropertyType;

        if (type.TypeHandle.Equals(s_stringTypeHandle) && info.IsDefined(typeof(LocalizedMemberAttribute), false))
        {
            value = ReadLocalizedString(languageIndex);
            return true;
        }
        if (type.TypeHandle.Equals(s_localizeFormatTypeHandle))
        {
            var format = ReadLocalizedString(languageIndex);
            value = LocalizeFormat.Parse(format);
            return true;
        }

        value = null;
        return false;
    }
}