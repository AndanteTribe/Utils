using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using AndanteTribe.Utils.Csv;
using AndanteTribe.Utils.GameServices;
using MessagePack;

namespace AndanteTribe.Utils.MasterServices;

internal static class CsvReaderExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static object? ReadObject(this LocalizeCsvReader reader, PropertyInfo info, uint languageIndex)
    {
        if (info.SetMethod == null)
        {
            throw new NotSupportedException($"対象プロパティにセッターがありません。{{ get; init; }}を検討してください, Type:{info.DeclaringType} Prop:{info.Name}");
        }

        // ローカライズ対応.
        // LocalizeFormatもMessagePackObjectなので、MessagePackObjectAttributeより前に処理.
        if (reader.TryGetLocalizeObject(info, languageIndex, out var value))
        {
            return value;
        }

        var type = info.PropertyType;

        // マスターIDだけセル一つで書けるように特別対応. e.g. "Air.0001"
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(MasterId<>))
        {
            return reader.ReadMasterId(type);
        }
        if (type.IsDefined(typeof(MessagePackObjectAttribute)))
        {
            return reader.ReadMessagePackObject(type, languageIndex);
        }

        return reader.ReadDynamic(type);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static object ReadMasterId(this CsvReader reader, Type type)
    {
        var rawValue = reader.ReadRawValue();
        var dotIndex = rawValue.IndexOf('.');
        if (dotIndex == -1)
        {
            throw new FormatException($"MasterIDのフォーマットが不正です。{rawValue.ToString()}");
        }

#if NET8_0_OR_GREATER
        var obj = RuntimeHelpers.GetUninitializedObject(type);
#else
        var obj = FormatterServices.GetUninitializedObject(type);
#endif

        // Group
        var groupProp = type.GetProperty("Group")!;
        groupProp.SetValue(obj, Enum.Parse(groupProp.PropertyType, rawValue[..dotIndex].ToString()));

        // Id
        var idProp = type.GetProperty("Id")!;
        idProp.SetValue(obj, uint.Parse(rawValue[(dotIndex + 1)..].ToString()));

        return obj;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static object ReadMessagePackObject(this LocalizeCsvReader reader, Type type, uint languageIndex)
    {
        // 現状、StringKey使う方式は対応していない
        var props = type.GetProperties()
            .OrderBy(static x => x.GetCustomAttribute<KeyAttribute>()?.IntKey ?? int.MaxValue);
#if NET8_0_OR_GREATER
        var obj = RuntimeHelpers.GetUninitializedObject(type);
#else
        var obj = FormatterServices.GetUninitializedObject(type);
#endif
        foreach (var prop in props)
        {
            var value = reader.ReadObject(prop, languageIndex);
            prop.SetValue(obj, value);
        }
        return obj;
    }
}