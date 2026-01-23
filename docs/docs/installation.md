# インストール手順
## 1. NuGet
.NETプロジェクトだと言わずもがな、Unityで利用する場合にも [NuGetForUnity](https://github.com/GlitchEnzo/NuGetForUnity) などを使ってインストールする必要がある．

1. [Utils.Core](https://www.nuget.org/packages/AndanteTribe.Utils.Core)
2. [Utils.BackPort](https://www.nuget.org/packages/AndanteTribe.Utils.BackPort)
3. [Utils.Csv](https://www.nuget.org/packages/AndanteTribe.Utils.Csv)
4. [Utils.GameServices](https://www.nuget.org/packages/AndanteTribe.Utils.GameServices)
5. [Utils.MasterServices](https://www.nuget.org/packages/AndanteTribe.Utils.MasterServices)

- NuGet経由で提供されているパッケージは上記５つ．
- [Utils.MasterServices](https://www.nuget.org/packages/AndanteTribe.Utils.MasterServices) は [Utils.Csv](https://www.nuget.org/packages/AndanteTribe.Utils.Csv) に依存しているなど、このパッケージ間の依存関係は自動解決されるため、意識する必要はない．
- 後述する `Utils.Unity` では [Utils.Core](https://www.nuget.org/packages/AndanteTribe.Utils.Core), [Utils.BackPort](https://www.nuget.org/packages/AndanteTribe.Utils.BackPort), [Utils.GameServices](https://www.nuget.org/packages/AndanteTribe.Utils.GameServices) あたりに依存している．

> [!Tip]
> [Utils.MasterServices](https://www.nuget.org/packages/AndanteTribe.Utils.MasterServices) (と依存する [Utils.Csv](https://www.nuget.org/packages/AndanteTribe.Utils.Csv) ) は、Unityで使用する場合ランタイムに含める必要のない実装である．  
> 可能であればEditorだけに含ませるよう構成設定したい．

## 2. Unity（`Utils.Unity` : Unity用の拡張）
1. [NuGetForUnity](https://github.com/GlitchEnzo/NuGetForUnity) などで [Utils.Core](https://www.nuget.org/packages/AndanteTribe.Utils.Core) or [Utils.GameServices](https://www.nuget.org/packages/AndanteTribe.Utils.GameServices) をインストール
   - [Utils.BackPort](https://www.nuget.org/packages/AndanteTribe.Utils.BackPort) は [Utils.Core](https://www.nuget.org/packages/AndanteTribe.Utils.Core) も [Utils.GameServices](https://www.nuget.org/packages/AndanteTribe.Utils.GameServices) も依存しているため、自動参照でインストールされるはず．
   - `Utils.Unity` は `UniTask` など、インストールされたサードパーティーライブラリによって依存関係が若干変わる．  
  [Utils.GameServices](https://www.nuget.org/packages/AndanteTribe.Utils.GameServices) がなくても依存関係が解決できるときもあるが、わからなければとりあえずインストールはあり．
2. Unityの `Package Manager` 経由で以下URLからインストール．

```
https://github.com/AndanteTribe/Utils.git?path=src/Utils.Unity/Packages/jp.andantetribe.utils
```

> [!Tip]
> **NuGetForUnityの操作**
> 1. [NuGetForUnity](https://github.com/GlitchEnzo/NuGetForUnity) をインストール．
> 2. エディタ左上ツールバーから `NuGet > Manage NuGet Packages` でNuGetウィンドウを開く．
> 3. [Utils.GameServices](https://www.nuget.org/packages/AndanteTribe.Utils.GameServices) であれば「Utils.GameServices」と検索．「AndanteTribe」で検索したほうがわかりやすいかも？

> [!Tip]
> **Package Manager(Unity)の操作**
> 1. エディタ左中央付近（モニター解像度による）の `Window > Package Manager` から Package Managerウィンドウを開く．
> 2. `[+] > Add package from git URL` からURLを入力してEnterで確定．
