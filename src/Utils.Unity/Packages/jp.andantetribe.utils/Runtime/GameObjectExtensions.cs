#nullable enable

using System.Runtime.CompilerServices;
using UnityEngine;

namespace AndanteTribe.Utils.Unity
{
    /// <summary>
    /// <see cref="GameObject"/>の拡張メソッド.
    /// </summary>
    public static class GameObjectExtensions
    {
        /// <summary>
        /// ゲームオブジェクトの階層パスを取得する.
        /// </summary>
        /// <param name="gameObject">対象のゲームオブジェクト.</param>
        /// <param name="includeScene">シーン名を含めるかどうか.</param>
        /// <returns>階層パス.</returns>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// using AndanteTribe.Utils.Unity;
        /// using UnityEngine;
        ///
        /// public class GetHierarchyPathExample : MonoBehaviour
        /// {
        ///     private void Start()
        ///     {
        ///         // シーン名を含める (デフォルト)
        ///         Debug.Log(this.gameObject.GetHierarchyPath());
        ///         // シーン名を含めない
        ///         Debug.Log(this.gameObject.GetHierarchyPath(false));
        ///     }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetHierarchyPath(this GameObject? gameObject, bool includeScene = true)
        {
            if (gameObject == null)
            {
                return "";
            }

            var sb = new DefaultInterpolatedStringHandler(0, 0, null, stackalloc char[256]);
            if (includeScene)
            {
                sb.AppendLiteral(gameObject.scene.name ?? "Unsaved Scene");
                sb.AppendLiteral("/");
            }

            GetTransformPath(gameObject.transform, ref sb);
            return sb.ToStringAndClear();

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static void GetTransformPath(Transform transform, ref DefaultInterpolatedStringHandler sb)
            {
                if (transform.parent != null)
                {
                    GetTransformPath(transform.parent, ref sb);
                    sb.AppendLiteral("/");
                }
                sb.AppendLiteral(transform.name);
            }
        }
    }
}


