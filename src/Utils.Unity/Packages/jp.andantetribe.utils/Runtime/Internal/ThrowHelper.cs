#nullable enable

using System;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace AndanteTribe.Utils.Internal
{
    internal static class ThrowHelper
    {
        [HideInCallstack]
        internal static void ThrowIfDisposedException([DoesNotReturnIf(true)] bool condition, object instance)
        {
            if (condition)
            {
                throw new ObjectDisposedException(instance?.GetType().FullName);
            }
        }
    }
}