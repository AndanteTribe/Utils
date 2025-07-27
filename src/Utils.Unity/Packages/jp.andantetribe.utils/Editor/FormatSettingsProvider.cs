#nullable enable

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AndanteTribe.Utils.Unity.Editor
{
    internal sealed class FormatSettingsProvider : SettingsProvider
    {
        private readonly Lazy<UnityEditor.Editor> _settingEditor = new(static () =>
        {
            var setting = FormatSettings.instance;
            setting.hideFlags = HideFlags.HideAndDontSave & ~HideFlags.NotEditable;
            var editor = default(UnityEditor.Editor);
            UnityEditor.Editor.CreateCachedEditor(setting, null, ref editor);
            return editor;
        });

        private FormatSettingsProvider(string path, SettingsScope scopes, IEnumerable<string>? keywords = null) : base(path, scopes, keywords)
        {
        }

        [SettingsProvider]
        public static SettingsProvider Create() =>
            new FormatSettingsProvider("Project/Editor/Format Settings", SettingsScope.Project, new[]{ "Format", "C#" })
            {
                label = "Format Settings"
            };

        public override void OnGUI(string searchContext)
        {
            using var check = new EditorGUI.ChangeCheckScope();
            _settingEditor.Value.OnInspectorGUI();
            if (check.changed)
            {
                FormatSettings.instance.Save();
            }
        }

        public override void OnDeactivate() => UnityEngine.Object.DestroyImmediate(_settingEditor.Value);
    }
}