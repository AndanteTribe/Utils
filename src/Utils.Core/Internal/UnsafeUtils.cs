#nullable enable

using System.Runtime.CompilerServices;

namespace AndanteTribe.Utils.Internal
{
    internal static class UnsafeUtils
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref TTo As<TFrom, TTo>(ref TFrom source)
        {
#if UNITY_2022_3_OR_NEWER
            return ref Unity.Collections.LowLevel.Unsafe.UnsafeUtility.As<TFrom, TTo>(ref source);
#else
            return ref Unsafe.As<TFrom, TTo>(ref source);
#endif
        }
    }
}