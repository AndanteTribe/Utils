#nullable enable

using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace AndanteTribe.Utils.Editor
{
    public static class UnityEditorToolbarUtils
    {
        private static readonly Type s_toolbarType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.Toolbar");

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddRight(Func<VisualElement> factory) =>
            EditorApplication.delayCall += () =>
            {
                var element = factory();
                element.style.overflow = Overflow.Visible;
                GetToolbarRoot()?.Q("ToolbarZoneRightAlign")?.Add(element);
            };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddLeft(Func<VisualElement> factory) =>
            EditorApplication.delayCall += () =>
            {
                var element = factory();
                element.style.overflow = Overflow.Visible;
                GetToolbarRoot()?.Q("ToolbarZoneLeftAlign")?.Add(element);
            };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddCenter(bool rightSide, Func<VisualElement> factory) =>
            EditorApplication.delayCall += () =>
            {
                var center = GetToolbarRoot()?.Q("ToolbarZonePlayMode");
                var element = factory();
                element.style.overflow = Overflow.Visible;
                if (rightSide)
                {
                    center?.Add(element);
                }
                else
                {
                    center?.Insert(0, element);
                }
            };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static VisualElement? GetToolbarRoot()
        {
            var toolbar = Resources.FindObjectsOfTypeAll(s_toolbarType).FirstOrDefault();
            if (toolbar == null) return null;

            var fieldInfo = s_toolbarType.GetField("m_Root", BindingFlags.NonPublic | BindingFlags.Instance);
            return fieldInfo?.GetValue(toolbar) as VisualElement;
        }
    }
}