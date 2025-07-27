#if ENABLE_TEXTMESHPRO
#nullable enable

using System;
using TMPro;
using UnityEngine;

namespace AndanteTribe.Utils.Unity.UGUI
{
    public abstract class TextView
    {
        [SerializeField]
        private TextMeshProUGUI? _text;
        [SerializeField, HideInInspector]
        protected string Format = "";

        public TextMeshProUGUI Text
        {
            get
            {
                if (_text == null)
                {
                    throw new NullReferenceException(nameof(_text));
                }
                return _text;
            }
        }
    }

    [Serializable]
    public sealed class IntTextView : TextView
    {
        [SerializeField, Tooltip("範囲")]
        private Vector2Int _range = new Vector2Int(int.MinValue, int.MaxValue);

        public void Bind(in int value)
        {
            var v = Math.Clamp(value, _range.x, _range.y);
            TrySetValue(v);

            void TrySetValue(in int value, int bufferLength = 16)
            {
                var buffer = (Span<char>)stackalloc char[bufferLength];
                if (value.TryFormat(buffer, out var written, Format))
                {
                    Text.SetCharArray(buffer[..written]);
                    return;
                }

                TrySetValue(value, bufferLength * 2);
            }
        }
    }

    [Serializable]
    public sealed class FloatTextView : TextView
    {
        [SerializeField, Tooltip("範囲")]
        private Vector2 _range = new Vector2(float.MinValue, float.MaxValue);

        public void Bind(in float value)
        {
            var v = Math.Clamp(value, _range.x, _range.y);
            TrySetValue(v);

            void TrySetValue(in float value, int bufferLength = 16)
            {
                var buffer = (Span<char>)stackalloc char[bufferLength];
                if (value.TryFormat(buffer, out var written, Format))
                {
                    Text.SetCharArray(buffer[..written]);
                    return;
                }

                TrySetValue(value, bufferLength * 2);
            }
        }
    }

    [Serializable]
    public sealed class DateTimeTextView : TextView
    {
        public void Bind(in DateTime value)
        {
            TrySetValue(value);

            void TrySetValue(in DateTime value, int bufferLength = 16)
            {
                var buffer = (Span<char>)stackalloc char[bufferLength];
                if (value.TryFormat(buffer, out var written, Format))
                {
                    Text.SetCharArray(buffer[..written]);
                    return;
                }

                TrySetValue(value, bufferLength * 2);
            }
        }
    }

    [Serializable]
    public sealed class TimeSpanTextView : TextView
    {
        public void Bind(in TimeSpan value)
        {
            TrySetValue(value);

            void TrySetValue(in TimeSpan value, int bufferLength = 16)
            {
                var buffer = (Span<char>)stackalloc char[bufferLength];
                if (value.TryFormat(buffer, out var written, Format))
                {
                    Text.SetCharArray(buffer[..written]);
                    return;
                }

                TrySetValue(value, bufferLength * 2);
            }
        }
    }
}

#endif