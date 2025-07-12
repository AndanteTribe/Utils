using System.Reflection;

#if !NET6_0_OR_GREATER

namespace AndanteTribe.Utils.BackPort.Internal;

internal delegate bool TryFormat<in T>(T value, in Span<char> destination, out int charsWritten, in ReadOnlySpan<char> format, IFormatProvider? provider);

internal static class CustomSpanFormatter
{
    private static readonly TryFormat<bool> s_boolean = TryFormat;
    private static readonly TryFormat<byte> s_byte = TryFormat;
    private static readonly TryFormat<DateTime> s_dateTime = TryFormat;
    private static readonly TryFormat<DateTimeOffset> s_dateTimeOffset = TryFormat;
    private static readonly TryFormat<double> s_double = TryFormat;
    private static readonly TryFormat<Guid> s_guid = TryFormat;
    private static readonly TryFormat<short> s_int16 = TryFormat;
    private static readonly TryFormat<int> s_int32 = TryFormat;
    private static readonly TryFormat<long> s_int64 = TryFormat;
    private static readonly TryFormat<sbyte> s_sByte = TryFormat;
    private static readonly TryFormat<float> s_single = TryFormat;
    private static readonly TryFormat<TimeSpan> s_timeSpan = TryFormat;
    private static readonly TryFormat<ushort> s_uInt16 = TryFormat;
    private static readonly TryFormat<uint> s_uInt32 = TryFormat;
    private static readonly TryFormat<ulong> s_uInt64 = TryFormat;
    private static readonly TryFormat<Version> s_version = TryFormat;
    private static readonly TryFormat<decimal> s_decimal = TryFormat;
    private static readonly TryFormat<System.Net.IPAddress> s_iPAddress = TryFormat;

    private static readonly RuntimeTypeHandle s_booleanTypeHandle = typeof(bool).TypeHandle;
    private static readonly RuntimeTypeHandle s_byteTypeHandle = typeof(byte).TypeHandle;
    private static readonly RuntimeTypeHandle s_dateTimeTypeHandle = typeof(DateTime).TypeHandle;
    private static readonly RuntimeTypeHandle s_dateTimeOffsetTypeHandle = typeof(DateTimeOffset).TypeHandle;
    private static readonly RuntimeTypeHandle s_doubleTypeHandle = typeof(double).TypeHandle;
    private static readonly RuntimeTypeHandle s_guidTypeHandle = typeof(Guid).TypeHandle;
    private static readonly RuntimeTypeHandle s_int16TypeHandle = typeof(short).TypeHandle;
    private static readonly RuntimeTypeHandle s_int32TypeHandle = typeof(int).TypeHandle;
    private static readonly RuntimeTypeHandle s_int64TypeHandle = typeof(long).TypeHandle;
    private static readonly RuntimeTypeHandle s_sByteTypeHandle = typeof(sbyte).TypeHandle;
    private static readonly RuntimeTypeHandle s_singleTypeHandle = typeof(float).TypeHandle;
    private static readonly RuntimeTypeHandle s_timeSpanTypeHandle = typeof(TimeSpan).TypeHandle;
    private static readonly RuntimeTypeHandle s_uInt16TypeHandle = typeof(ushort).TypeHandle;
    private static readonly RuntimeTypeHandle s_uInt32TypeHandle = typeof(uint).TypeHandle;
    private static readonly RuntimeTypeHandle s_uInt64TypeHandle = typeof(ulong).TypeHandle;
    private static readonly RuntimeTypeHandle s_versionTypeHandle = typeof(Version).TypeHandle;
    private static readonly RuntimeTypeHandle s_decimalTypeHandle = typeof(decimal).TypeHandle;
    private static readonly RuntimeTypeHandle s_iPAddressTypeHandle = typeof(System.Net.IPAddress).TypeHandle;

    private static readonly MethodInfo s_genericTryFormatMethod =
        typeof(CustomSpanFormatter).GetMethods(BindingFlags.NonPublic | BindingFlags.Static).First(static x => x.Name == nameof(TryFormat) && x.IsGenericMethod);

    public static IServiceProvider? OtherFormatterHelper { get; set; }

    private static class Cache<T>
    {
        public static readonly TryFormat<T>? Formatter;

        static Cache() => Formatter = GetDefaultHelper(typeof(T)) as TryFormat<T>;
    }

    public static TryFormat<T>? GetFormatter<T>() => Cache<T>.Formatter;

    private static object? GetDefaultHelper(Type type)
    {
        var t = type.TypeHandle;

