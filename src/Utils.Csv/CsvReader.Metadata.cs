using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace AndanteTribe.Utils.Csv;

public partial class CsvReader
{
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

        return Type.GetTypeCode(type) switch
        {
            // 列挙体の型は基底型(デフォルトでint型)で出力してしまうので、最優先で処理する.
            _ when type.IsEnum => ReadEnumValue(type),
            TypeCode.Boolean => ReadBool(),
            TypeCode.Char => ReadChar(),
            TypeCode.SByte => ReadSbyte(),
            TypeCode.Byte => ReadByte(),
            TypeCode.Int16 => ReadShort(),
            TypeCode.UInt16 => ReadUshort(),
            TypeCode.Int32 => ReadInt(),
            TypeCode.UInt32 => ReadUint(),
            TypeCode.Int64 => ReadLong(),
            TypeCode.UInt64 => ReadUlong(),
            TypeCode.Single => ReadFloat(),
            TypeCode.Double => ReadDouble(),
            TypeCode.Decimal => ReadDecimal(),
            TypeCode.DateTime => ReadDateTime(),
            TypeCode.String => ReadString(),
            _ when type == typeof(DateTimeOffset) => ReadDateTimeOffset(),
            _ when type == typeof(TimeSpan) => ReadTimeSpan(),
            _ when type == typeof(Guid) => ReadGuid(),
            _ when type == typeof(Uri) => ReadUri(),
            _ when type == typeof(Version) => ReadVersion(),
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