#nullable enable

using System;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace AndanteTribe.Utils.Unity.Editor
{
    [CustomPropertyDrawer(typeof(IObjectReference<>), true)]
    public class ObjectReferenceDrawer : PropertyDrawer
    {
        private static readonly Lazy<Texture2D> s_settingIcon = new(static () =>
            (Texture2D)EditorGUIUtility.Load("SettingsIcon"));

        private static readonly Lazy<Type[]> s_objectReferenceTypes = new(static () =>
        {
            return TypeCache.GetTypesDerivedFrom(typeof(IObjectReference<>))
                .Where(t => t.IsDefined(typeof(SerializableAttribute), false)).ToArray();
        });

        private static readonly Lazy<TypeCache.TypeCollection> s_unityObjectTypes = new(static () =>
            TypeCache.GetTypesDerivedFrom(typeof(UnityEngine.Object)));

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var root = UIElementUtils.CreateBox(property.displayName);

            // SerializeReferenceの時だけ表示
            if (property.propertyType == SerializedPropertyType.ManagedReference)
            {
                SetOptionButton(root, property);
            }

            var valueProperty = property.FindPropertyRelative("_value");
            if (valueProperty != null)
            {
                root.Add(new PropertyField(valueProperty, "Value"));
            }

            return root;
        }

        private static void SetOptionButton(VisualElement root, SerializedProperty property)
        {
            var opBtn = new Button()
            {
                style =
                {
                    height = 15,
                    width = 15,
                    left = 5,
                    backgroundImage = s_settingIcon.Value,
                    backgroundColor = Color.white
                }
            };
            opBtn.RegisterCallback<ClickEvent, SerializedProperty>(static (evt, property) =>
            {
                var btn = (Button)evt.currentTarget;
                var genericDropdown = new GenericDropdownMenu();
                foreach (var type in s_objectReferenceTypes.Value)
                {
                    genericDropdown.AddItem(type.Name.AsSpan()[..^5].ToString(), false, static d =>
                    {
                        var data = (DropDownData)d;
                        var type = data.Type;
                        var property = data.Property;
                        var genericType = property.managedReferenceValue?.GetType().GetGenericArguments()[0];

                        // managedReferenceValueがnullの時は型が取得できないので、無理矢理文字列から型判定
                        if (genericType == null)
                        {
                            var fieldName = property.managedReferenceFieldTypename.AsSpan();
                            var i = fieldName.IndexOf("[[");
                            var typeName = fieldName.Slice(i + 2);
                            typeName = typeName[..typeName.IndexOf(',')];
                            genericType = Type.GetType(typeName.ToString());
                            if (genericType == null)
                            {
                                foreach (var unityType in s_unityObjectTypes.Value)
                                {
                                    if (typeName.SequenceEqual(unityType.FullName))
                                    {
                                        genericType = unityType;
                                        break;
                                    }
                                }
                            }
                        }

                        property.managedReferenceValue = Activator.CreateInstance(type.MakeGenericType(genericType));
                        property.serializedObject.ApplyModifiedProperties();
                    }, new DropDownData { Type = type, Property = property });
                }

                genericDropdown.DropDown(btn.worldBound, btn, false);
            }, property);

            var label = root.Q<Label>(property.displayName);
            label.style.flexDirection = FlexDirection.RowReverse;
            label.Add(opBtn);
        }

        private sealed class DropDownData
        {
            public Type Type { get; init; } = null!;
            public SerializedProperty Property { get; init; } = null!;
        }
    }
}