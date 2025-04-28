#nullable enable

using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace AndanteTribe.Utils.Editor
{
    [CustomPropertyDrawer(typeof(IObjectReference<>), true)]
    public class ObjectReferenceDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var root = UIElementUtils.CreateBox(property.displayName);
            var valueProperty = property.FindPropertyRelative("_value");
            if (valueProperty != null)
            {
                root.Add(new PropertyField(valueProperty, "Value"));
            }
            return root;
        }
    }
}