#nullable enable

using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace AndanteTribe.Utils.Unity.Editor
{
    public static class UnityEditorToolbarUtils
    {
        private static readonly Type s_toolbarType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.Toolbar");

        /// <summary>
        /// Unityエディタツールバーの右側に新しい要素を追加します。
        /// </summary>
        /// <param name="factory"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddRight(Func<VisualElement?> factory) =>
            EditorApplication.delayCall += () =>
            {
                var element = factory();
                if (element != null)
                {
                    element.style.overflow = Overflow.Visible;
                    GetToolbarRoot()?.Q("ToolbarZoneRightAlign")?.Add(element);
                }
            };

        /// <summary>
        /// Unityエディタツールバーの左側に新しい要素を追加します。
        /// </summary>
        /// <param name="factory"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddLeft(Func<VisualElement?> factory) =>
            EditorApplication.delayCall += () =>
            {
                var element = factory();
                if (element != null)
                {
                    element.style.overflow = Overflow.Visible;
                    GetToolbarRoot()?.Q("ToolbarZoneLeftAlign")?.Add(element);
                }
            };

        /// <summary>
        /// Unityエディタツールバーの中央に新しい要素を追加します。
        /// </summary>
        /// <example>
        /// <![CDATA[
        /// using AndanteTribe.Utils.Unity.Editor;
        /// using UnityEditor;
        /// using UnityEditor.SceneManagement;
        /// using UnityEngine.UIElements;
        ///
        /// [InitializeOnLoad]
        /// public static class ToolbarManager
        /// {
        ///     static ToolbarManager()
        ///     {
        ///         UnityEditorToolbarUtils.AddCenter(false, static () =>
        ///         {
        ///             var button = new Button { text = "Systemシーンをロード" };
        ///             button.RegisterCallback<ClickEvent>(static _ =>
        ///             {
        ///                 if (EditorApplication.isPlaying)
        ///                 {
        ///                     EditorApplication.isPlaying = false;
        ///                 }
        ///                 else
        ///                 {
        ///                     EditorSceneManager.OpenScene("Assets/Scenes/System.unity");
        ///                     EditorApplication.isPlaying = true;
        ///                 }
        ///             };
        ///             return button;
        ///         }
        ///     }
        /// }
        /// ]]>
        /// </example>
        /// <param name="rightSide">ゲーム再生ボタン群からみて右側に配置するかどうか.</param>
        /// <param name="factory"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddCenter(bool rightSide, Func<VisualElement?> factory)
        {
            EditorApplication.delayCall += () =>
            {
                var center = GetToolbarRoot()?.Q("ToolbarZonePlayMode");
                var element = factory();
                if (element != null)
                {
                    element.style.overflow = Overflow.Visible;
                    if (rightSide)
                    {
                        center?.Add(element);
                    }
                    else
                    {
                        center?.Insert(0, element);
                    }
                }
            };
        }

        /// <summary>
        /// 汎用性がありそうな倍速実装だけデフォルトで生やしておく.
        /// </summary>
        public static void AddTimeScaleSlider()
        {
            AddLeft(static () =>
            {
                var slider = new SliderInt(1, 5)
                {
                    style =
                    {
                        flexDirection = FlexDirection.Row,
                        alignItems = Align.FlexStart,
                        width = 150,
                    }
                };
                slider.Insert(0, new Image()
                {
                    image = (Texture2D)EditorGUIUtility.IconContent("d_UnityEditor.AnimationWindow").image,
                    style =
                    {
                        width = 16,
                        height = 16,
                        marginRight = 5
                    }
                });
                var label = new Label()
                {
                    text = string.Create(3, slider.value, static (span, i) =>
                    {
                        stackalloc char[] { 'x', ' ' }.CopyTo(span);
                        i.TryFormat(span[2..], out _);
                    }),
                    style =
                    {
                        marginLeft = 5,
                    }
                };
                slider.RegisterCallback<ChangeEvent<int>, Label>(static (v, label) =>
                {
                    Time.timeScale = v.newValue;
                    label.text = string.Create(3, v.newValue, static (span, i) =>
                    {
                        stackalloc char[] { 'x', ' ' }.CopyTo(span);
                        i.TryFormat(span[2..], out _);
                    });
                }, label);
                slider.Add(label);
                return slider;
            });
        }

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