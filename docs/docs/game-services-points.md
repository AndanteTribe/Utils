# 利用上のポイント集
[Utils.GameServices](https://www.nuget.org/packages/AndanteTribe.Utils.GameServices) の解説記事だが、詳細なAPIはAPIドキュメントを参照することを推奨する。

## マスターID
`MasterId` はその名の通り、マスターデータで利用するIDを実装したオブジェクト．  
IDは通常、ユニーク、いわゆる一意性を求められる．  
つまり、同じ値が発生しないような構造にしなければならない．  

そのうえで、マスターのようなデータテーブル構造のIDは、ある程度の表現能力を求められる．  
単なる数字のIDでも運用はできるだろうが、接頭辞がつく構造のほうがカテゴライズして羅列することができるようになる．  
`MasterId` はマスターデータ構築に便利なライブラリ [MasterMemory](https://github.com/Cysharp/MasterMemory) と併用することを念頭に置き、接頭辞としての列挙体1つと数字（uint型）を保持する構造で提供されている．

```cs
// 列挙体を必ず定義
// 毎回定義するのは面倒なようで、そこまで手間ではない、はず
public enum TextCategory : int
{
    Invalid = 0,
    Toast = 1,
}

[MemoryTable(nameof(TextMaster)), MessagePackObject]
[FileName("text.csv")]
public class TextMaster
{
    [PrimaryKey, Key(0)]
    public required MasterId<TextCategory> Id { get; init; }

    [SecondaryKey(0), NonUnique, IgnoreMember]
    public TextCategory Group => Id.Group;    // 接頭辞の列挙体部分をMasterMemoryのSecondaryKey化すると便利.

    [Key(1)]
    public string Text { get; init; }
}
```

```cs
var master1 = MemoryDatabase.TextMasterTable.Find(new MasterId(TextCategory.Toast, 1));

// コンストラクタ毎回書くのは開発体験がよくないので、ValueTupleでもキャストするように調整してみた．
var master2 = MemoryDatabase.TextMasterTable.Find( (TextCategory.Toast, 1) );
```

> [!Tip]
> 人力で入力をしていくマスターのようなデータテーブルにおいて、一意な値が必要な時に最も表現能力が高いものといえば、文字列（string型）が真っ先に思い浮かぶ．
> ただ、検索引数としてIDを利用するという、最も発生頻度の高いID利用ユースケースを想定するとき、検索アルゴリズムの高速化の選択肢が文字列だと絞られる．
> これに関連する事例だが、[MasterMemory](https://github.com/Cysharp/MasterMemory) では、IDをstring型で運用しようとすると機能制限されたコードが生成されてしまう現象があった（一敗）．  

## メモリ改竄対策
ゲームでは、変数などをキャッシュしているマシンメモリに直接手を加えて、ゲームの挙動を変えるチート手法がある．  
チートツールなどを使えば、意外と簡単に実行可能で、ユーザーにも手が届きやすいチートになる．  
この対策として、内部でランタイム中に簡単な暗号化をすることによってこれを回避する `Obscured` を提供する．

```cs
public record EnemyMaster
{
    private readonly Obscured<MasterId<BattleField>> _id; // IDを改竄されたくないので、キャッシュする部分を対策.

    [PrimaryKey, Key(0)]
    public required MasterId<BattleField> Id // 内部ではメモリ改竄対策しているが、開発体験を損なわないようプロパティで調整．
    {
        get => _id;
        init => _id = value;
    }

    /// <summary>
    /// グループ（フィールド種別）.
    /// </summary>
    [SecondaryKey(0), NonUnique, IgnoreMember]
    public BattleField Group => Id.Group;
}
```

## ローカライズ（`LocalizeFormat`型）
ローカライズ対応するためのオブジェクト．  
フォーマット文字列（"魔法石 {0}個"みたいな文字列．）のパース機能を持っているくらいで、実態としては一つの文字列データ．  
このオブジェクトの意義は `LocalizedFormat` 型という型でマーキングして、マスターコンバート時にローカライズ実装を差し込む余地を作ることにある．  

```cs
public class TextMaster
{
    public required MasterId<TextCategory> Id { get; init; }

    [SecondaryKey(0), NonUnique, IgnoreMember]
    public TextCategory Group => Id.Group;

    [Key(1)]
    public required LocalizeFormat Format { get; init; } // マスター内に保持．
}
```

```cs
 // パースもできる．
LocalizedFormat format = LocalizedFormat.Parse("魔法石 {0}個");
// ユーティリティLocalizeクラスを使ってローカライズ対応．
string result = Localize.Format(format, 13);
// "魔法石 13個"
Console.Writeline(result);
```

### `LocalizeFormat`の対応している書式指定文字列について
`LocalizeFormat`は.NET由来の書式指定文字列をほぼほぼ取り込んでいるため、これらを使った文字列を指定することができる．

```cs
// N0：3桁区切りでコンマ
LocalizedFormat format = LocalizedFormat.Parse("魔法石 {0:N0}個");
// ユーティリティLocalizeクラスを使ってローカライズ対応．
string result = Localize.Format(format, 13154);
// "魔法石 13,154個"
Console.Writeline(result);
```

また、Unityに限ったユースケースだが `TextMeshPro` などで表示する前提ならUnity独自の**リッチテキスト**を使うこともできる．  
マスターデータの表現能力を高められるため、ある程度知っておくと便利．

#### 参照文献
- [.NETドキュメント 数値、日付、その他の方の書式を設定する](https://learn.microsoft.com/ja-jp/dotnet/standard/base-types/formatting-types)
- [unity サポートされるリッチテキストタグ](https://docs.unity3d.com/ja/2023.2/Manual/UIE-supported-tags.html)

## 簡易DIコンテナ
[Utils.Unity.Addressable.SceneControllerCore](https://andantetribe.github.io/Utils/api/AndanteTribe.Utils.Unity.Addressable.SceneControllerCore-1.html) などで使っている、ワンスコープ限りのコンパクトなDI風記述．  
提供されている `Container` も `Provider` も構造体であり、内部は配列プールを活用しているため、あまり多くないオブジェクト数でかつ長く保持しないユースケースしか想定していない．  
もし長く保持するDIが必要であれば、おとなしく [Microsoft.Extensions.DependencyInjection](https://learn.microsoft.com/ja-jp/dotnet/api/microsoft.extensions.dependencyinjection)などを使うことを推奨．

```cs
var container = new Container();
container.Bind(new Hoge()); // クラスのみバインド可能．
container.Bind(new Fuga());

// プロバイダー構築．
var provider = container.Build();
// 確保したオブジェクトのインスタンスを列挙するくらいしかできない．
foreach (var (type, instance) in provider.Bindings)
{
    Console.WriteLine($"{type.FullName}: {instance.ToString()}");
}
```
