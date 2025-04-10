#if !NET6_0_OR_GREATER && UNITY_2022_3_OR_NEWER
#nullable enable

namespace System.Runtime.CompilerServices
{
    public ref partial struct DefaultInterpolatedStringHandler
    {
        public void AppendFormatted(UnityEngine.Bounds value)
        {
            // If there's a custom formatter, always use it.
            if (_hasCustomFormatter)
            {
                AppendCustomFormatter(value, format: null);
                return;
            }

            // The value should be able to format itself directly into our buffer, so do.
            var s = value.ToString(format: null, _provider); // constrained call avoiding boxing for value types

            AppendLiteral(s);
        }

        public void AppendFormatted(UnityEngine.Bounds value, int alignment)
        {
            int startingPos = _pos;
            AppendFormatted(value);
            if (alignment != 0)
            {
                AppendOrInsertAlignmentIfNeeded(startingPos, alignment);
            }
        }

        public void AppendFormatted(UnityEngine.Bounds value, string? format)
        {
            // If there's a custom formatter, always use it.
            if (_hasCustomFormatter)
            {
                AppendCustomFormatter(value, format);
                return;
            }

            // The value should be able to format itself directly into our buffer, so do.
            var s = value.ToString(format, _provider); // constrained call avoiding boxing for value types

            AppendLiteral(s);
        }

        public void AppendFormatted(UnityEngine.Bounds value, int alignment, string? format)
        {
            int startingPos = _pos;
            AppendFormatted(value, format);
            if (alignment != 0)
            {
                AppendOrInsertAlignmentIfNeeded(startingPos, alignment);
            }
        }

        public void AppendFormatted(UnityEngine.BoundsInt value)
        {
            // If there's a custom formatter, always use it.
            if (_hasCustomFormatter)
            {
                AppendCustomFormatter(value, format: null);
                return;
            }

            // The value should be able to format itself directly into our buffer, so do.
            var s = value.ToString(format: null, _provider); // constrained call avoiding boxing for value types

            AppendLiteral(s);
        }

        public void AppendFormatted(UnityEngine.BoundsInt value, int alignment)
        {
            int startingPos = _pos;
            AppendFormatted(value);
            if (alignment != 0)
            {
                AppendOrInsertAlignmentIfNeeded(startingPos, alignment);
            }
        }

        public void AppendFormatted(UnityEngine.BoundsInt value, string? format)
        {
            // If there's a custom formatter, always use it.
            if (_hasCustomFormatter)
            {
                AppendCustomFormatter(value, format);
                return;
            }

            // The value should be able to format itself directly into our buffer, so do.
            var s = value.ToString(format, _provider); // constrained call avoiding boxing for value types

            AppendLiteral(s);
        }

        public void AppendFormatted(UnityEngine.BoundsInt value, int alignment, string? format)
        {
            int startingPos = _pos;
            AppendFormatted(value, format);
            if (alignment != 0)
            {
                AppendOrInsertAlignmentIfNeeded(startingPos, alignment);
            }
        }

        public void AppendFormatted(UnityEngine.Color value)
        {
            // If there's a custom formatter, always use it.
            if (_hasCustomFormatter)
            {
                AppendCustomFormatter(value, format: null);
                return;
            }

            // The value should be able to format itself directly into our buffer, so do.
            var s = value.ToString(format: null, _provider); // constrained call avoiding boxing for value types

            AppendLiteral(s);
        }

        public void AppendFormatted(UnityEngine.Color value, int alignment)
        {
            int startingPos = _pos;
            AppendFormatted(value);
            if (alignment != 0)
            {
                AppendOrInsertAlignmentIfNeeded(startingPos, alignment);
            }
        }

        public void AppendFormatted(UnityEngine.Color value, string? format)
        {
            // If there's a custom formatter, always use it.
            if (_hasCustomFormatter)
            {
                AppendCustomFormatter(value, format);
                return;
            }

            // The value should be able to format itself directly into our buffer, so do.
            var s = value.ToString(format, _provider); // constrained call avoiding boxing for value types

            AppendLiteral(s);
        }

        public void AppendFormatted(UnityEngine.Color value, int alignment, string? format)
        {
            int startingPos = _pos;
            AppendFormatted(value, format);
            if (alignment != 0)
            {
                AppendOrInsertAlignmentIfNeeded(startingPos, alignment);
            }
        }

        public void AppendFormatted(UnityEngine.Color32 value)
        {
            // If there's a custom formatter, always use it.
            if (_hasCustomFormatter)
            {
                AppendCustomFormatter(value, format: null);
                return;
            }

            // The value should be able to format itself directly into our buffer, so do.
            var s = value.ToString(format: null, _provider); // constrained call avoiding boxing for value types

            AppendLiteral(s);
        }

        public void AppendFormatted(UnityEngine.Color32 value, int alignment)
        {
            int startingPos = _pos;
            AppendFormatted(value);
            if (alignment != 0)
            {
                AppendOrInsertAlignmentIfNeeded(startingPos, alignment);
            }
        }

        public void AppendFormatted(UnityEngine.Color32 value, string? format)
        {
            // If there's a custom formatter, always use it.
            if (_hasCustomFormatter)
            {
                AppendCustomFormatter(value, format);
                return;
            }

            // The value should be able to format itself directly into our buffer, so do.
            var s = value.ToString(format, _provider); // constrained call avoiding boxing for value types

            AppendLiteral(s);
        }

        public void AppendFormatted(UnityEngine.Color32 value, int alignment, string? format)
        {
            int startingPos = _pos;
            AppendFormatted(value, format);
            if (alignment != 0)
            {
                AppendOrInsertAlignmentIfNeeded(startingPos, alignment);
            }
        }

        public void AppendFormatted(UnityEngine.Matrix4x4 value)
        {
            // If there's a custom formatter, always use it.
            if (_hasCustomFormatter)
            {
                AppendCustomFormatter(value, format: null);
                return;
            }

            // The value should be able to format itself directly into our buffer, so do.
            var s = value.ToString(format: null, _provider); // constrained call avoiding boxing for value types

            AppendLiteral(s);
        }

        public void AppendFormatted(UnityEngine.Matrix4x4 value, int alignment)
        {
            int startingPos = _pos;
            AppendFormatted(value);
            if (alignment != 0)
            {
                AppendOrInsertAlignmentIfNeeded(startingPos, alignment);
            }
        }

        public void AppendFormatted(UnityEngine.Matrix4x4 value, string? format)
        {
            // If there's a custom formatter, always use it.
            if (_hasCustomFormatter)
            {
                AppendCustomFormatter(value, format);
                return;
            }

            // The value should be able to format itself directly into our buffer, so do.
            var s = value.ToString(format, _provider); // constrained call avoiding boxing for value types

            AppendLiteral(s);
        }

        public void AppendFormatted(UnityEngine.Matrix4x4 value, int alignment, string? format)
        {
            int startingPos = _pos;
            AppendFormatted(value, format);
            if (alignment != 0)
            {
                AppendOrInsertAlignmentIfNeeded(startingPos, alignment);
            }
        }

        public void AppendFormatted(UnityEngine.Plane value)
        {
            // If there's a custom formatter, always use it.
            if (_hasCustomFormatter)
            {
                AppendCustomFormatter(value, format: null);
                return;
            }

            // The value should be able to format itself directly into our buffer, so do.
            var s = value.ToString(format: null, _provider); // constrained call avoiding boxing for value types

            AppendLiteral(s);
        }

        public void AppendFormatted(UnityEngine.Plane value, int alignment)
        {
            int startingPos = _pos;
            AppendFormatted(value);
            if (alignment != 0)
            {
                AppendOrInsertAlignmentIfNeeded(startingPos, alignment);
            }
        }

        public void AppendFormatted(UnityEngine.Plane value, string? format)
        {
            // If there's a custom formatter, always use it.
            if (_hasCustomFormatter)
            {
                AppendCustomFormatter(value, format);
                return;
            }

            // The value should be able to format itself directly into our buffer, so do.
            var s = value.ToString(format, _provider); // constrained call avoiding boxing for value types

            AppendLiteral(s);
        }

        public void AppendFormatted(UnityEngine.Plane value, int alignment, string? format)
        {
            int startingPos = _pos;
            AppendFormatted(value, format);
            if (alignment != 0)
            {
                AppendOrInsertAlignmentIfNeeded(startingPos, alignment);
            }
        }

        public void AppendFormatted(UnityEngine.Quaternion value)
        {
            // If there's a custom formatter, always use it.
            if (_hasCustomFormatter)
            {
                AppendCustomFormatter(value, format: null);
                return;
            }

            // The value should be able to format itself directly into our buffer, so do.
            var s = value.ToString(format: null, _provider); // constrained call avoiding boxing for value types

            AppendLiteral(s);
        }

        public void AppendFormatted(UnityEngine.Quaternion value, int alignment)
        {
            int startingPos = _pos;
            AppendFormatted(value);
            if (alignment != 0)
            {
                AppendOrInsertAlignmentIfNeeded(startingPos, alignment);
            }
        }

        public void AppendFormatted(UnityEngine.Quaternion value, string? format)
        {
            // If there's a custom formatter, always use it.
            if (_hasCustomFormatter)
            {
                AppendCustomFormatter(value, format);
                return;
            }

            // The value should be able to format itself directly into our buffer, so do.
            var s = value.ToString(format, _provider); // constrained call avoiding boxing for value types

            AppendLiteral(s);
        }

        public void AppendFormatted(UnityEngine.Quaternion value, int alignment, string? format)
        {
            int startingPos = _pos;
            AppendFormatted(value, format);
            if (alignment != 0)
            {
                AppendOrInsertAlignmentIfNeeded(startingPos, alignment);
            }
        }

        public void AppendFormatted(UnityEngine.Ray value)
        {
            // If there's a custom formatter, always use it.
            if (_hasCustomFormatter)
            {
                AppendCustomFormatter(value, format: null);
                return;
            }

            // The value should be able to format itself directly into our buffer, so do.
            var s = value.ToString(format: null, _provider); // constrained call avoiding boxing for value types

            AppendLiteral(s);
        }

        public void AppendFormatted(UnityEngine.Ray value, int alignment)
        {
            int startingPos = _pos;
            AppendFormatted(value);
            if (alignment != 0)
            {
                AppendOrInsertAlignmentIfNeeded(startingPos, alignment);
            }
        }

        public void AppendFormatted(UnityEngine.Ray value, string? format)
        {
            // If there's a custom formatter, always use it.
            if (_hasCustomFormatter)
            {
                AppendCustomFormatter(value, format);
                return;
            }

            // The value should be able to format itself directly into our buffer, so do.
            var s = value.ToString(format, _provider); // constrained call avoiding boxing for value types

            AppendLiteral(s);
        }

        public void AppendFormatted(UnityEngine.Ray value, int alignment, string? format)
        {
            int startingPos = _pos;
            AppendFormatted(value, format);
            if (alignment != 0)
            {
                AppendOrInsertAlignmentIfNeeded(startingPos, alignment);
            }
        }

        public void AppendFormatted(UnityEngine.Ray2D value)
        {
            // If there's a custom formatter, always use it.
            if (_hasCustomFormatter)
            {
                AppendCustomFormatter(value, format: null);
                return;
            }

            // The value should be able to format itself directly into our buffer, so do.
            var s = value.ToString(format: null, _provider); // constrained call avoiding boxing for value types

            AppendLiteral(s);
        }

        public void AppendFormatted(UnityEngine.Ray2D value, int alignment)
        {
            int startingPos = _pos;
            AppendFormatted(value);
            if (alignment != 0)
            {
                AppendOrInsertAlignmentIfNeeded(startingPos, alignment);
            }
        }

        public void AppendFormatted(UnityEngine.Ray2D value, string? format)
        {
            // If there's a custom formatter, always use it.
            if (_hasCustomFormatter)
            {
                AppendCustomFormatter(value, format);
                return;
            }

            // The value should be able to format itself directly into our buffer, so do.
            var s = value.ToString(format, _provider); // constrained call avoiding boxing for value types

            AppendLiteral(s);
        }

        public void AppendFormatted(UnityEngine.Ray2D value, int alignment, string? format)
        {
            int startingPos = _pos;
            AppendFormatted(value, format);
            if (alignment != 0)
            {
                AppendOrInsertAlignmentIfNeeded(startingPos, alignment);
            }
        }

        public void AppendFormatted(UnityEngine.Rect value)
        {
            // If there's a custom formatter, always use it.
            if (_hasCustomFormatter)
            {
                AppendCustomFormatter(value, format: null);
                return;
            }

            // The value should be able to format itself directly into our buffer, so do.
            var s = value.ToString(format: null, _provider); // constrained call avoiding boxing for value types

            AppendLiteral(s);
        }

        public void AppendFormatted(UnityEngine.Rect value, int alignment)
        {
            int startingPos = _pos;
            AppendFormatted(value);
            if (alignment != 0)
            {
                AppendOrInsertAlignmentIfNeeded(startingPos, alignment);
            }
        }

        public void AppendFormatted(UnityEngine.Rect value, string? format)
        {
            // If there's a custom formatter, always use it.
            if (_hasCustomFormatter)
            {
                AppendCustomFormatter(value, format);
                return;
            }

            // The value should be able to format itself directly into our buffer, so do.
            var s = value.ToString(format, _provider); // constrained call avoiding boxing for value types

            AppendLiteral(s);
        }

        public void AppendFormatted(UnityEngine.Rect value, int alignment, string? format)
        {
            int startingPos = _pos;
            AppendFormatted(value, format);
            if (alignment != 0)
            {
                AppendOrInsertAlignmentIfNeeded(startingPos, alignment);
            }
        }

        public void AppendFormatted(UnityEngine.RectInt value)
        {
            // If there's a custom formatter, always use it.
            if (_hasCustomFormatter)
            {
                AppendCustomFormatter(value, format: null);
                return;
            }

            // The value should be able to format itself directly into our buffer, so do.
            var s = value.ToString(format: null, _provider); // constrained call avoiding boxing for value types

            AppendLiteral(s);
        }

        public void AppendFormatted(UnityEngine.RectInt value, int alignment)
        {
            int startingPos = _pos;
            AppendFormatted(value);
            if (alignment != 0)
            {
                AppendOrInsertAlignmentIfNeeded(startingPos, alignment);
            }
        }

        public void AppendFormatted(UnityEngine.RectInt value, string? format)
        {
            // If there's a custom formatter, always use it.
            if (_hasCustomFormatter)
            {
                AppendCustomFormatter(value, format);
                return;
            }

            // The value should be able to format itself directly into our buffer, so do.
            var s = value.ToString(format, _provider); // constrained call avoiding boxing for value types

            AppendLiteral(s);
        }

        public void AppendFormatted(UnityEngine.RectInt value, int alignment, string? format)
        {
            int startingPos = _pos;
            AppendFormatted(value, format);
            if (alignment != 0)
            {
                AppendOrInsertAlignmentIfNeeded(startingPos, alignment);
            }
        }

        public void AppendFormatted(UnityEngine.RectOffset value)
        {
            // If there's a custom formatter, always use it.
            if (_hasCustomFormatter)
            {
                AppendCustomFormatter(value, format: null);
                return;
            }

            // The value should be able to format itself directly into our buffer, so do.
            var s = value.ToString(format: null, _provider); // constrained call avoiding boxing for value types

            AppendLiteral(s);
        }

        public void AppendFormatted(UnityEngine.RectOffset value, int alignment)
        {
            int startingPos = _pos;
            AppendFormatted(value);
            if (alignment != 0)
            {
                AppendOrInsertAlignmentIfNeeded(startingPos, alignment);
            }
        }

        public void AppendFormatted(UnityEngine.RectOffset value, string? format)
        {
            // If there's a custom formatter, always use it.
            if (_hasCustomFormatter)
            {
                AppendCustomFormatter(value, format);
                return;
            }

            // The value should be able to format itself directly into our buffer, so do.
            var s = value.ToString(format, _provider); // constrained call avoiding boxing for value types

            AppendLiteral(s);
        }

        public void AppendFormatted(UnityEngine.RectOffset value, int alignment, string? format)
        {
            int startingPos = _pos;
            AppendFormatted(value, format);
            if (alignment != 0)
            {
                AppendOrInsertAlignmentIfNeeded(startingPos, alignment);
            }
        }

        public void AppendFormatted(UnityEngine.Vector2 value)
        {
            // If there's a custom formatter, always use it.
            if (_hasCustomFormatter)
            {
                AppendCustomFormatter(value, format: null);
                return;
            }

            // The value should be able to format itself directly into our buffer, so do.
            var s = value.ToString(format: null, _provider); // constrained call avoiding boxing for value types

            AppendLiteral(s);
        }

        public void AppendFormatted(UnityEngine.Vector2 value, int alignment)
        {
            int startingPos = _pos;
            AppendFormatted(value);
            if (alignment != 0)
            {
                AppendOrInsertAlignmentIfNeeded(startingPos, alignment);
            }
        }

        public void AppendFormatted(UnityEngine.Vector2 value, string? format)
        {
            // If there's a custom formatter, always use it.
            if (_hasCustomFormatter)
            {
                AppendCustomFormatter(value, format);
                return;
            }

            // The value should be able to format itself directly into our buffer, so do.
            var s = value.ToString(format, _provider); // constrained call avoiding boxing for value types

            AppendLiteral(s);
        }

        public void AppendFormatted(UnityEngine.Vector2 value, int alignment, string? format)
        {
            int startingPos = _pos;
            AppendFormatted(value, format);
            if (alignment != 0)
            {
                AppendOrInsertAlignmentIfNeeded(startingPos, alignment);
            }
        }

        public void AppendFormatted(UnityEngine.Vector2Int value)
        {
            // If there's a custom formatter, always use it.
            if (_hasCustomFormatter)
            {
                AppendCustomFormatter(value, format: null);
                return;
            }

            // The value should be able to format itself directly into our buffer, so do.
            var s = value.ToString(format: null, _provider); // constrained call avoiding boxing for value types

            AppendLiteral(s);
        }

        public void AppendFormatted(UnityEngine.Vector2Int value, int alignment)
        {
            int startingPos = _pos;
            AppendFormatted(value);
            if (alignment != 0)
            {
                AppendOrInsertAlignmentIfNeeded(startingPos, alignment);
            }
        }

        public void AppendFormatted(UnityEngine.Vector2Int value, string? format)
        {
            // If there's a custom formatter, always use it.
            if (_hasCustomFormatter)
            {
                AppendCustomFormatter(value, format);
                return;
            }

            // The value should be able to format itself directly into our buffer, so do.
            var s = value.ToString(format, _provider); // constrained call avoiding boxing for value types

            AppendLiteral(s);
        }

        public void AppendFormatted(UnityEngine.Vector2Int value, int alignment, string? format)
        {
            int startingPos = _pos;
            AppendFormatted(value, format);
            if (alignment != 0)
            {
                AppendOrInsertAlignmentIfNeeded(startingPos, alignment);
            }
        }

        public void AppendFormatted(UnityEngine.Vector3 value)
        {
            // If there's a custom formatter, always use it.
            if (_hasCustomFormatter)
            {
                AppendCustomFormatter(value, format: null);
                return;
            }

            // The value should be able to format itself directly into our buffer, so do.
            var s = value.ToString(format: null, _provider); // constrained call avoiding boxing for value types

            AppendLiteral(s);
        }

        public void AppendFormatted(UnityEngine.Vector3 value, int alignment)
        {
            int startingPos = _pos;
            AppendFormatted(value);
            if (alignment != 0)
            {
                AppendOrInsertAlignmentIfNeeded(startingPos, alignment);
            }
        }

        public void AppendFormatted(UnityEngine.Vector3 value, string? format)
        {
            // If there's a custom formatter, always use it.
            if (_hasCustomFormatter)
            {
                AppendCustomFormatter(value, format);
                return;
            }

            // The value should be able to format itself directly into our buffer, so do.
            var s = value.ToString(format, _provider); // constrained call avoiding boxing for value types

            AppendLiteral(s);
        }

        public void AppendFormatted(UnityEngine.Vector3 value, int alignment, string? format)
        {
            int startingPos = _pos;
            AppendFormatted(value, format);
            if (alignment != 0)
            {
                AppendOrInsertAlignmentIfNeeded(startingPos, alignment);
            }
        }

        public void AppendFormatted(UnityEngine.Vector3Int value)
        {
            // If there's a custom formatter, always use it.
            if (_hasCustomFormatter)
            {
                AppendCustomFormatter(value, format: null);
                return;
            }

            // The value should be able to format itself directly into our buffer, so do.
            var s = value.ToString(format: null, _provider); // constrained call avoiding boxing for value types

            AppendLiteral(s);
        }

        public void AppendFormatted(UnityEngine.Vector3Int value, int alignment)
        {
            int startingPos = _pos;
            AppendFormatted(value);
            if (alignment != 0)
            {
                AppendOrInsertAlignmentIfNeeded(startingPos, alignment);
            }
        }

        public void AppendFormatted(UnityEngine.Vector3Int value, string? format)
        {
            // If there's a custom formatter, always use it.
            if (_hasCustomFormatter)
            {
                AppendCustomFormatter(value, format);
                return;
            }

            // The value should be able to format itself directly into our buffer, so do.
            var s = value.ToString(format, _provider); // constrained call avoiding boxing for value types

            AppendLiteral(s);
        }

        public void AppendFormatted(UnityEngine.Vector3Int value, int alignment, string? format)
        {
            int startingPos = _pos;
            AppendFormatted(value, format);
            if (alignment != 0)
            {
                AppendOrInsertAlignmentIfNeeded(startingPos, alignment);
            }
        }

        public void AppendFormatted(UnityEngine.Vector4 value)
        {
            // If there's a custom formatter, always use it.
            if (_hasCustomFormatter)
            {
                AppendCustomFormatter(value, format: null);
                return;
            }

            // The value should be able to format itself directly into our buffer, so do.
            var s = value.ToString(format: null, _provider); // constrained call avoiding boxing for value types

            AppendLiteral(s);
        }

        public void AppendFormatted(UnityEngine.Vector4 value, int alignment)
        {
            int startingPos = _pos;
            AppendFormatted(value);
            if (alignment != 0)
            {
                AppendOrInsertAlignmentIfNeeded(startingPos, alignment);
            }
        }

        public void AppendFormatted(UnityEngine.Vector4 value, string? format)
        {
            // If there's a custom formatter, always use it.
            if (_hasCustomFormatter)
            {
                AppendCustomFormatter(value, format);
                return;
            }

            // The value should be able to format itself directly into our buffer, so do.
            var s = value.ToString(format, _provider); // constrained call avoiding boxing for value types

            AppendLiteral(s);
        }

        public void AppendFormatted(UnityEngine.Vector4 value, int alignment, string? format)
        {
            int startingPos = _pos;
            AppendFormatted(value, format);
            if (alignment != 0)
            {
                AppendOrInsertAlignmentIfNeeded(startingPos, alignment);
            }
        }

    }
}

#endif