        if (t.Equals(s_booleanTypeHandle))
        {
            return s_boolean;
        }
        if (t.Equals(s_byteTypeHandle))
        {
            return s_byte;
        }
        if (t.Equals(s_dateTimeTypeHandle))
        {
            return s_dateTime;
        }
        if (t.Equals(s_dateTimeOffsetTypeHandle))
        {
            return s_dateTimeOffset;
        }
        if (t.Equals(s_doubleTypeHandle))
        {
            return s_double;
        }
        if (t.Equals(s_guidTypeHandle))
        {
            return s_guid;
        }
        if (t.Equals(s_int16TypeHandle))
        {
            return s_int16;
        }
        if (t.Equals(s_int32TypeHandle))
        {
            return s_int32;
        }
        if (t.Equals(s_int64TypeHandle))
        {
            return s_int64;
        }
        if (t.Equals(s_sByteTypeHandle))
        {
            return s_sByte;
        }
        if (t.Equals(s_singleTypeHandle))
        {
            return s_single;
        }
        if (t.Equals(s_timeSpanTypeHandle))
        {
            return s_timeSpan;
        }
        if (t.Equals(s_uInt16TypeHandle))
        {
            return s_uInt16;
        }
        if (t.Equals(s_uInt32TypeHandle))
        {
            return s_uInt32;
        }
        if (t.Equals(s_uInt64TypeHandle))
        {
            return s_uInt64;
        }
        if (t.Equals(s_versionTypeHandle))
        {
            return s_version;
        }
        if (t.Equals(s_decimalTypeHandle))
        {
            return s_decimal;
        }
        if (t.Equals(s_iPAddressTypeHandle))
        {
            return s_iPAddress;
        }

        if (typeof(ISpanFormattable).IsAssignableFrom(type))
        {
            return Delegate.CreateDelegate(typeof(TryFormat<>).MakeGenericType(type), s_genericTryFormatMethod.MakeGenericMethod(type));
        }

        var formatter = OtherFormatterHelper?.GetService(type);
        return formatter;
    }

    private static bool TryFormat(bool value, in Span<char> destination, out int charsWritten, in ReadOnlySpan<char> _, IFormatProvider? __) =>
        value.TryFormat(destination, out charsWritten);

    private static bool TryFormat(byte value, in Span<char> destination, out int charsWritten, in ReadOnlySpan<char> format, IFormatProvider? provider) =>
        value.TryFormat(destination, out charsWritten, format, provider);

    private static bool TryFormat(DateTime value, in Span<char> destination, out int charsWritten, in ReadOnlySpan<char> format, IFormatProvider? provider) =>
        value.TryFormat(destination, out charsWritten, format, provider);

    private static bool TryFormat(DateTimeOffset value, in Span<char> destination, out int charsWritten, in ReadOnlySpan<char> format, IFormatProvider? provider) =>
        value.TryFormat(destination, out charsWritten, format, provider);

    private static bool TryFormat(double value, in Span<char> destination, out int charsWritten, in ReadOnlySpan<char> format, IFormatProvider? provider) =>
        value.TryFormat(destination, out charsWritten, format, provider);

    private static bool TryFormat(Guid value, in Span<char> destination, out int charsWritten, in ReadOnlySpan<char> format, IFormatProvider? _) =>
        value.TryFormat(destination, out charsWritten, format);

    private static bool TryFormat(short value, in Span<char> destination, out int charsWritten, in ReadOnlySpan<char> format, IFormatProvider? provider) =>
        value.TryFormat(destination, out charsWritten, format, provider);

    private static bool TryFormat(int value, in Span<char> destination, out int charsWritten, in ReadOnlySpan<char> format, IFormatProvider? provider) =>
        value.TryFormat(destination, out charsWritten, format, provider);

    private static bool TryFormat(long value, in Span<char> destination, out int charsWritten, in ReadOnlySpan<char> format, IFormatProvider? provider) =>
        value.TryFormat(destination, out charsWritten, format, provider);

    private static bool TryFormat(sbyte value, in Span<char> destination, out int charsWritten, in ReadOnlySpan<char> format, IFormatProvider? provider) =>
        value.TryFormat(destination, out charsWritten, format, provider);

    private static bool TryFormat(float value, in Span<char> destination, out int charsWritten, in ReadOnlySpan<char> format, IFormatProvider? provider) =>
        value.TryFormat(destination, out charsWritten, format, provider);

    private static bool TryFormat(TimeSpan value, in Span<char> destination, out int charsWritten, in ReadOnlySpan<char> format, IFormatProvider? provider) =>
        value.TryFormat(destination, out charsWritten, format, provider);

    private static bool TryFormat(ushort value, in Span<char> destination, out int charsWritten, in ReadOnlySpan<char> format, IFormatProvider? provider) =>
        value.TryFormat(destination, out charsWritten, format, provider);

    private static bool TryFormat(uint value, in Span<char> destination, out int charsWritten, in ReadOnlySpan<char> format, IFormatProvider? provider) =>
        value.TryFormat(destination, out charsWritten, format, provider);

    private static bool TryFormat(ulong value, in Span<char> destination, out int charsWritten, in ReadOnlySpan<char> format, IFormatProvider? provider) =>
        value.TryFormat(destination, out charsWritten, format, provider);

    private static bool TryFormat(Version value, in Span<char> destination, out int charsWritten, in ReadOnlySpan<char> _, IFormatProvider? __) =>
        value.TryFormat(destination, out charsWritten);

    private static bool TryFormat(decimal value, in Span<char> destination, out int charsWritten, in ReadOnlySpan<char> format, IFormatProvider? provider) =>
        value.TryFormat(destination, out charsWritten, format, provider);

    private static bool TryFormat(System.Net.IPAddress value, in Span<char> destination, out int charsWritten, in ReadOnlySpan<char> _, IFormatProvider? __) =>
        value.TryFormat(destination, out charsWritten);

    [Preserve]
    private static bool TryFormat<T>(T value, in Span<char> destination, out int charsWritten, in ReadOnlySpan<char> format, IFormatProvider? provider) where T : ISpanFormattable =>
        value.TryFormat(destination, out charsWritten, format, provider);
}

#endif