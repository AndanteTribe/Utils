using System.Runtime.CompilerServices;
namespace AndanteTribe.Utils;

/// <summary>
/// <see cref="IDisposable"/>のユーティリティ.
/// </summary>
public static class DisposableUtils
{
    /// <summary>
    /// <see cref="IDisposable"/>を指定した<see cref="IDisposable"/>コンテナに追加します.
    /// </summary>
    /// <example>
    /// <code>
    /// <![CDATA[
    /// using System.Threading;
    /// using AndanteTribe.Utils;
    /// using R3;
    /// using UnityEngine;
    ///
    /// public class AddToExample : MonoBehaviour
    /// {
    ///     private CancellationTokenRegistration _subscription;
    ///
    ///     private void Start()
    ///     {
    ///         // Same as the following code:
    ///         // _subscription = Observable
    ///         //     .EveryUpdate()
    ///         //     .Subscribe(static _ => Debug.Log(Time.frameCount))
    ///         //     .RegisterTo(destroyCancellationToken);
    ///
    ///         Observable
    ///             .EveryUpdate()
    ///             .Subscribe(static _ => Debug.Log(Time.frameCount))
    ///             .RegisterTo(destroyCancellationToken) // return CancellationTokenRegistration that is struct
    ///             .AddTo(ref _subscription);
    ///     }
    ///
    ///     public void CancelSubscription()
    ///     {
    ///         _subscription.Dispose();
    ///     }
    /// }
    /// ]]>
    /// </code>
    /// </example>
    /// <param name="disposable">任意の<see cref="IDisposable"/>実装オブジェクト.</param>
    /// <param name="disposableContainer">追加先の<see cref="IDisposable"/>コンテナ.</param>
    /// <typeparam name="T">追加した<see cref="IDisposable"/>.</typeparam>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AddTo<T>(this T disposable, ref T disposableContainer) where T : IDisposable => disposableContainer = disposable;

    /// <summary>
    /// <see cref="CancellationTokenSource"/>がキャンセルされているかどうかを確認し、キャンセルされている場合は<see cref="ObjectDisposedException"/>をスローします。
    /// </summary>
    /// <example>
    /// <code>
    /// <![CDATA[
    /// using System;
    /// using System.Threading;
    /// using System.Threading.Tasks;
    ///
    /// public class Sample
    /// {
    ///     private CancellationTokenSource _cancellationDisposable = new();
    ///
    ///     public async Task LoadAsync()
    ///     {
    ///         // //従来の書き方
    ///         // //既にキャンセル処理が呼ばれているか確認
    ///         // if (_cancellationDisposable.IsCancellationRequested)
    ///         // {
    ///         //     throw new ObjectDisposedException("AudioPlayer");
    ///         // }
    ///
    ///         //このように一行でキャンセル済みか確認可能
    ///         _cancellationDisposable.ThrowIfDisposed(this);
    ///
    ///         var token = _cancellationDisposable.Token;
    ///
    ///         // 以下、非同期処理が続く
    ///         // await Addressables.LoadAsync(token);
    ///     }
    ///
    /// }
    /// ]]>
    /// </code>
    /// </example>
    /// <param name="cancellationTokenSource">対象のインスタンス.</param>
    /// <param name="instance">オプションのインスタンス名。<see cref="ObjectDisposedException"/>のメッセージに使用されます。</param>
    /// <exception cref="ObjectDisposedException">キャンセルされている場合にスローされます。</exception>
    /// <remarks><see cref="CancellationTokenSource"/>を<see cref="IDisposable"/>に見立てるような思想のメソッド.</remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ThrowIfDisposed(this CancellationTokenSource cancellationTokenSource, object? instance = null)
    {
        if (cancellationTokenSource.IsCancellationRequested)
        {
            throw new ObjectDisposedException(instance?.GetType().FullName);
        }
    }

    /// <summary>
    /// <see cref="CancellationTokenSource"/>と<see cref="CancellationToken"/>をリンクさせた新しい<see cref="CancellationTokenSource"/>を作成します。
    /// </summary>
    /// <example>
    /// <code>
    /// <![CDATA[
    /// using System.Threading;
    /// using Cysharp.Threading.Tasks;
    /// using UnityEngine;
    ///
    /// public class Sample : MonoBehaviour
    /// {
    ///     private CancellationTokenSource _cancellationTokenSource = new();
    ///
    ///     public async UniTaskVoid Load()
    ///     {
    ///         var token = Application.exitCancellationToken;
    ///
    ///         // このインスタンスのCancellationTokenSource、またはアプリケーションの終了が要求されたときにキャンセルされる、リンクされたCancellationTokenを作成します。
    ///         var linkedTokenSource = _cancellationTokenSource.CreateLinkedTokenSource(token);
    ///
    ///         await UniTask.Delay(TimeSpan.FromSeconds(10), cancellationToken:linkedTokenSource.Token);
    ///
    ///     }
    /// }
    /// ]]>
    /// </code>
    /// </example>
    /// <param name="cancellationTokenSource">リンク元の<see cref="CancellationTokenSource"/>.</param>
    /// <param name="token">リンクする<see cref="CancellationToken"/>.</param>
    /// <returns>新しい<see cref="CancellationTokenSource"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static CancellationTokenSource CreateLinkedTokenSource(this CancellationTokenSource cancellationTokenSource, CancellationToken token) =>
        CancellationTokenSource.CreateLinkedTokenSource(cancellationTokenSource.Token, token);

    /// <summary>
    /// <see cref="CancellationTokenSource"/>と2つの<see cref="CancellationToken"/>をリンクさせた新しい<see cref="CancellationTokenSource"/>を作成します。
    /// </summary>
    /// <example>
    /// <code>
    /// <![CDATA[
    /// using System.Threading;
    /// using Cysharp.Threading.Tasks;
    /// using UnityEngine;
    ///
    /// public class Sample : MonoBehaviour
    /// {
    ///     [SerializeField] private Player _player;
    ///     [SerializeField] private Enemy _enemy;
    ///
    ///     private CancellationTokenSource _cancellationTokenSource = new();
    ///
    ///     public async UniTaskVoid Load()
    ///     {
    ///         var token = _player.destroyCancellationToken;
    ///         var token2 = _enemy.destroyCancellationToken;
    ///
    ///         // プレイヤーか敵が破棄された場合、またはこのクラスのCancellationTokenSourceがキャンセルされた場合にキャンセルされるトークンを作成する。
    ///         var linkedTokenSource = _cancellationTokenSource.CreateLinkedTokenSource(token,token2);
    ///
    ///         await UniTask.Delay(TimeSpan.FromSeconds(10), cancellationToken:linkedTokenSource.Token);
    ///
    ///     }
    /// }
    ///
    /// public class Player : MonoBehaviour
    /// {
    ///     //以下コード
    /// }
    ///
    /// public class Enemy : MonoBehaviour
    /// {
    ///     //以下コード
    /// }
    /// ]]>
    /// </code>
    /// </example>
    /// <param name="cancellationTokenSource">リンク元の<see cref="CancellationTokenSource"/>.</param>
    /// <param name="tokens1">リンクする最初の<see cref="CancellationToken"/>.</param>
    /// <param name="tokens2">リンクする2つ目の<see cref="CancellationToken"/>.</param>
    /// <returns>新しい<see cref="CancellationTokenSource"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static CancellationTokenSource CreateLinkedTokenSource(this CancellationTokenSource cancellationTokenSource, CancellationToken tokens1, CancellationToken tokens2) =>
        CancellationTokenSource.CreateLinkedTokenSource(cancellationTokenSource.Token, tokens1, tokens2);
}