using MasterMemory;
using MasterMemory.Meta;

namespace AndanteTribe.Utils.MasterServices;

/// <summary>
/// マスターコンバート設定.
/// </summary>
/// <param name="InputDirectoryPath">入力ディレクトリパス.</param>
/// <param name="Metadata">マスターメタデータ.</param>
/// <param name="BuilderFactory">データベースビルダーファクトリ.</param>
public record MasterSettings(string InputDirectoryPath, MetaDatabase Metadata, Func<DatabaseBuilderBase> BuilderFactory)
{
    /// <summary>
    /// ローカライズ対応している最大言語数.
    /// </summary>
    public uint MaxLanguageCount { get; init; } = 1;

    /// <summary>
    /// ローカライズ出力する言語インデックス.
    /// </summary>
    public uint LanguageIndex { get; init; } = 0;
}