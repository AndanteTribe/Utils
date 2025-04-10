#if ENABLE_TEXTMESHPRO
#nullable enable

using System;
using System.Buffers;
using System.Threading;
using TMPro;
using UnityEngine;

namespace AndanteTribe.Utils.UGUI
{
    public abstract class TextView
    {
        [SerializeField]
        private TextMeshProUGUI? _textMeshProUGUI;
        [SerializeField, HideInInspector]
        protected string Format = "";

        [NonSerialized]
        protected char[] Cache = Array.Empty<char>();
        protected CancellationTokenRegistration Subscription { get; private set; }

        public TextMeshProUGUI TextMeshProUGUI => _textMeshProUGUI ?? throw new NullReferenceException(nameof(_textMeshProUGUI));

        protected void SetSubscription()
        {
            var restoreFlow = false;
            if (!ExecutionContext.IsFlowSuppressed())
            {
                ExecutionContext.SuppressFlow();
                restoreFlow = true;
            }

            try
            {
                Subscription = TextMeshProUGUI.destroyCancellationToken
                    .Register(static cache => ArrayPool<char>.Shared.Return((char[])cache), Cache, false);
            }
            finally
            {
                if (restoreFlow)
                {
                    ExecutionContext.RestoreFlow();
                }
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
            if (v.TryFormat(Cache, out var written, Format))
            {
                TextMeshProUGUI.SetCharArray(Cache, 0, written);
                return;
            }

            var bufferLength = Math.Max(Cache.Length, 16);
            Subscription.Dispose();
            (Cache, written) = GetCharArray(v, Format, bufferLength);
            TextMeshProUGUI.SetCharArray(Cache, 0, written);
            SetSubscription();

            static (char[], int) GetCharArray(in int value, in ReadOnlySpan<char> format, int bufferLength)
            {
                var span = (Span<char>)stackalloc char[bufferLength];
                if (value.TryFormat(span, out var written, format))
                {
                    var array = ArrayPool<char>.Shared.Rent(written);
                    span[..written].CopyTo(array);
                    return (array, written);
                }
                return GetCharArray(value, format, bufferLength * 2);
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
            if (v.TryFormat(Cache, out var written, Format))
            {
                TextMeshProUGUI.SetCharArray(Cache, 0, written);
                return;
            }

            var bufferLength = Math.Max(Cache.Length, 16);
            Subscription.Dispose();
            (Cache, written) = GetCharArray(v, Format, bufferLength);
            TextMeshProUGUI.SetCharArray(Cache, 0, written);
            SetSubscription();

            static (char[], int) GetCharArray(in float value, in ReadOnlySpan<char> format, int bufferLength)
            {
                var span = (Span<char>)stackalloc char[bufferLength];
                if (value.TryFormat(span, out var written, format))
                {
                    var array = ArrayPool<char>.Shared.Rent(written);
                    span[..written].CopyTo(array);
                    return (array, written);
                }
                return GetCharArray(value, format, bufferLength * 2);
            }
        }
    }

    [Serializable]
    public sealed class DateTimeTextView : TextView
    {
        public void Bind(in DateTime value)
        {
            if (value.TryFormat(Cache, out var written, Format))
            {
                TextMeshProUGUI.SetCharArray(Cache, 0, written);
                return;
            }

            var bufferLength = Math.Max(Cache.Length, 16);
            Subscription.Dispose();
            (Cache, written) = GetCharArray(value, Format, bufferLength);
            TextMeshProUGUI.SetCharArray(Cache, 0, written);
            SetSubscription();

            static (char[], int) GetCharArray(in DateTime value, in ReadOnlySpan<char> format, int bufferLength)
            {
                var span = (Span<char>)stackalloc char[bufferLength];
                if (value.TryFormat(span, out var written, format))
                {
                    var array = ArrayPool<char>.Shared.Rent(written);
                    span[..written].CopyTo(array);
                    return (array, written);
                }
                return GetCharArray(value, format, bufferLength * 2);
            }
        }
    }

    [Serializable]
    public sealed class TimeSpanTextView : TextView
    {
        public void Bind(in TimeSpan value)
        {
            if (value.TryFormat(Cache, out var written, Format))
            {
                TextMeshProUGUI.SetCharArray(Cache, 0, written);
                return;
            }

            var bufferLength = Math.Max(Cache.Length, 16);
            Subscription.Dispose();
            (Cache, written) = GetCharArray(value, Format, bufferLength);
            TextMeshProUGUI.SetCharArray(Cache, 0, written);
            SetSubscription();

            static (char[], int) GetCharArray(in TimeSpan value, in ReadOnlySpan<char> format, int bufferLength)
            {
                var span = (Span<char>)stackalloc char[bufferLength];
                if (value.TryFormat(span, out var written, format))
                {
                    var array = ArrayPool<char>.Shared.Rent(written);
                    span[..written].CopyTo(array);
                    return (array, written);
                }
                return GetCharArray(value, format, bufferLength * 2);
            }
        }
    }
}

#endif