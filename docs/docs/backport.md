# Utils.BackPort
## バックポートしているAPI
- [CallerArgumentExpressionAttribute](https://learn.microsoft.com/ja-jp/dotnet/api/system.runtime.compilerservices.callerargumentexpressionattribute?view=net-6.0)
- [CancellationToken.UnsafeRegister](https://learn.microsoft.com/ja-jp/dotnet/api/system.threading.cancellationtoken.unsaferegister?view=net-6.0)
- [CollectionsMarshal.AsSpan<T>(List<T>)](https://learn.microsoft.com/ja-jp/dotnet/api/system.runtime.interopservices.collectionsmarshal.asspan?view=net-6.0)
- [DefaultInterpolatedStringHandler](https://learn.microsoft.com/ja-jp/dotnet/api/system.runtime.compilerservices.defaultinterpolatedstringhandler?view=net-6.0)
- [ISpanFormattable](https://learn.microsoft.com/ja-jp/dotnet/api/system.ispanformattable?view=net-6.0)
- [InterpolatedStringHandlerArgumentAttribute](https://learn.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.interpolatedstringhandlerargumentattribute?view=net-6.0)
- [InterpolatedStringHandlerAttribute](https://learn.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.interpolatedstringhandlerattribute?view=net-6.0)
- [IsExternalInit](https://learn.microsoft.com/ja-jp/dotnet/api/system.runtime.compilerservices.isexternalinit?view=net-6.0)

## 補足事項
- `CancellationToken.UnsafeRegister`に関して、APIは同じだが、挙動は UniTask の `CancellationToken.RegisterWithoutCaptureExecutionContext` と同じ。
- `DefaultInterpolatedStringHandler`周りの挙動は、多少実装を弄っていて、`ISpanFormattable`を使った最適化も可能なようにしてある。`Utils.Unity`導入時は`UnityEngine.Vector3`型など、Unity環境特有の型への最適化対応もしてある。

## これらの実装のバックポートがもたらす副次的効果
- `init`アクセサ
    - [init 専用セッター - C# feature specifications](https://learn.microsoft.com/ja-jp/dotnet/csharp/language-reference/proposals/csharp-9.0/init)
    - [init キーワード - init のみのプロパティ - C# reference](https://learn.microsoft.com/ja-jp/dotnet/csharp/language-reference/keywords/init)
- `record`型
    - [レコード - C# reference](https://learn.microsoft.com/ja-jp/dotnet/csharp/language-reference/builtin-types/record)
- 補間文字列の強化
    - [補間文字列の改善 - C# feature specifications](https://learn.microsoft.com/ja-jp/dotnet/csharp/language-reference/proposals/csharp-10.0/improved-interpolated-strings)
