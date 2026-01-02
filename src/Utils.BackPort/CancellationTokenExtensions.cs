#if !NET6_0_OR_GREATER

namespace System.Threading;

/// <summary>
/// Provides extension methods for <see cref="CancellationToken"/> to allow
/// </summary>
public static class CancellationTokenExtensions
{
    /// <summary>
    /// Registers a delegate that will be called when this
    /// <see cref="CancellationToken">CancellationToken</see> is canceled.
    /// </summary>
    /// <remarks>
    /// <para>
    /// If this token is already in the canceled state, the delegate will be run immediately and synchronously.
    /// Any exception the delegate generates will be propagated out of this method call.
    /// </para>
    /// <para>
    /// <see cref="ExecutionContext">ExecutionContext</see> is not captured nor flowed
    /// to the callback's invocation.
    /// </para>
    /// </remarks>
    /// <param name="cancellationToken"> The <see cref="CancellationToken">CancellationToken</see> to register the callback with.</param>
    /// <param name="callback">The delegate to be executed when the <see cref="CancellationToken">CancellationToken</see> is canceled.</param>
    /// <param name="state">The state to pass to the <paramref name="callback"/> when the delegate is invoked.  This may be null.</param>
    /// <returns>The <see cref="CancellationTokenRegistration"/> instance that can
    /// be used to unregister the callback.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="callback"/> is null.</exception>
    public static CancellationTokenRegistration UnsafeRegister(this CancellationToken cancellationToken, Action<object?> callback, object? state)
    {
        var restoreFlow = false;
        if (!ExecutionContext.IsFlowSuppressed())
        {
            ExecutionContext.SuppressFlow();
            restoreFlow = true;
        }

        try
        {
            return cancellationToken.Register(callback, state, false);
        }
        finally
        {
            if (restoreFlow)
            {
                ExecutionContext.RestoreFlow();
            }
        }
    }
}

#endif