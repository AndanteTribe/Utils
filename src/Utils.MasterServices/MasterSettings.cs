using MasterMemory;
using MasterMemory.Meta;
using MessagePack;

namespace AndanteTribe.Utils.MasterServices;

/// <summary>
/// マスターコンバート設定.
/// </summary>
public record MasterSettings
{
    /// <summary>
    /// 入力ディレクトリパス.
    /// </summary>
    public readonly string InputDirectoryPath;

    /// <summary>
    /// マスターメタデータ.
    /// </summary>
    public readonly MetaDatabase Metadata;

    /// <summary>
    /// マスタービルダークラスの型.
    /// </summary>
    public readonly Type BuilderType;

    /// <summary>
    /// ローカライズ対応している最大言語数.
    /// </summary>
    public readonly uint MaxLanguageCount = 1;

    /// <summary>
    /// ローカライズ出力する言語インデックス.
    /// </summary>
    public readonly uint LanguageIndex = 0;

    /// <summary>
    /// MessagePackのカスタムフォーマッタリゾルバを指定します.
    /// </summary>
    public IFormatterResolver? CustomResolver { get; init; }

    private MasterSettings(string inputDirectoryPath, MetaDatabase metadata, Type builderType, IFormatterResolver? customResolver)
    {
        InputDirectoryPath = inputDirectoryPath;
        Metadata = metadata;
        BuilderType = builderType;
        CustomResolver = customResolver;
    }

    private MasterSettings(string inputDirectoryPath, MetaDatabase metadata, Type builderType, uint maxLanguageCount, uint languageIndex, IFormatterResolver? customResolver)
    {
        InputDirectoryPath = inputDirectoryPath;
        Metadata = metadata;
        BuilderType = builderType;
        MaxLanguageCount = maxLanguageCount;
        LanguageIndex = languageIndex;
    }

    /// <summary>
    /// マスターコンバート設定を作成します.
    /// </summary>
    /// <param name="inputDirectoryPath"></param>
    /// <param name="metadata"></param>
    /// <param name="customResolver"></param>
    /// <typeparam name="TBuilder"></typeparam>
    /// <returns></returns>
    public static MasterSettings Create<TBuilder>(string inputDirectoryPath, MetaDatabase metadata, IFormatterResolver? customResolver = null) where TBuilder : DatabaseBuilderBase
    {
        return new MasterSettings(inputDirectoryPath, metadata, typeof(TBuilder), customResolver);
    }

    /// <summary>
    /// マスターコンバート設定を作成します(ローカライズ対応).
    /// </summary>
    /// <param name="inputDirectoryPath"></param>
    /// <param name="metadata"></param>
    /// <param name="maxLanguageCount"></param>
    /// <param name="languageIndex"></param>
    /// <param name="customResolver"></param>
    /// <typeparam name="TBuilder"></typeparam>
    /// <returns></returns>
    public static MasterSettings Create<TBuilder>(string inputDirectoryPath, MetaDatabase metadata, uint maxLanguageCount, uint languageIndex, IFormatterResolver? customResolver = null) where TBuilder : DatabaseBuilderBase
    {
        return new MasterSettings(inputDirectoryPath, metadata, typeof(TBuilder), maxLanguageCount, languageIndex, customResolver);
    }
}