#if ENABLE_UNITASK && ENABLE_LITMOTION
#nullable enable

using AndanteTribe.Utils.Unity.Tasks;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace AndanteTribe.Utils.Unity.Editor
{
    [CustomPropertyDrawer(typeof(ToastControllerCore), true)]
    public class ToastControllerCoreDrawer : PropertyDrawer
    {
        /// <inheritdoc />
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var root = UIElementUtils.CreateBox("トースト通知設定");

            foreach (var fieldName in ("_spacingY", "_animDuration", "_displayDuration", "_firstExtraDisplayDuration", "_consecutiveWaitDuration"))
            {
                var p = property.FindPropertyRelative(fieldName);
                root.Add(new PropertyField(p));
            }

            return root;
        }
    }
}

#endif