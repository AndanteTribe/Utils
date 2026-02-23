using System.Runtime.CompilerServices;

namespace AndanteTribe.Utils.GameServices;

/// <summary>
/// マスターID.
/// </summary>
/// <remarks>
/// 検索ロジック等、一番使うことになる型だと思うので、できる限りパフォーマンスよいコードを目指す.
/// </remarks>
/// <param name="Id">ID.</param>
/// <param name="Group">グループ.</param>
/// <typeparam name="TGroup"></typeparam>
public readonly record struct MasterId<TGroup>(TGroup Group, uint Id)
    : IEquatable<MasterId<TGroup>>, IComparable<MasterId<TGroup>>, ISpanFormattable where TGroup : unmanaged, Enum
{
    private int GroupId
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            var group = Group;
            return Unsafe.As<TGroup, int>(ref group);
        }
    }

    /// <summary>
    /// タプルからの暗黙的変換.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static implicit operator MasterId<TGroup>(in ValueTuple<TGroup, uint> value) => new(value.Item1, value.Item2);

    /// <inheritdoc />
    public bool Equals(MasterId<TGroup> other) => Id == other.Id && GroupId == other.GroupId;

    /// <inheritdoc />
    public int CompareTo(MasterId<TGroup> other) => (this, other) switch
    {
        var (a, b) when a.GroupId < b.GroupId || a.Id < b.Id => -1,
        var (a, b) when a.GroupId > b.GroupId || a.Id > b.Id => 1,
        _ => 0
    };

    /// <inheritdoc />
    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
    {
        var group = Group.ToString();

        if (!group.AsSpan().TryCopyTo(destination))
        {
            charsWritten = 0;
            return false;
        }
        charsWritten = group.Length;

        if (charsWritten >= destination.Length)
        {
            charsWritten = 0;
            return false;
        }
        destination[charsWritten++] = '.';

        // Id部分はフォーマット対応している
        if (!Id.TryFormat(destination[charsWritten..], out var idCharsWritten, format.IsEmpty ? "0000" : format, provider))
        {
            charsWritten = 0;
            return false;
        }
        charsWritten += idCharsWritten;
        return true;
    }

    /// <inheritdoc />
    public string ToString(string? format, IFormatProvider? formatProvider) => GetStringInternal(format, formatProvider);

    /// <inheritdoc />
    public override string ToString() => GetStringInternal(ReadOnlySpan<char>.Empty, null);

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(Id, GroupId);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private string GetStringInternal(in ReadOnlySpan<char> format, IFormatProvider? formatProvider)
    {
        var bufferLength = 16;
        var result = "";
        while (!TryGetStr(this, format, formatProvider, bufferLength, out result))
        {
            bufferLength *= 2;
        }
        return result;

        static bool TryGetStr(in MasterId<TGroup> instance, in ReadOnlySpan<char> format, IFormatProvider? formatProvider, int bufferLength, out string result)
        {
            var buffer = (Span<char>)stackalloc char[bufferLength];
            if (instance.TryFormat(buffer, out var written, format, formatProvider))
            {
                result = buffer[..written].ToString();
                return true;
            }
            result = "";
            return false;
        }
    }
}