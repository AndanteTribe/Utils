using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace AndanteTribe.Utils;

internal static class RandomCache
{
#if NET6_0_OR_GREATER
    public static Random Shared { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => Random.Shared; }
#else
    public static Random Shared { get; } = new();
#endif
}

/// <summary>
/// メモリ改ざん対策構造体.
/// </summary>
/// <remarks>
/// 基本的にスコープ内でキャストして使う.
/// 本体はコレクション周りの利用で面倒がないように<see cref="Equals"/>や<see cref="CompareTo"/>などは公開しているが、それ以上は何もできない.
/// </remarks>
public readonly struct Obscured<T> : IEquatable<Obscured<T>>, IComparable<Obscured<T>> where T : unmanaged
{
    internal readonly T _hiddenValue;
    internal readonly T _key;

    /// <summary>
    /// Initializes a new instance of the <see cref="Obscured{T}"/> struct.
    /// </summary>
    public Obscured()
    {
        _key = GenerateKey();
        _hiddenValue = Xor(default, _key);
    }

    private Obscured(T value)
    {
        _key = GenerateKey();
        _hiddenValue = Xor(value, _key);
    }

    internal Obscured(T hiddenValue, T key)
    {
        _hiddenValue = hiddenValue;
        _key = key;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static T GenerateKey()
    {
        var buffer = (Span<byte>)stackalloc byte[Unsafe.SizeOf<T>()];
        RandomCache.Shared.NextBytes(buffer);
        return MemoryMarshal.Read<T>(buffer);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static T Xor(T value, T key)
    {
        var v = MemoryMarshal.AsBytes(MemoryMarshal.CreateSpan(ref value, 1));
        var k = MemoryMarshal.AsBytes(MemoryMarshal.CreateSpan(ref key, 1));
        for (var i = 0; i < v.Length; i++)
        {
            v[i] ^= k[i];
        }
        return MemoryMarshal.Read<T>(v);
    }

    /// <summary>
    /// <see cref="Obscured{T}"/>から<see cref="T"/>への暗黙的変換.
    /// </summary>
    /// <param name="obscured"></param>
    /// <returns></returns>
    public static implicit operator T(Obscured<T> obscured) => Xor(obscured._hiddenValue, obscured._key);

    /// <summary>
    /// <see cref="T"/>から<see cref="Obscured{T}"/>への暗黙的変換.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static implicit operator Obscured<T>(T value) => new(value);

    /// <inheritdoc />
    public bool Equals(Obscured<T> other) =>
        EqualityComparer<T>.Default.Equals(Xor(_hiddenValue, _key), Xor(other._hiddenValue, other._key));

    /// <inheritdoc />
    public int CompareTo(Obscured<T> other) =>
        Comparer<T>.Default.Compare(Xor(_hiddenValue, _key), Xor(other._hiddenValue, other._key));

    /// <inheritdoc />
    public override int GetHashCode() => EqualityComparer<T>.Default.GetHashCode(Xor(_hiddenValue, _key));
}