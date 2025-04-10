#nullable enable

using System.Runtime.CompilerServices;
using UnityEngine;

namespace AndanteTribe.Utils.Internal
{
    internal static class DontDestroyObject
    {
        public static Transform Root
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Shared.transform;
        }

        public static GameObject Shared
        {
            get
            {
                if (s_shared == null)
                {
                    s_shared = new GameObject("MinimalUtility." + nameof(DontDestroyObject));
                    Object.DontDestroyOnLoad(s_shared);
                }
                return s_shared;
            }
        }

        private static GameObject? s_shared;
    }
}