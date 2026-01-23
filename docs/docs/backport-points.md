# 利用上のポイント集
## 1. 文字列は、深く考えず文字列補間を使う
[Utils.BackPort](https://www.nuget.org/packages/AndanteTribe.Utils.BackPort) を導入している場合、強化された文字列補間を利用できるので、あまり考えたくなければ文字列補間を使うことを推奨する．  
C#の文字列関連の実装中で最大の可読性も担保できるので、使わないメリットは薄い．  
[Cysharp/ZString](https://github.com/Cysharp/ZString)のようなライブラリも（やっていることが同じ、かつより高い可読性を文字列補間のほうが有するため）入れる必要なし．

## 2. `init` アクセサを使いこなす
`init` は次節で紹介する `record` 型を実装する過程で登場したアクセサだが、これ単体でも結構有用．  
オブジェクトを実装時、コンストラクタの引数に設定すると、その値を設定しなければ該当オブジェクトのインスタンスを生成できなくなる．

> [!Tip]
> コンストラクタで引数を設定することは、オブジェクトの生成に条件をつけることと同義である.  
> その引数を指定しなければインスタンスを生成できないのだから、当然のことといえる．  
> これは設計の観点でいえば**依存関係の形成**という意義があり、単純に制限の厳しさに目を向ければ「複数のコンストラクタの許容」・「デフォルト引数」などの制限を緩和するＣ＃機能に価値を見出すこともできるだろう．

```csharp
// init アクセサはコンストラクタの引数ではないので、省略可能
var player = new Player("Hero", 1);

// デバッグ用途で攻撃力を設定したい場合など、必要なときに有用
var debugPlayer = new Player("Hero", 1)
{
    Attack = 20 // こういう書き方をイニシャライザー（初期化子）といい、プロパティ名を明示的に書くのでわかりやすい.
};

public sealed class Player
{
    public readonly string Name; // 必須
    public readonly int Level;  // 必須

    public int Attack { get; init; } = 10; // 任意、デフォルト値あり（オプション寄りの書き方）

    public Player(string name, int level)
    {
        Name = name;
        Level = level;
    }
}
```

## 3. `record` 型
`record` 型はイミュータブルなオブジェクトを簡単に実装できて便利．

```csharp
public record PlayerRecord(string Name, int Level);
```

上記の実装は、以下のようにコンパイル過程で展開される．

<details><summary>PlayerRecordの逆コンパイル結果</summary>

```csharp
[NullableContext(1)]
[Nullable(0)]
public class PlayerRecord : IEquatable<PlayerRecord>
{
    [CompilerGenerated]
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly string <Name>k__BackingField;

    [CompilerGenerated]
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly int <Level>k__BackingField;

    [CompilerGenerated]
    protected virtual Type EqualityContract
    {
        [CompilerGenerated]
        get
        {
            return typeof(PlayerRecord);
        }
    }

    public string Name
    {
        [CompilerGenerated]
        get
        {
            return <Name>k__BackingField;
        }
        [CompilerGenerated]
        init
        {
            <Name>k__BackingField = value;
        }
    }

    public int Level
    {
        [CompilerGenerated]
        get
        {
            return <Level>k__BackingField;
        }
        [CompilerGenerated]
        init
        {
            <Level>k__BackingField = value;
        }
    }

    public PlayerRecord(string Name, int Level)
    {
        <Name>k__BackingField = Name;
        <Level>k__BackingField = Level;
        base..ctor();
    }

    [CompilerGenerated]
    public override string ToString()
    {
        StringBuilder stringBuilder = new StringBuilder();
        stringBuilder.Append("PlayerRecord");
        stringBuilder.Append(" { ");
        if (PrintMembers(stringBuilder))
        {
            stringBuilder.Append(' ');
        }
        stringBuilder.Append('}');
        return stringBuilder.ToString();
    }

    [CompilerGenerated]
    protected virtual bool PrintMembers(StringBuilder builder)
    {
        RuntimeHelpers.EnsureSufficientExecutionStack();
        builder.Append("Name = ");
        builder.Append((object)Name);
        builder.Append(", Level = ");
        builder.Append(Level.ToString());
        return true;
    }

    [NullableContext(2)]
    [CompilerGenerated]
    public static bool operator !=(PlayerRecord left, PlayerRecord right)
    {
        return !(left == right);
    }

    [NullableContext(2)]
    [CompilerGenerated]
    public static bool operator ==(PlayerRecord left, PlayerRecord right)
    {
        return (object)left == right || ((object)left != null && left.Equals(right));
    }

    [CompilerGenerated]
    public override int GetHashCode()
    {
        return (EqualityComparer<Type>.Default.GetHashCode(EqualityContract) * -1521134295 + EqualityComparer<string>.Default.GetHashCode(<Name>k__BackingField)) * -1521134295 + EqualityComparer<int>.Default.GetHashCode(<Level>k__BackingField);
    }

    [NullableContext(2)]
    [CompilerGenerated]
    public override bool Equals(object obj)
    {
        return Equals(obj as PlayerRecord);
    }

    [NullableContext(2)]
    [CompilerGenerated]
    public virtual bool Equals(PlayerRecord other)
    {
        return (object)this == other || ((object)other != null && EqualityContract == other.EqualityContract && EqualityComparer<string>.Default.Equals(<Name>k__BackingField, other.<Name>k__BackingField) && EqualityComparer<int>.Default.Equals(<Level>k__BackingField, other.<Level>k__BackingField));
    }

    [CompilerGenerated]
    public virtual PlayerRecord <Clone>$()
    {
        return new PlayerRecord(this);
    }

    [CompilerGenerated]
    protected PlayerRecord(PlayerRecord original)
    {
        <Name>k__BackingField = original.<Name>k__BackingField;
        <Level>k__BackingField = original.<Level>k__BackingField;
    }

    [CompilerGenerated]
    public void Deconstruct(out string Name, out int Level)
    {
        Name = this.Name;
        Level = this.Level;
    }
}
```

</details>

`ToString()` の自動実装はログ出力時に中身の値を表示するなど、非常に便利．  
中身の実装が気に入らない場合、生成に頼らず自力実装していればその実装が優先される（≒実質上書き可能）ので、実装を制限されるデメリットもない．

## 4. `with` 式
`record` 型と組み合わせて使うと便利なのが `with` 式．  
オブジェクトの一部のプロパティだけを変更した新しいオブジェクトを生成できる．

```csharp
var player1 = new PlayerRecord("Hero", 1);
var player2 = player1 with { Level = 2 }; // Name は "Hero"
```

## 5. `required` 修飾子
`required` 修飾子を使うと、オブジェクト生成時に必須のプロパティを指定できる．  
`init` アクセサと組み合わせて使うことが多く、うまく使えば面倒なコンストラクタ実装を省きつつ、実質コンストラクタと同等のインスタンス生成制限を実現できる．

```csharp
// required 修飾子を使ったプロパティは、オブジェクト生成時に必須なので、設定しなければエラー
var enemy1 = new Enemy(); // ERROR

var enemy2 = new Enemy
{
    Type = "Goblin" // OK
};

var enemy3 = new Enemy
{
    Type = "Orc",
    Health = 150 // 任意プロパティも設定可能
};

public sealed class Enemy
{
    public required string Type { get; init; } // 必須
    public int Health { get; init; } = 100;   // 任意、デフォルト値あり
}
```

## 6. キャンセル時コールバック登録は `CancellationToken.UnsafeRegister` を統一的に利用する．
`CancellationToken.UnsafeRegister` は.NET6で登場した、`CancellationToken.Register`よりも高効率なキャンセル時コールバック登録メソッド．  
`CancellationToken.UnsafeRegister` は内部実装含め.NET6以降でしか使えないが、[Utils.BackPort](https://www.nuget.org/packages/AndanteTribe.Utils.BackPort) では同じAPIながら、.NET5以前でよく使われたＣ＃ハックによる高効率な内部実装をしたものを提供している．  
つまり、`CancellationToken.Register` よりも効率的で、また.NETのバージョンをあげたときにコード変更なしにパフォーマンス向上の恩恵を受けられる、というもの．

> [!Tip]
> [Utils.BackPort](https://www.nuget.org/packages/AndanteTribe.Utils.BackPort) で提供している `CancellationToken.UnsafeRegister` の内部実装は、[Cysharp/UniTask](https://github.com/Cysharp/UniTask) の [CancellationToken.RegisterWithoutCaptureExecutionContext](https://cysharp.github.io/UniTask/api/Cysharp.Threading.Tasks.CancellationTokenExtensions.html#Cysharp_Threading_Tasks_CancellationTokenExtensions_RegisterWithoutCaptureExecutionContext_System_Threading_CancellationToken_System_Action_System_Object__System_Object_) と同じものなので、どちらを利用してもパフォーマンスは同じだが、将来的な .NET ランタイムバージョンアップを想定して `CancellationToken.UnsafeRegister` を利用することを推奨する．  
> また余談だが、[CancellationToken.RegisterWithoutCaptureExecutionContext](https://cysharp.github.io/UniTask/api/Cysharp.Threading.Tasks.CancellationTokenExtensions.html#Cysharp_Threading_Tasks_CancellationTokenExtensions_RegisterWithoutCaptureExecutionContext_System_Threading_CancellationToken_System_Action_System_Object__System_Object_) を内部実装に利用している関係で、[Cysharp/R3](https://github.com/Cysharp/R3) の`RegisterTo` よりも [Cysharp/UniTask](https://github.com/Cysharp/UniTask) の [AddTo](https://cysharp.github.io/UniTask/api/Cysharp.Threading.Tasks.CancellationTokenExtensions.html#Cysharp_Threading_Tasks_CancellationTokenExtensions_AddTo_System_IDisposable_System_Threading_CancellationToken_) を利用する方が効率が良い．
