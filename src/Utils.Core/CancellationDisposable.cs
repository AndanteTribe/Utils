#nullable enable

using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace AndanteTribe.Utils
{
    public abstract class CancellationDisposable : ITrackableDisposable
    {
        private readonly CancellationTokenSource _disposableTokenSource = new();

        public CancellationToken DisposableToken => IsDisposed ? new CancellationToken(true) : _disposableTokenSource.Token;

        public bool IsDisposed => _disposableTokenSource.IsCancellationRequested;

        public virtual void Dispose()
        {
            if (!IsDisposed)
            {
                _disposableTokenSource.Cancel();
                _disposableTokenSource.Dispose();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected IDisposable CreateLinkedToken(in CancellationToken cancellationToken, out CancellationToken linkedToken)
        {
            var cts = CancellationTokenSource.CreateLinkedTokenSource(_disposableTokenSource.Token, cancellationToken);
            linkedToken = cts.Token;
            return cts;
        }
    }
}