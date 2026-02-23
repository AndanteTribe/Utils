#if ENABLE_R3
#nullable enable

using System;
using System.Runtime.CompilerServices;
using R3;

namespace AndanteTribe.Utils.Unity.R3
{
    /// <content>
    /// Observableの拡張メソッド.
    /// </content>
    public static partial class ObservableExtensions
    {
        /// <summary>
        /// Observable上で起きたすべてのイベントをログに出力する.
        /// </summary>
        /// <param name="source">任意のObservable.</param>
        /// <param name="label">ログに出力するラベル.</param>
        /// <typeparam name="T">Observableの型.</typeparam>
        /// <returns>ログ出力を行うObservable.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Observable<T> Debug<T>(this Observable<T> source, string label = "")
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            var l = string.IsNullOrEmpty(label) ? "" : "[" + label + "]";
            return source
                .Do(
                    state: l,
                    onNext: static (x, lb) => UnityEngine.Debug.Log(lb + "OnNext(" + x + ")"),
                    onErrorResume: static (x, lb) => UnityEngine.Debug.Log(lb + "OnErrorResume(" + x + ")"),
                    onCompleted: static (x, lb) => UnityEngine.Debug.Log(lb + "OnCompleted(" + x + ")"),
                    onSubscribe: static lb => UnityEngine.Debug.Log(lb + "OnSubscribe"),
                    onDispose: static lb => UnityEngine.Debug.Log(lb + "OnDispose"));
#else
            return source;
#endif
        }

        /// <summary>
        /// Observable上で起きたすべてのイベントをログに出力する.
        /// </summary>
        /// <param name="source">任意のObservable.</param>
        /// <param name="onNext">ログに出力したい値を取得する処理.</param>
        /// <typeparam name="T">Observableの型.</typeparam>
        /// <typeparam name="TState">ログに出力したい値の型.</typeparam>
        /// <returns>ログ出力を行うObservable.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Observable<T> Debug<T, TState>(this Observable<T> source, Func<T, TState> onNext)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            return source
                .Do(
                    state: onNext,
                    onNext: static (x, state) => UnityEngine.Debug.Log($"OnNext({state(x)})"),
                    onErrorResume: static (x, _) => UnityEngine.Debug.Log($"OnErrorResume({x})"),
                    onCompleted: static (x, _) => UnityEngine.Debug.Log($"OnCompleted({x})"),
                    onSubscribe: static _ => UnityEngine.Debug.Log("OnSubscribe"),
                    onDispose: static _ => UnityEngine.Debug.Log("OnDispose"));
#else
            return source;
#endif
        }
    }
}
#endif