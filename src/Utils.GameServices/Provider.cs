using System.Buffers;

namespace AndanteTribe.Utils.GameServices;

/// <summary>
/// 軽量DIのProvider実装.
/// </summary>
public readonly record struct Provider : IDisposable
{
    private readonly (Type type, object instance)[] _bindings;
    private readonly int _count;

    /// <summary>
    /// 空のプロバイダー.
    /// </summary>
    public static readonly Provider Empty = new(Array.Empty<(Type type, object instance)>(), 0);

    /// <summary>
    /// バインドされたオブジェクト群.
    /// </summary>
    public ReadOnlySpan<(Type type, object instance)> Bindings => new(_bindings, 0, _count);

    internal Provider((Type type, object instance)[] bindings, int count)
    {
        _bindings = bindings;
        _count = count;
    }

    /// <inheritdoc />
    void IDisposable.Dispose()
    {
        if (_bindings.Length != 0)
        {
            ArrayPool<(Type type, object instance)>.Shared.Return(_bindings);
        }
    }
}