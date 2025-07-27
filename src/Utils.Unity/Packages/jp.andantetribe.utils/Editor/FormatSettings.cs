#nullable enable

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AndanteTribe.Utils.Unity.Editor
{
    [FilePath("ProjectSettings/" + nameof(FormatSettings) + ".asset", FilePathAttribute.Location.ProjectFolder)]
    internal sealed class FormatSettings : ScriptableSingleton<FormatSettings>
    {
        [Serializable]
        internal struct KeyValuePair : IEquatable<KeyValuePair>
        {
            [SerializeField]
            private string _key;
            [SerializeField]
            private string _value;

            public string Key
            {
                get => _key;
                init => _key = value;
            }
            public string Value
            {
                get => _value;
                init => _value = value;
            }

            public override string ToString() => string.IsNullOrWhiteSpace(_value) ? "(undefined)" : _key + " (" + _value + ")";

            public bool Equals(KeyValuePair other) => _value == other._value;
        }

        [CustomPropertyDrawer(typeof(KeyValuePair))]
        private sealed class KeyValuePairDrawer : PropertyDrawer
        {
            public override void OnGUI(Rect position, SerializedProperty property, GUIContent _)
            {
                using (new EditorGUI.IndentLevelScope(0))
                {
                    var fieldWidth = position.width / 2f;
                    var size = new Vector2(fieldWidth - 5, position.height - 2);
                    var keyRect = new Rect(position.position, size);
                    var valueRect = new Rect(position.position + new Vector2(fieldWidth + 5, 0), size);

                    EditorGUI.PropertyField(keyRect, property.FindPropertyRelative("_key"), GUIContent.none);
                    EditorGUI.PropertyField(valueRect, property.FindPropertyRelative("_value"), GUIContent.none);
                }
            }
        }

        [SerializeField]
        private KeyValuePair[] _intFormats = Array.Empty<KeyValuePair>();
        [SerializeField]
        private KeyValuePair[] _floatFormats = Array.Empty<KeyValuePair>();
        [SerializeField]
        private KeyValuePair[] _dateTimeFormats = Array.Empty<KeyValuePair>();
        [SerializeField]
        private KeyValuePair[] _timeSpanFormats = Array.Empty<KeyValuePair>();

        public List<KeyValuePair> GetFormats(Type type)
        {
            var list = new List<KeyValuePair>(1) { new KeyValuePair { Value = "" } };
            if (type == typeof(int))
            {
                list.AddRange(_intFormats);
                return list;
            }
            if (type == typeof(float))
            {
                list.AddRange(_floatFormats);
                return list;
            }
            if (type == typeof(DateTime))
            {
                list.AddRange(_dateTimeFormats);
                return list;
            }
            if (type == typeof(TimeSpan))
            {
                list.AddRange(_timeSpanFormats);
                return list;
            }
            throw new NotImplementedException(type.Name);
        }

        public void Save() => Save(true);
    }
}