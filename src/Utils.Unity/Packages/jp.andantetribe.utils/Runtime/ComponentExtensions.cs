#nullable enable

using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace AndanteTribe.Utils
{
    /// <summary>
    /// <see cref="Component"/>の拡張メソッド.
    /// </summary>
    public static class ComponentExtensions
    {
        /// <summary>
        /// 子オブジェクトの<typeparamref name="T"/>コンポーネントを全て取得する.
        /// </summary>
        /// <remarks>
        /// <see cref="Component.GetComponentsInChildren{T}()"/>と異なり、孫オブジェクト以降は検索しない.
        /// </remarks>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// using System;
        /// using MinimalUtility;
        /// using UnityEngine;
        ///
        /// public class GetComponentsInOnlyChildrenExample : MonoBehaviour
        /// {
        ///     private void Start()
        ///     {
        ///         ReadOnlyMemory<HingeJoint> hingeJoints = this.GetComponentsInOnlyChildren<HingeJoint>();
        ///
        ///         if (!hingeJoints.IsEmpty)
        ///         {
        ///             foreach (HingeJoint joint in hingeJoints)
        ///             {
        ///                 joint.useSpring = false;
        ///             }
        ///         }
        ///         else
        ///         {
        ///             // Try again, looking for inactive GameObjects
        ///             ReadOnlyMemory<HingeJoint> hingesInactive = this.GetComponentsInOnlyChildren<HingeJoint>(true);
        ///
        ///             foreach (HingeJoint joint in hingesInactive)
        ///             {
        ///                 joint.useSpring = false;
        ///             }
        ///         }
        ///     }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="self">対象の<see cref="Component"/>.</param>
        /// <param name="includeInactive">非アクティブのゲームオブジェクトも含めるかどうか.デフォルトは含めない.</param>
        /// <typeparam name="T">取得したいコンポーネントの型.</typeparam>
        /// <returns>取得したコンポーネントインスタンスのコレクション.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlyMemory<T> GetComponentsInOnlyChildren<T>(this Component self, bool includeInactive = false) where T : Component
        {
            var transform = self.transform;
            var limit = transform.childCount;
            if (limit == 0)
            {
                return ReadOnlyMemory<T>.Empty;
            }

            var array = new T[limit];
            var span = array.AsSpan();
            var count = 0;
            for (var i = 0; i < span.Length; i++)
            {
                var gameObject = transform.GetChild(i).gameObject;
                if (gameObject.TryGetComponent<T>(out var component))
                {
                    if (!includeInactive && !gameObject.activeSelf)
                    {
                        continue;
                    }

                    span[count++] = component;
                }
            }
            return new ReadOnlyMemory<T>(array, 0, count);
        }

        /// <summary>
        /// 安全な<see cref="Component.GetComponent{T}"/>.
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// using MinimalUtility;
        /// using UnityEngine;
        ///
        /// public class SafeGetComponentExample : MonoBehaviour
        /// {
        ///     private HingeJoint _hinge;
        ///
        ///     private void Update()
        ///     {
        ///         hinge ??= this.SafeGetComponent<HingeJoint>();
        ///         hinge.useSpring = false;
        ///     }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="self">対象の<see cref="Component"/>.</param>
        /// <typeparam name="T">取得したいコンポーネントの型.</typeparam>
        /// <returns>取得したコンポーネントインスタンス.なお取得不可の場合はSystemのnullを返す.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T SafeGetComponent<T>(this Component self) where T : Component
        {
            self.gameObject.TryGetComponent<T>(out var component);
            return component;
        }
    }
}
