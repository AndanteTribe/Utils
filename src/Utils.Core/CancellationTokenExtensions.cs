#nullable enable

using System.Runtime.CompilerServices;
using System.Threading;

namespace AndanteTribe.Utils
{
    public static class CancellationTokenExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static CancellationTokenRegistration LinkTo(in this CancellationToken self, CancellationTokenSource other)
        {
#if NET6_0_OR_GREATER
            return self.UnsafeRegister(static c =>
            {
                var other = (CancellationTokenSource)c;
                other.Cancel();
                other.Dispose();
            }, other);
#else
            var restoreFlow = false;
            if (!ExecutionContext.IsFlowSuppressed())
            {
                ExecutionContext.SuppressFlow();
                restoreFlow = true;
            }

            try
            {
                return self.Register(static c =>
                {
                    var other = (CancellationTokenSource)c;
                    other.Cancel();
                    other.Dispose();
                }, other, false);
            }
            finally
            {
                if (restoreFlow)
                {
                    ExecutionContext.RestoreFlow();
                }
            }
#endif
        }
    }
}