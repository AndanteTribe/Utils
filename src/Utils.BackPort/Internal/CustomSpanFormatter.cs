#if !NET6_0_OR_GREATER

using System.Reflection;

namespace AndanteTribe.Utils.BackPort.Internal;

internal delegate bool TryFormat<in T>(T value, in Span<char> destination, out int charsWritten, in ReadOnlySpan<char> format, IFormatProvider? provider);

internal static class CustomSpanFormatter
{
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
            return new TryFormat<bool>(static (bool value, in Span<char> destination, out int written, in ReadOnlySpan<char> _, IFormatProvider? _) =>
                value.TryFormat(destination, out written));
        }
        if (t.Equals(s_byteTypeHandle))
        {
            return new TryFormat<byte>(static (byte value, in Span<char> destination, out int written, in ReadOnlySpan<char> format, IFormatProvider? provider) =>
                value.TryFormat(destination, out written, format, provider));
        }
        if (t.Equals(s_dateTimeTypeHandle))
        {
            return new TryFormat<DateTime>(static (DateTime value, in Span<char> destination, out int written, in ReadOnlySpan<char> format, IFormatProvider? provider) =>
                value.TryFormat(destination, out written, format, provider));
        }
        if (t.Equals(s_dateTimeOffsetTypeHandle))
        {
            return new TryFormat<DateTimeOffset>(static (DateTimeOffset value, in Span<char> destination, out int written, in ReadOnlySpan<char> format, IFormatProvider? provider) =>
                value.TryFormat(destination, out written, format, provider));
        }
        if (t.Equals(s_doubleTypeHandle))
        {
            return new TryFormat<double>(static (double value, in Span<char> destination, out int written, in ReadOnlySpan<char> format, IFormatProvider? provider) =>
                value.TryFormat(destination, out written, format, provider));
        }
        if (t.Equals(s_guidTypeHandle))
        {
            return new TryFormat<Guid>(static (Guid value, in Span<char> destination, out int written, in ReadOnlySpan<char> format, IFormatProvider? _) =>
                value.TryFormat(destination, out written, format));
        }
        if (t.Equals(s_int16TypeHandle))
        {
            return new TryFormat<short>(static (short value, in Span<char> destination, out int written, in ReadOnlySpan<char> format, IFormatProvider? provider) =>
                value.TryFormat(destination, out written, format, provider));
        }
        if (t.Equals(s_int32TypeHandle))
        {
            return new TryFormat<int>(static (int value, in Span<char> destination, out int written, in ReadOnlySpan<char> format, IFormatProvider? provider) =>
                value.TryFormat(destination, out written, format, provider));
        }
        if (t.Equals(s_int64TypeHandle))
        {
            return new TryFormat<long>(static (long value, in Span<char> destination, out int written, in ReadOnlySpan<char> format, IFormatProvider? provider) =>
                value.TryFormat(destination, out written, format, provider));
        }
        if (t.Equals(s_sByteTypeHandle))
        {
            return new TryFormat<sbyte>(static (sbyte value, in Span<char> destination, out int written, in ReadOnlySpan<char> format, IFormatProvider? provider) =>
                value.TryFormat(destination, out written, format, provider));
        }
        if (t.Equals(s_singleTypeHandle))
        {
            return new TryFormat<float>(static (float value, in Span<char> destination, out int written, in ReadOnlySpan<char> format, IFormatProvider? provider) =>
                value.TryFormat(destination, out written, format, provider));
        }
        if (t.Equals(s_timeSpanTypeHandle))
        {
            return new TryFormat<TimeSpan>(static (TimeSpan value, in Span<char> destination, out int written, in ReadOnlySpan<char> format, IFormatProvider? provider) =>
                value.TryFormat(destination, out written, format, provider));
        }
        if (t.Equals(s_uInt16TypeHandle))
        {
            return new TryFormat<ushort>(static (ushort value, in Span<char> destination, out int written, in ReadOnlySpan<char> format, IFormatProvider? provider) =>
                value.TryFormat(destination, out written, format, provider));
        }
        if (t.Equals(s_uInt32TypeHandle))
        {
            return new TryFormat<uint>(static (uint value, in Span<char> destination, out int written, in ReadOnlySpan<char> format, IFormatProvider? provider) =>
                value.TryFormat(destination, out written, format, provider));
        }
        if (t.Equals(s_uInt64TypeHandle))
        {
            return new TryFormat<ulong>(static (ulong value, in Span<char> destination, out int written, in ReadOnlySpan<char> format, IFormatProvider? provider) =>
                value.TryFormat(destination, out written, format, provider));
        }
        if (t.Equals(s_versionTypeHandle))
        {
            return new TryFormat<Version>(static (Version value, in Span<char> destination, out int written, in ReadOnlySpan<char> _, IFormatProvider? _) =>
                value.TryFormat(destination, out written));
        }
        if (t.Equals(s_decimalTypeHandle))
        {
            return new TryFormat<decimal>(static (decimal value, in Span<char> destination, out int written, in ReadOnlySpan<char> format, IFormatProvider? provider) =>
                value.TryFormat(destination, out written, format, provider));
        }
        if (t.Equals(s_iPAddressTypeHandle))
        {
            return new TryFormat<System.Net.IPAddress>(static (System.Net.IPAddress value, in Span<char> destination, out int written, in ReadOnlySpan<char> _, IFormatProvider? _) =>
                value.TryFormat(destination, out written));
        }

        if (typeof(ISpanFormattable).IsAssignableFrom(type))
        {
            return Delegate.CreateDelegate(typeof(TryFormat<>).MakeGenericType(type), s_genericTryFormatMethod.MakeGenericMethod(type));
        }

        var formatter = OtherFormatterHelper?.GetService(type);
        return formatter;
    }


    [Preserve]
    private static bool TryFormat<T>(T value, in Span<char> destination, out int charsWritten, in ReadOnlySpan<char> format, IFormatProvider? provider) where T : ISpanFormattable =>
        value.TryFormat(destination, out charsWritten, format, provider);
}

#endif