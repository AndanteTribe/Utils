#nullable enable

using UnityEngine;
using UnityEngine.UIElements;

namespace AndanteTribe.Utils.Editor
{
    public static class UIElementUtils
    {
        public static VisualElement CreateBox(string label = "")
        {
            var box = new Box
            {
                style =
                {
                    paddingTop = 3,
                    paddingBottom = 3,
                    paddingRight = 5,
                    paddingLeft = 5,
                    marginTop = 3,
                    marginBottom = 3,

                    borderTopWidth = 1,
                    borderBottomWidth = 1,
                    borderLeftWidth = 1,
                    borderRightWidth = 1,
                    borderTopColor = Color.black,
                    borderBottomColor = Color.black,
                    borderLeftColor = Color.black,
                    borderRightColor = Color.black,
                    borderTopLeftRadius = 4,
                    borderTopRightRadius = 4,
                    borderBottomLeftRadius = 4,
                    borderBottomRightRadius = 4,
                }
            };
            if (!string.IsNullOrEmpty(label))
            {
                box.Add(new Label(label)
                {
                    style =
                    {
                        marginTop = 3,
                        marginBottom = 3,
                        unityFontStyleAndWeight = FontStyle.Bold
                    }
                });
            }
            return box;
        }
    }
}