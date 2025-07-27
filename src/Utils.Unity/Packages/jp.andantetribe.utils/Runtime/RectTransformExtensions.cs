#nullable enable

using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace AndanteTribe.Utils.Unity
{
    /// <summary>
    /// <see cref="RectTransform"/>の拡張メソッド.
    /// </summary>
    public static class RectTransformExtensions
    {
        /// <summary>
        /// <see cref="RectTransform.sizeDelta"/>よりも安全なサイズ設定.
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// using MinimalUtility;
        /// using UnityEngine;
        ///
        /// public class SetSizeExample : MonoBehaviour
        /// {
        ///     private void Start()
        ///     {
        ///         RectTransform rectTransform = (RectTransform) transform;
        ///         rectTransform.SetSize(100, 100);
        ///     }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="rectTransform">対象の<see cref="RectTransform"/>.</param>
        /// <param name="width">横の長さ.</param>
        /// <param name="height">縦の長さ.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetSize(this RectTransform rectTransform, in float width, in float height)
        {
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
        }

        /// <summary>
        /// <see cref="RectTransform.sizeDelta"/>よりも安全なサイズ設定.
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// using MinimalUtility;
        /// using UnityEngine;
        ///
        /// public class SetSizeExample : MonoBehaviour
        /// {
        ///     private void Start()
        ///     {
        ///         RectTransform rectTransform = (RectTransform) transform;
        ///         var size = new Vector2(100, 100);
        ///         rectTransform.SetSize(size);
        ///     }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="rectTransform">対象の<see cref="RectTransform"/>.</param>
        /// <param name="size">任意の縦横サイズ.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetSize(this RectTransform rectTransform, in Vector2 size)
        {
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);
        }

        /// <summary>
        /// <see cref="RectTransform.sizeDelta"/>よりも安全な横幅設定.
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// using MinimalUtility;
        /// using UnityEngine;
        ///
        /// public class SetWidthExample : MonoBehaviour
        /// {
        ///     private void Start()
        ///     {
        ///         RectTransform rectTransform = (RectTransform) transform;
        ///         rectTransform.SetWidth(100);
        ///     }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="rectTransform">対象の<see cref="RectTransform"/>.</param>
        /// <param name="width">横の長さ.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetWidth(this RectTransform rectTransform, in float width) =>
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);

        /// <summary>
        /// <see cref="RectTransform.sizeDelta"/>よりも安全な縦幅設定.
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// using MinimalUtility;
        /// using UnityEngine;
        ///
        /// public class SetHeightExample : MonoBehaviour
        /// {
        ///     private void Start()
        ///     {
        ///         RectTransform rectTransform = (RectTransform) transform;
        ///         rectTransform.SetHeight(100);
        ///     }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="rectTransform">対象の<see cref="RectTransform"/>.</param>
        /// <param name="height">縦の長さ.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetHeight(this RectTransform rectTransform, in float height) =>
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);

        /// <summary>
        /// <see cref="RectTransform.sizeDelta"/>よりも安全なサイズ取得.
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// using MinimalUtility;
        /// using UnityEngine;
        ///
        /// public class GetSizeExample : MonoBehaviour
        /// {
        ///     private void Start()
        ///     {
        ///         RectTransform rectTransform = (RectTransform) transform;
        ///         Vector2 size = rectTransform.GetSize();
        ///         Debug.Log("Size : " + size.ToString());
        ///     }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="rectTransform">対象の<see cref="RectTransform"/>.</param>
        /// <returns>対象のサイズ.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 GetSize(this RectTransform rectTransform) => rectTransform.rect.size;

        /// <summary>
        /// <see cref="RectTransform"/>を全面的に伸ばす(stretch * stretchにする).
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// using MinimalUtility;
        /// using UnityEngine;
        ///
        /// public class SetFullStretchExample : MonoBehaviour
        /// {
        ///     private void Start()
        ///     {
        ///         RectTransform rectTransform = (RectTransform) transform;
        ///         rectTransform.SetFullStretch();
        ///     }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="rectTransform">対象の<see cref="RectTransform"/>.</param>
        /// <param name="left">左端のオフセット.</param>
        /// <param name="right">右端のオフセット.</param>
        /// <param name="top">上端のオフセット.</param>
        /// <param name="bottom">下端のオフセット.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetFullStretch(this RectTransform rectTransform, in float left = 0, in float right = 0, in float top = 0, in float bottom = 0)
        {
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = new Vector2(left, bottom);
            rectTransform.offsetMax = new Vector2(-right, -top);
        }

        /// <summary>
        /// ワールドスペースで計算された長方形の角を取得する.
        /// </summary>
        /// <remarks>
        /// <see cref="RectTransform.GetWorldCorners(Vector3[])"/>に同じ.
        /// </remarks>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// using System;
        /// using MinimalUtility;
        /// using UnityEngine;
        ///
        /// public class GetWorldCornersExampleClass : MonoBehaviour
        /// {
        ///     private RectTransform _rectTransform;
        ///
        ///     private void Start()
        ///     {
        ///         _rectTransform = (RectTransform)transform;
        ///         DisplayWorldCorners();
        ///     }
        ///
        ///     private void DisplayWorldCorners()
        ///     {
        ///         var v = (stackalloc Vector3[4]);
        ///         _rectTransform.GetWorldCorners(v);
        ///
        ///         Debug.Log("World Corners");
        ///         for (var i = 0; i < 4; i++)
        ///         {
        ///             Debug.Log("World Corner " + i + " : " + v[i]);
        ///         }
        ///     }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="rectTransform">対象の<see cref="RectTransform"/>.</param>
        /// <param name="fourCornersSpan">取得した角の座標を格納する<see cref="Span{T}"/>.</param>
        /// <returns>取得した角の座標を格納した<see cref="ReadOnlySpan{T}"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<Vector3> GetWorldCorners(this RectTransform rectTransform, in Span<Vector3> fourCornersSpan)
        {
            if (fourCornersSpan.IsEmpty || fourCornersSpan.Length < 4)
            {
                Debug.LogError("Calling GetWorldCorners with an array that is null or has less than 4 elements.");
            }
            else
            {
                rectTransform.GetCalculateLocalCorners(fourCornersSpan);
                var localToWorldMatrix = rectTransform.localToWorldMatrix;
                for (var i = 0; i < fourCornersSpan.Length; i++)
                {
                    fourCornersSpan[i] = localToWorldMatrix.MultiplyPoint(fourCornersSpan[i]);
                }
            }
            return fourCornersSpan[..4];
        }

        /// <summary>
        /// <see cref="RectTransform"/>のローカル空間で計算された長方形の角を取得する.
        /// </summary>
        /// <remarks>
        /// <see cref="RectTransform.GetLocalCorners(Vector3[])"/>に同じ.
        /// </remarks>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// using System;
        /// using MinimalUtility;
        /// using UnityEngine;
        ///
        /// public class GetLocalCornersExampleClass : MonoBehaviour
        /// {
        ///     private RectTransform _rectTransform;
        ///
        ///     private void Start()
        ///     {
        ///         _rectTransform = (RectTransform) transform;
        ///         DisplayLocalCorners();
        ///     }
        ///
        ///     private void DisplayLocalCorners()
        ///     {
        ///         var v = (stackalloc Vector3[4]);
        ///
        ///         _rectTransform.rotation = Quaternion.AngleAxis(45, Vector3.forward);
        ///         _rectTransform.GetLocalCorners(v);
        ///
        ///         Debug.Log("Local Corners");
        ///         for (var i = 0; i < 4; i++)
        ///         {
        ///             Debug.Log("Local Corner " + i + " : " + v[i]);
        ///         }
        ///     }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="rectTransform">対象の<see cref="RectTransform"/>.</param>
        /// <param name="fourCornersSpan">取得した角の座標を格納する<see cref="Span{T}"/>.</param>
        /// <returns>取得した角の座標を格納した<see cref="ReadOnlySpan{T}"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<Vector3> GetLocalCorners(this RectTransform rectTransform, in Span<Vector3> fourCornersSpan)
        {
            if (fourCornersSpan.IsEmpty || fourCornersSpan.Length < 4)
            {
                Debug.LogError("Calling GetLocalCorners with an array that is null or has less than 4 elements.");
            }
            else
            {
                rectTransform.GetCalculateLocalCorners(fourCornersSpan);
            }
            return fourCornersSpan[..4];
        }

        /// <summary>
        /// <see cref="RectTransform"/>のローカル空間で計算された長方形の角を取得する.
        /// </summary>
        /// <remarks>
        /// <see cref="RectTransform.GetLocalCorners(Vector3[])"/>に同じ.
        /// </remarks>
        /// <param name="rectTransform">対象の<see cref="RectTransform"/>.</param>
        /// <param name="fourCornersSpan">取得した角の座標を格納する<see cref="Span{T}"/>.</param>
        /// <returns>取得した角の座標を格納した<see cref="ReadOnlySpan{T}"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void GetCalculateLocalCorners(this RectTransform rectTransform, in Span<Vector3> fourCornersSpan)
        {
            var rect = rectTransform.rect;
            var (x, y, xMax, yMax) = (rect.x, rect.y, rect.xMax, rect.yMax);
            stackalloc Vector3[] { new(x, y), new(x, yMax), new(xMax, yMax), new(xMax, y) }.TryCopyTo(fourCornersSpan);
        }
    }
}
