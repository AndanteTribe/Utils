#if ENABLE_TEXTMESHPRO
#nullable enable

using System;
using AndanteTribe.Utils.Unity.UI;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace AndanteTribe.Utils.Unity.Editor
{
    [CustomPropertyDrawer(typeof(TextView), true)]
    public class TextViewDrawer : PropertyDrawer
    {
        /// <inheritdoc />
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var root = UIElementUtils.CreateBox();
            var foldout = new Foldout { style = { marginLeft = 10, paddingRight = 3 } };
            root.Add(foldout);
            var foldoutCheck = foldout.Q(className: Foldout.checkmarkUssClassName);
            foldoutCheck.parent.Add(new Label(property.displayName)
            {
                style =
                {
                    unityFontStyleAndWeight = FontStyle.Bold,
                    flexGrow = 1
                }
            });
            var objectField = new ObjectField()
            {
                bindingPath = "_textMeshProUGUI",
                objectType = typeof(TMPro.TextMeshProUGUI),
                style = { flexGrow = 1 }
            };
            objectField.Q(className: ObjectField.ussClassName + "-display__label").style.marginLeft = 2;
            foldoutCheck.parent.Add(objectField);

            var formatProperty = property.FindPropertyRelative("Format");
            var value = formatProperty.stringValue;
            var defaultValue = new FormatSettings.KeyValuePair { Value = string.IsNullOrEmpty(value) ? "" : value };
            var options = FormatSettings.instance.GetFormats(property.type switch
            {
                nameof(IntTextView) => typeof(int),
                nameof(FloatTextView) => typeof(float),
                nameof(DateTimeTextView) => typeof(DateTime),
                nameof(TimeSpanTextView) => typeof(TimeSpan),
                _ => throw new NotImplementedException(property.type)
            });
            var popupField = new PopupField<FormatSettings.KeyValuePair>("Format", options, defaultValue)
            {
                style = { marginBottom = 3 },
            };
            popupField.RegisterCallback<ChangeEvent<FormatSettings.KeyValuePair>, SerializedProperty>(static (evt, property) =>
            {
                property.stringValue = evt.newValue.Value;
                property.serializedObject.ApplyModifiedProperties();
            }, formatProperty);
            foldout.Add(popupField);

            var range = property.FindPropertyRelative("_range");
            if (range != null)
            {
                switch (range.propertyType)
                {
                    case SerializedPropertyType.Vector2:
                        foldout.Add(new FloatField("Min")
                        {
                            bindingPath = "_range.x",
                            style = { marginBottom = 3 }
                        });
                        foldout.Add(new FloatField("Max")
                        {
                            bindingPath = "_range.y",
                            style = { marginBottom = 3 }
                        });
                        break;
                    case SerializedPropertyType.Vector2Int:
                        foldout.Add(new IntegerField("Min")
                        {
                            bindingPath = "_range.x",
                            style = { marginBottom = 3 }
                        });
                        foldout.Add(new IntegerField("Max")
                        {
                            bindingPath = "_range.y",
                            style = { marginBottom = 3 }
                        });
                        break;
                    default:
                        throw new NotImplementedException(range.propertyType.ToString());
                }
            }

            return root;
        }
    }
}

#endif