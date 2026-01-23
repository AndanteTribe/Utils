# 利用上のポイント集
[Utils.MasterServices](https://www.nuget.org/packages/AndanteTribe.Utils.MasterServices) の解説記事だが、詳細なAPIはAPIドキュメントを参照することを推奨する。

## マスターCSVコンバート実装方法
内部的に [Utils.Csv](https://www.nuget.org/packages/AndanteTribe.Utils.Csv) を使った、CSVマスターのコンバート実装だが、利用者側で実装するコードは多くない．  
挙動として、自動的に実行環境のコア数などを参照して、並列実行で高速にコンバート作業を進行する．

まず [MasterMemory](https://github.com/Cysharp/MasterMemory) を活用してマスターテーブルを構築する実装をする．  
このときに、[Utils.FileNameAttribute](https://andantetribe.github.io/Utils/api/AndanteTribe.Utils.FileNameAttribute.html)で該当するCSVファイルを指定する．

```cs
[MemoryTable(nameof(TextMasterEntity)), MessagePackObject]
[FileName("text.csv")] // ファイル名を属性で紐づけする：:*.csvの拡張子はあってもなくてもよい
public class TextMasterEntity
{
    [PrimaryKey, Key(0)]
    public required MasterId<TextCategory> Id { get; init; }

    [SecondaryKey(0), NonUnique, IgnoreMember]
    public TextCategory Group => Id.Group;

    [Key(1)]
    public required LocalizeFormat Format { get; init; }

    [IgnoreMember]
    public string Text => Format.ToString();
}
```

あとは以下のようなコードを書けば、ひとまず指定したディレクトリにマスターバイナリを出力する．

```cs
// 設定定義.
var settings = new MasterSettings(
    "../../master",                     // マスターのCSV群があるディレクトリパス.
    MemoryDatabase.GetMetaDatabase(),   // MasterMemoryが生やすコードにある、メタデータコレクション.
    static () => new DatabaseBuilder()); // これもMasterMemoryが生やす実装のオブジェクトが内部的に必要なので渡す.

MasterConverter.Build(settings, "../../bin"); // 第一引数に設定定義, 第二引数に出力ディレクトリパスを渡せば完了．
```

基本は上記のコードで十分だが、
- ローカライズ対応
- マスター出力暗号化
- ファイル出力せず、メモリ展開してテストなどを実行

などをしたい場合は、APIリファレンスなどを参照して対応すること．

> [!Tip]
> [MasterConverter.GetAllCharacters](https://andantetribe.github.io/Utils/api/AndanteTribe.Utils.MasterServices.MasterConverter.html#AndanteTribe_Utils_MasterServices_MasterConverter_GetAllCharacters_AndanteTribe_Utils_MasterServices_MasterSettings_)というものも提供している．
> これはマスターをコンバートして、マスターに含まれた文字を重複なくスタックして返す、というもの．
> UnityのTextMeshProのフォントアセットに取得した文字だけ登録できればアセットデータをかなり削減できるなど、使い道がいろいろと考えられる便利メソッドになる．
