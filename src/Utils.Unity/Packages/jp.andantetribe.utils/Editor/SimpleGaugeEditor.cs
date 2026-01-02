#if ENABLE_UGUI
#nullable enable

using AndanteTribe.Utils.Unity.UI;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace AndanteTribe.Utils.Unity.Editor
{
    [CustomEditor(typeof(SimpleGauge))]
    public class SimpleGaugeEditor : UnityEditor.Editor
    {
        /// <inheritdoc/>
        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();
            root.Add(new PropertyField(serializedObject.FindProperty("_mode")));
            root.Add(new PropertyField(serializedObject.FindProperty("_value")));
            return root;
        }
    }
}

#endif