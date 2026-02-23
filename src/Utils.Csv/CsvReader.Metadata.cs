using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace AndanteTribe.Utils.Csv;

public partial class CsvReader
{
    private readonly RuntimeTypeHandle _boolTypeHandle = typeof(bool).TypeHandle;
    private readonly RuntimeTypeHandle _charTypeHandle = typeof(char).TypeHandle;
    private readonly RuntimeTypeHandle _sbyteTypeHandle = typeof(sbyte).TypeHandle;
    private readonly RuntimeTypeHandle _byteTypeHandle = typeof(byte).TypeHandle;
    private readonly RuntimeTypeHandle _shortTypeHandle = typeof(short).TypeHandle;
    private readonly RuntimeTypeHandle _ushortTypeHandle = typeof(ushort).TypeHandle;
    private readonly RuntimeTypeHandle _intTypeHandle = typeof(int).TypeHandle;
    private readonly RuntimeTypeHandle _uintTypeHandle = typeof(uint).TypeHandle;
    private readonly RuntimeTypeHandle _longTypeHandle = typeof(long).TypeHandle;
    private readonly RuntimeTypeHandle _ulongTypeHandle = typeof(ulong).TypeHandle;
    private readonly RuntimeTypeHandle _floatTypeHandle = typeof(float).TypeHandle;
    private readonly RuntimeTypeHandle _doubleTypeHandle = typeof(double).TypeHandle;
    private readonly RuntimeTypeHandle _decimalTypeHandle = typeof(decimal).TypeHandle;
    private readonly RuntimeTypeHandle _dateTimeTypeHandle = typeof(DateTime).TypeHandle;
    private readonly RuntimeTypeHandle _stringTypeHandle = typeof(string).TypeHandle;
    private readonly RuntimeTypeHandle _dateTimeOffsetTypeHandle = typeof(DateTimeOffset).TypeHandle;
    private readonly RuntimeTypeHandle _timeSpanTypeHandle = typeof(TimeSpan).TypeHandle;
    private readonly RuntimeTypeHandle _guidTypeHandle = typeof(Guid).TypeHandle;
    private readonly RuntimeTypeHandle _uriTypeHandle = typeof(Uri).TypeHandle;
    private readonly RuntimeTypeHandle _versionTypeHandle = typeof(Version).TypeHandle;
    private readonly RuntimeTypeHandle _enumTypeHandle = typeof(Enum).TypeHandle;

    /// <summary>
    /// 型情報から値を読み込む.
    /// </summary>
    /// <remarks>
    /// 列挙体の場合、<see cref="EnumMemberAttribute"/>で指定された名前も考慮してパースを試みる.
    /// またフラグ列挙体の場合、"Hoge_Fuga_Piyo"のようにアンダースコア区切りで複数指定された場合も対応する.
    /// </remarks>
    /// <param name="type"></param>
    /// <returns></returns>
    /// <exception cref="NotSupportedException"></exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public object? ReadDynamic(Type type)
    {
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            if (PeekRawValue().IsWhiteSpace())
            {
                return null;
            }

            type = type.GenericTypeArguments[0];
        }

        return type.TypeHandle switch
        {
            // 列挙体の型は基底型(デフォルトでint型)で出力してしまうので、最優先で処理する.
            _ when type.BaseType?.TypeHandle.Equals(_enumTypeHandle) == true => ReadEnumValue(type),
            _ when type.TypeHandle.Equals(_boolTypeHandle) => ReadBool(),
            _ when type.TypeHandle.Equals(_charTypeHandle) => ReadChar(),
            _ when type.TypeHandle.Equals(_sbyteTypeHandle) => ReadSbyte(),
            _ when type.TypeHandle.Equals(_byteTypeHandle) => ReadByte(),
            _ when type.TypeHandle.Equals(_shortTypeHandle) => ReadShort(),
            _ when type.TypeHandle.Equals(_ushortTypeHandle) => ReadUshort(),
            _ when type.TypeHandle.Equals(_intTypeHandle) => ReadInt(),
            _ when type.TypeHandle.Equals(_uintTypeHandle) => ReadUint(),
            _ when type.TypeHandle.Equals(_longTypeHandle) => ReadLong(),
            _ when type.TypeHandle.Equals(_ulongTypeHandle) => ReadUlong(),
            _ when type.TypeHandle.Equals(_floatTypeHandle) => ReadFloat(),
            _ when type.TypeHandle.Equals(_doubleTypeHandle) => ReadDouble(),
            _ when type.TypeHandle.Equals(_decimalTypeHandle) => ReadDecimal(),
            _ when type.TypeHandle.Equals(_dateTimeTypeHandle) => ReadDateTime(),
            _ when type.TypeHandle.Equals(_stringTypeHandle) => ReadString(),
            _ when type.TypeHandle.Equals(_dateTimeOffsetTypeHandle) => ReadDateTimeOffset(),
            _ when type.TypeHandle.Equals(_timeSpanTypeHandle) => ReadTimeSpan(),
            _ when type.TypeHandle.Equals(_guidTypeHandle) => ReadGuid(),
            _ when type.TypeHandle.Equals(_uriTypeHandle) => ReadUri(),
            _ when type.TypeHandle.Equals(_versionTypeHandle) => ReadVersion(),
            _ => throw new NotSupportedException($"The type '{type.FullName}' is not supported.")
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private object ReadEnumValue(Type type)
    {
        var rawValue = ReadRawValue();

        // フラグ非対応列挙体.
        if (!type.IsDefined(typeof(FlagsAttribute)))
        {
#if NET6_0_OR_GREATER
            if (Enum.TryParse(type, rawValue, out var single))
#else
            if (Enum.TryParse(type, rawValue.ToString(), out var single))
#endif
            {
                return single;
            }

            foreach (var prop in type.GetFields().AsSpan())
            {
                if (prop.GetCustomAttribute<EnumMemberAttribute>()?.Value is { } attrValue && rawValue.SequenceEqual(attrValue))
                {
                    return Enum.Parse(type, prop.Name);
                }
            }

            // ここに到達している場合はパース失敗なので、詳しい不可条件をthrowするべくもう一度パースを呼ぶ.
#if NET6_0_OR_GREATER
            return Enum.Parse(type, rawValue);
#else
            return Enum.Parse(type, rawValue.ToString());
#endif
        }

        // "Hoge_Fuga_Piyo" -> "Hoge,Fuga,Piyo"
        // HACK: csv文脈でカンマ区切りは途中処理でもやりたくないが、Enum.Parseの仕様のため、やむを得ず置換する.
        var value = (Span<char>)stackalloc char[rawValue.Length];
        for (var i = 0; i < rawValue.Length; i++)
        {
            value[i] = rawValue[i] == '_' ? ',' : rawValue[i];
        }
#if NET6_0_OR_GREATER
        if (Enum.TryParse(type, value, out var result))
#else
        if (Enum.TryParse(type, value.ToString(), out var result))
#endif
        {
            return result!;
        }

        // Enum.Parseに失敗した場合、EnumMemberAttributeで定義されている名前を試す.
        var builder = new DefaultInterpolatedStringHandler(value.Length, 0);
        foreach (var prop in type.GetFields().AsSpan())
        {
            var attr = prop.GetCustomAttribute<EnumMemberAttribute>();
            if (attr != null && value.IndexOf(attr.Value) != -1)
            {
                builder.AppendLiteral(prop.Name);
                builder.AppendLiteral(",");
            }
        }
        var str = builder.ToStringAndClear().TrimEnd(',');
        return Enum.Parse(type, str);
    }
}