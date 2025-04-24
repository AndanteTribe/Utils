#nullable enable

using System.Runtime.CompilerServices;
using UnityEngine;

namespace AndanteTribe.Utils
{
    /// <summary>
    /// <see cref="Transform"/>の拡張メソッド.
    /// </summary>
    public static class TransformExtensions
    {
        /// <summary>
        /// <see cref="Transform.position"/>のX成分を設定する.
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="x"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetX(this Transform transform, in float x)
        {
            var position = transform.position;
            position.x = x;
            transform.position = position;
        }

        /// <summary>
        /// <see cref="Transform.position"/>のX成分を加算する.
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="x"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddX(this Transform transform, in float x)
        {
            var position = transform.position;
            position.x += x;
            transform.position = position;
        }

        /// <summary>
        /// <see cref="Transform.position"/>のX成分を減算する.
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="x"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RemoveX(this Transform transform, in float x)
        {
            var position = transform.position;
            position.x -= x;
            transform.position = position;
        }

        /// <summary>
        /// <see cref="Transform.position"/>のY成分を設定する.
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="y"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetY(this Transform transform, in float y)
        {
            var position = transform.position;
            position.y = y;
            transform.position = position;
        }

        /// <summary>
        /// <see cref="Transform.position"/>のY成分を加算する.
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="y"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddY(this Transform transform, in float y)
        {
            var position = transform.position;
            position.y += y;
            transform.position = position;
        }

        /// <summary>
        /// <see cref="Transform.position"/>のY成分を減算する.
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="y"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RemoveY(this Transform transform, in float y)
        {
            var position = transform.position;
            position.y -= y;
            transform.position = position;
        }

        /// <summary>
        /// <see cref="Transform.position"/>のZ成分を設定する.
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="z"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetZ(this Transform transform, in float z)
        {
            var position = transform.position;
            position.z = z;
            transform.position = position;
        }

        /// <summary>
        /// <see cref="Transform.position"/>のZ成分を加算する.
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="z"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddZ(this Transform transform, in float z)
        {
            var position = transform.position;
            position.z += z;
            transform.position = position;
        }

        /// <summary>
        /// <see cref="Transform.position"/>のZ成分を減算する.
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="z"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RemoveZ(this Transform transform, in float z)
        {
            var position = transform.position;
            position.z -= z;
            transform.position = position;
        }
    }
}