using System.Buffers;

namespace AndanteTribe.Utils.GameServices;

/// <summary>
/// 軽量DIのレジスター実装.
/// </summary>
public struct Container : IDisposable
{
    private (Type type, object instance)[] _bindings;
    private int _count;

    /// <summary>
    /// デフォルトコンストラクタ.
    /// </summary>
    public Container()
    {
        _bindings = [];
    }

    /// <summary>
    /// オブジェクトをバインドする.
    /// </summary>
    /// <param name="instance">バインドするインスタンス.</param>
    /// <typeparam name="T">参照型.インターフェイスも登録可.nullは非許容.</typeparam>
    public void Bind<T>(T instance) where T : class
    {
        ArrayPool<(Type type, object instance)>.Shared.Grow(ref _bindings, _count + 1);
        _bindings[_count++] = (typeof(T), instance);
    }

    /// <summary>
    /// プロバイダーを構築し、内部状態をリセットする.
    /// </summary>
    /// <returns></returns>
    public Provider Build()
    {
        var (bindings, count) = (_bindings, _count);
        (_bindings, _count) = ([], 0);
        return new Provider(bindings, count);
    }

    /// <inheritdoc />
    void IDisposable.Dispose()
    {
        if (_bindings.Length != 0)
        {
            ArrayPool<(Type type, object instance)>.Shared.Return(_bindings);
            _bindings = [];
            _count = 0;
        }
    }
}