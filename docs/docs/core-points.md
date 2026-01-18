# 利用上のポイント集
[Utils.Core](https://www.nuget.org/packages/AndanteTribe.Utils.Core) で提供する実装の重要なAPIは、おおよそAPIドキュメントにてサンプルコードとともに解説されている．
APIドキュメントは参照することを推奨する．

## `IInitializable` インターフェイス
初期化処理の提供、すなわちエントリーポイントを司るものとして定義されている．
[IDisposable](https://learn.microsoft.com/en-us/dotnet/api/system.idisposable) の逆．

この実装はかなり癖が強い実装で、まずこのインターフェイスを実装した際にコンパイルエラーが発生しない．
ただインターフェイス定義がある２つのメソッドのいずれかを実装しない場合、実行時にエラーを発する．

```csharp
public class EntryPoint : IInitializable
{
    // コンパイルエラーは発生しない
}

var entry = new EntryPoint();
entry.Initialize(); // 実行時にエラーが発生する
```

`IInitializable` インターフェイスを実装したクラスは、`Initialize` メソッドまたは `InitializeAsync` メソッドのいずれか一方を実装しなければならない．
`Initialize` メソッドが同期的な初期化処理、`InitializeAsync` メソッドが非同期的な初期化処理を記述できるようになっている．

> [!Tip]
> [VContainer](https://github.com/hadashiA/VContainer) にも [VContainer.Unity.Initializable](https://github.com/hadashiA/VContainer/blob/master/VContainer/Assets/VContainer/Runtime/Annotations/IInitializable.cs) という同名のインターフェイスがある．
> 定義上の意味合いとして、命名が被るのは致し方ないが、併用する場合は混同しないように注意が必要である．