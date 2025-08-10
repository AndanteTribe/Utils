#if ENABLE_UNITASK

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace AndanteTribe.Utils.Unity.Tasks
{
    public sealed class AsyncSemaphore
    {
        private readonly List<Handle> _queue = new List<Handle>();
        private uint _version = 0;
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        /// <summary>
        /// 順番待ちをします.
        /// </summary>
        /// <remarks>
        /// 返されたハンドルは処理が終わったら必ずDisposeすること.
        /// </remarks>
        /// <returns>順番を明け渡すためのハンドル.</returns>
        public async UniTask<IDisposable> WaitOneAsync(
            CancellationToken cancellationToken = default,
            [CallerFilePath] string callerFilePath = "",
            [CallerMemberName] string callerMethodName = "",
            [CallerLineNumber] int callerLineNumber = -1)
        {
            cancellationToken.ThrowIfCancellationRequested();
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _cancellationTokenSource.Token);

#if UNITY_EDITOR || DEVELOP_BUILD
            var callerInfo = GetCallerInfo(callerFilePath, callerMethodName, callerLineNumber);
            Debug.Log("[AsyncSemaphore] WaitOneAsync : " + callerInfo);
            var handle = new Handle(_queue, _version++, callerInfo);
#else
			var handle = new Handle(queue, _version++);
#endif
            await UniTask.WaitUntil(handle, static handle => handle.IsReady, cancellationToken: cts.Token);
            return handle;
        }

        /// <summary>
        /// 現時点でキュー待機している処理を全てキャンセル・破棄します.
        /// </summary>
        public void CancelAll(
            [CallerFilePath] string callerFilePath = "",
            [CallerMemberName] string callerMethodName = "",
            [CallerLineNumber] int callerLineNumber = -1)
        {
#if UNITY_EDITOR || DEVELOP_BUILD
            var callerInfo = GetCallerInfo(callerFilePath, callerMethodName, callerLineNumber);
            Debug.Log("[AsyncSemaphore] CancelAll : " + callerInfo);
#endif
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
            _cancellationTokenSource = new CancellationTokenSource();
            foreach (var h in _queue.AsSpan())
            {
                h.Dispose();
            }
            _queue.Clear(); // 念のためクリア
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string GetCallerInfo(string filePath, string methodName, int lineNumber)
        {
            var sb = new DefaultInterpolatedStringHandler(4, 2);
            var lastSlashIndex = filePath.LastIndexOf('/');
            sb.AppendFormatted(lastSlashIndex == -1 ? filePath : filePath[(lastSlashIndex + 1)..]);
            sb.AppendLiteral(" ");
            sb.AppendLiteral(methodName);
            sb.AppendLiteral(" ");
            sb.AppendFormatted(lineNumber);
            sb.AppendLiteral("行目");
            return sb.ToStringAndClear();
        }

        /// <summary>
        /// 順番待ちに使うハンドル.
        /// </summary>
        public readonly struct Handle : IEquatable<Handle>, IDisposable
        {
            private readonly List<Handle> _queue;
            private readonly uint _version;

            /// <summary>
            /// 現在のハンドルが順番待ちの先頭にいるかどうかを示します
            /// </summary>
            public bool IsReady => _queue[0]._version == _version;

#if UNITY_EDITOR || DEVELOP_BUILD
            private readonly string _callerInfo;

            /// <summary>
            /// Initialize a new instance of the <see cref="Handle"/> struct.
            /// </summary>
            /// <param name="queue"></param>
            /// <param name="version"></param>
            /// <param name="callerInfo"></param>
            public Handle(List<Handle> queue, uint version, string callerInfo = "")
            {
                _version = version;
                _callerInfo = callerInfo;
                _queue = queue;
                queue.Add(this);
            }
#else
            /// <summary>
            /// Initialize a new instance of the <see cref="Handle"/> struct.
            /// </summary>
            /// <param name="queue"></param>
            /// <param name="version"></param>
			public Handle(List<Handle> queue, uint version)
			{
                _version = version;
                _queue = queue;
                queue.Add(this);
			}
#endif
            public bool Equals(Handle other) =>
                _queue == other._queue && _version == other._version;

            public void Dispose()
            {
                _queue.Remove(this);
#if UNITY_EDITOR || DEVELOP_BUILD
                if (!string.IsNullOrEmpty(_callerInfo))
                {
                    Debug.Log("[AsyncSemaphore] Handle Dispose : " + _callerInfo);
                }
#endif
            }
        }
    }
}

#endif