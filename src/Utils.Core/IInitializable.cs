namespace AndanteTribe.Utils;

/// <summary>
/// 初期化処理を提供するインターフェース.
/// </summary>
public interface IInitializable
{
    /// <summary>
    /// 非同期で初期化処理を行う.
    /// </summary>
    /// <param name="cancellationToken">キャンセル用のトークン.</param>
    /// <returns>初期化が完了したら完了するタスク.</returns>
    ValueTask InitializeAsync(CancellationToken cancellationToken);
}