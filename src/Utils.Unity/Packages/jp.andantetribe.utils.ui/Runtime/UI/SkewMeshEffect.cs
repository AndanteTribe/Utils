#if ENABLE_UGUI
#nullable enable

using UnityEngine;
using UnityEngine.UI;

namespace AndanteTribe.Utils.Unity.UI
{
    /// <summary>
    /// UGUIのメッシュを斜めに変形するエフェクト.
    /// </summary>
    /// <remarks>
    /// <see cref="Image"/>のゲームオブジェクトにアタッチすると、見た目も切り口も斜めなゲージなどを表現できる.
    /// </remarks>
    [RequireComponent(typeof(Graphic))]
    public class SkewMeshEffect : BaseMeshEffect
    {
        [SerializeField]
        private Vector2 _skew = new Vector2(0.5f, 0f);

        /// <inheritdoc />
        public override void ModifyMesh(VertexHelper helper)
        {
            if (!IsActive())
            {
                return;
            }

            var vertex = new UIVertex();
            for (var i = 0; i < helper.currentVertCount; i++)
            {
                helper.PopulateUIVertex(ref vertex, i);
                var pos = vertex.position;
                pos.x += _skew.x * pos.y;
                pos.y += _skew.y * pos.x;
                vertex.position = pos;
                helper.SetUIVertex(vertex, i);
            }
        }
    }
}

#endif
