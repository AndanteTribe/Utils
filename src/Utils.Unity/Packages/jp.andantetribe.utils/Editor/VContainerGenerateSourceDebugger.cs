#if ENABLE_VCONTAINER
#nullable enable

using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;

namespace AndanteTribe.Utils.Unity.Editor
{
    public class VContainerGenerateSourceDebugger : EditorWindow
    {
        [MenuItem("Tools/AndanteTribe.Utils/VContainerGenerateSourceDebugger")]
        private static void ShowWindow()
        {
            var window = GetWindow<VContainerGenerateSourceDebugger>();
            window.titleContent = new GUIContent("GenerateSourceDebugger");
            window.Show();
        }

        private void CreateGUI()
        {
            // ScrollView を作る
            var scrollView = new ScrollView
            {
                style =
                {
                    flexGrow = 1,
                    paddingLeft = 2,
                    paddingRight = 2,
                    paddingTop = 2,
                    paddingBottom = 2
                }
            };

            rootVisualElement.Add(scrollView);

            var next = false;

            foreach (var type in TypeCache.GetTypesDerivedFrom<IInjector>()
                         .Where(static t => t.FullName != "VContainer.Internal.ReflectionInjector")
                         .OrderBy(static t => t.FullName, StringComparer.Ordinal))
            {
                var label = new Label(type.FullName)
                {
                    style =
                    {
                        backgroundColor = next ? new Color(0.85f, 0.85f, 0.85f, 1f) : new Color(0.35f, 0.35f, 0.35f, 1f),
                        color = next ? new Color(0.15f, 0.15f, 0.15f, 1f) : new Color(0.95f, 0.95f, 0.95f, 1f),
                        paddingLeft = 8,
                        paddingTop = 4,
                        paddingBottom = 4,
                        unityFontStyleAndWeight = FontStyle.Bold,
                        marginBottom = 1
                    }
                };
                scrollView.Add(label);
                next = !next;
            }
        }
    }
}

#endif