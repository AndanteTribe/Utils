#nullable enable

using System.Runtime.CompilerServices;
using UnityEngine;

namespace AndanteTribe.Utils.Unity
{
    /// <summary>
    /// <see cref="Vector3"/>の拡張クラス.
    /// </summary>
    public static class VectorExtensions
    {
        /// <summary>
        /// <see cref="Vector3"/>のXZ成分だけ取り出す.
        /// </summary>
        /// <param name="target">対象.</param>
        /// <returns>対象のx成分をx, z成分をyに保持する<see cref="Vector2"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 XZ(in this Vector3 target) => new Vector2(target.x, target.z);

        /// <summary>
        /// <see cref="Vector3"/>のYZ成分だけ取り出す.
        /// </summary>
        /// <param name="target">対象.</param>
        /// <returns>対象のy成分をx, z成分をzに保持する<see cref="Vector2"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 YZ(in this Vector3 target) => new Vector2(target.y, target.z);
    }
}