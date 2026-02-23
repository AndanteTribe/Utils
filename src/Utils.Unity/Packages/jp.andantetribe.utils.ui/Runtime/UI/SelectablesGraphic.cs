#if ENABLE_UGUI
#nullable enable

using System;
using UnityEngine;
using UnityEngine.UI;

namespace AndanteTribe.Utils.Unity.UI
{
    /// <summary>
    /// <see cref="UnityEngine.UI.Selectable"/>を使用するうえで最低限の機能を持つグラフィック.
    /// </summary>
    /// <remarks>
    /// 子オブジェクトのGraphicをキャッシュして選択時に色変化を伝播させる
    /// </remarks>
    public sealed class SelectablesGraphic : EmptyGraphic
    {
        /// <summary>
        /// 子オブジェクト以下のGraphicのキャッシュ.
        /// </summary>
        private ReadOnlyMemory<Graphic> _childGraphics;

        /// <inheritdoc>
        /// Selectableの押下時の色変化はここを呼び出しているみたいなので、ちょっと処理を上書き.
        /// </inheritdoc>
        public override void CrossFadeColor(Color targetColor, float duration, bool ignoreTimeScale, bool useAlpha)
        {
            if (_childGraphics.IsEmpty)
            {
                _childGraphics = this.GetComponentsInOnlyChildren<Graphic>();
            }
            foreach (var childGraphic in _childGraphics)
            {
                childGraphic.CrossFadeColor(targetColor, duration, ignoreTimeScale, useAlpha);
            }
        }
    }
}
#endif