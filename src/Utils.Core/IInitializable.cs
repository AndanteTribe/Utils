namespace AndanteTribe.Utils;

/// <summary>
/// 初期化処理を提供するインターフェース.
/// </summary>
/// <exception cref="NotImplementedException">
/// <see cref="IInitializable"/>を実装する場合、<see cref="Initialize"/>または<see cref="InitializeAsync"/>のいずれかを実装する必要があります.
/// </exception>
public interface IInitializable
{
    /// <summary>
    /// 同期的に初期化処理を行う.
    /// </summary>
    /// <param name="cancellationToken">キャンセル用のトークン.</param>
    void Initialize(CancellationToken cancellationToken) =>
        throw new NotImplementedException("IInitializableを実装する場合,Initialize(CancellationToken)またはInitializeAsync(CancellationToken)のいずれかを実装する必要があります.");

    /// <summary>
    /// 非同期で初期化処理を行う.
    /// </summary>
    /// <param name="cancellationToken">キャンセル用のトークン.</param>
    /// <returns>初期化が完了したら完了するタスク.</returns>
    ValueTask InitializeAsync(CancellationToken cancellationToken)
    {
        Initialize(cancellationToken);
        return default;
    }
}