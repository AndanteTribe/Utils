#nullable enable

using System;
using UnityEditor;
using UnityEngine;

namespace AndanteTribe.Utils.Editor
{
    /// <summary>
    /// <see cref="Animator.keepAnimatorStateOnDisable"/>や<see cref="Animator.writeDefaultValuesOnDisable"/>も編集できる
    /// <see cref="Animator"/>カスタムエディタ.
    /// </summary>
    [CustomEditor(typeof(Animator))]
    public class AnimatorInspector : UnityComponentInspector<Animator>
    {
        private readonly Lazy<GUIContent> _keepAnimatorStateStyle = new (static () =>
        {
            const string displayName = "Keep Animator State On Disable";
            const string tooltip = "GameObjectが無効化された時にAnimatorControllerの状態を保持するかどうか";
            return EditorGUIUtility.TrTextContent(displayName + "|" + tooltip, displayName, tooltip, null);
        });

        private readonly Lazy<GUIContent> _writeDefaultValuesStyle = new (static () =>
        {
            const string displayName = "Write Default Values On Disable";
            const string tooltip = "Animatorが無効化された時にデフォルト値を書き込むかどうか";
            return EditorGUIUtility.TrTextContent(displayName + "|" + tooltip, displayName, tooltip, null);
        });

        private SerializedProperty? _keepAnimatorStateOnDisable;
        private SerializedProperty? _writeDefaultValuesOnDisable;

        /// <inheritdoc/>
        protected override string InspectorTypeName => "UnityEditor.AnimatorInspector";

        /// <inheritdoc/>
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            _keepAnimatorStateOnDisable ??= serializedObject.FindProperty("m_KeepAnimatorStateOnDisable");
            _writeDefaultValuesOnDisable ??= serializedObject.FindProperty("m_WriteDefaultValuesOnDisable");

            EditorGUILayout.PropertyField(_keepAnimatorStateOnDisable, _keepAnimatorStateStyle.Value);
            EditorGUILayout.PropertyField(_writeDefaultValuesOnDisable, _writeDefaultValuesStyle.Value);

            serializedObject.ApplyModifiedProperties();
        }
    }
}