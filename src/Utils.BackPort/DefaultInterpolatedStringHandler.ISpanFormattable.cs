#if !NET6_0_OR_GREATER
#nullable enable

namespace System.Runtime.CompilerServices
{
    public ref partial struct DefaultInterpolatedStringHandler
    {
        public void AppendFormatted(byte value)
        {
            // If there's a custom formatter, always use it.
            if (_hasCustomFormatter)
            {
                AppendCustomFormatter(value, format: null);
                return;
            }

            int charsWritten;
            while (!value.TryFormat(_chars.Slice(_pos), out charsWritten, format: null, _provider)) // constrained call avoiding boxing for value types
            {
                Grow();
            }

            _pos += charsWritten;
        }

        public void AppendFormatted(byte value, int alignment)
        {
            int startingPos = _pos;
            AppendFormatted(value);
            if (alignment != 0)
            {
                AppendOrInsertAlignmentIfNeeded(startingPos, alignment);
            }
        }

        public void AppendFormatted(byte value, string? format)
        {
            // If there's a custom formatter, always use it.
            if (_hasCustomFormatter)
            {
                AppendCustomFormatter(value, format);
                return;
            }

            int charsWritten;
            while (!value.TryFormat(_chars.Slice(_pos), out charsWritten, format: format, _provider)) // constrained call avoiding boxing for value types
            {
                Grow();
            }

            _pos += charsWritten;
        }

        public void AppendFormatted(byte value, int alignment, string? format)
        {
            int startingPos = _pos;
            AppendFormatted(value, format);
            if (alignment != 0)
            {
                AppendOrInsertAlignmentIfNeeded(startingPos, alignment);
            }
        }

        public void AppendFormatted(sbyte value)
        {
            // If there's a custom formatter, always use it.
            if (_hasCustomFormatter)
            {
                AppendCustomFormatter(value, format: null);
                return;
            }

            int charsWritten;
            while (!value.TryFormat(_chars.Slice(_pos), out charsWritten, format: null, _provider)) // constrained call avoiding boxing for value types
            {
                Grow();
            }

            _pos += charsWritten;
        }

        public void AppendFormatted(sbyte value, int alignment)
        {
            int startingPos = _pos;
            AppendFormatted(value);
            if (alignment != 0)
            {
                AppendOrInsertAlignmentIfNeeded(startingPos, alignment);
            }
        }

        public void AppendFormatted(sbyte value, string? format)
        {
            // If there's a custom formatter, always use it.
            if (_hasCustomFormatter)
            {
                AppendCustomFormatter(value, format);
                return;
            }

            int charsWritten;
            while (!value.TryFormat(_chars.Slice(_pos), out charsWritten, format: format, _provider)) // constrained call avoiding boxing for value types
            {
                Grow();
            }

            _pos += charsWritten;
        }

        public void AppendFormatted(sbyte value, int alignment, string? format)
        {
            int startingPos = _pos;
            AppendFormatted(value, format);
            if (alignment != 0)
            {
                AppendOrInsertAlignmentIfNeeded(startingPos, alignment);
            }
        }

        public void AppendFormatted(short value)
        {
            // If there's a custom formatter, always use it.
            if (_hasCustomFormatter)
            {
                AppendCustomFormatter(value, format: null);
                return;
            }

            int charsWritten;
            while (!value.TryFormat(_chars.Slice(_pos), out charsWritten, format: null, _provider)) // constrained call avoiding boxing for value types
            {
                Grow();
            }

            _pos += charsWritten;
        }

        public void AppendFormatted(short value, int alignment)
        {
            int startingPos = _pos;
            AppendFormatted(value);
            if (alignment != 0)
            {
                AppendOrInsertAlignmentIfNeeded(startingPos, alignment);
            }
        }

        public void AppendFormatted(short value, string? format)
        {
            // If there's a custom formatter, always use it.
            if (_hasCustomFormatter)
            {
                AppendCustomFormatter(value, format);
                return;
            }

            int charsWritten;
            while (!value.TryFormat(_chars.Slice(_pos), out charsWritten, format: format, _provider)) // constrained call avoiding boxing for value types
            {
                Grow();
            }

            _pos += charsWritten;
        }

        public void AppendFormatted(short value, int alignment, string? format)
        {
            int startingPos = _pos;
            AppendFormatted(value, format);
            if (alignment != 0)
            {
                AppendOrInsertAlignmentIfNeeded(startingPos, alignment);
            }
        }

        public void AppendFormatted(ushort value)
        {
            // If there's a custom formatter, always use it.
            if (_hasCustomFormatter)
            {
                AppendCustomFormatter(value, format: null);
                return;
            }

            int charsWritten;
            while (!value.TryFormat(_chars.Slice(_pos), out charsWritten, format: null, _provider)) // constrained call avoiding boxing for value types
            {
                Grow();
            }

            _pos += charsWritten;
        }

        public void AppendFormatted(ushort value, int alignment)
        {
            int startingPos = _pos;
            AppendFormatted(value);
            if (alignment != 0)
            {
                AppendOrInsertAlignmentIfNeeded(startingPos, alignment);
            }
        }

        public void AppendFormatted(ushort value, string? format)
        {
            // If there's a custom formatter, always use it.
            if (_hasCustomFormatter)
            {
                AppendCustomFormatter(value, format);
                return;
            }

            int charsWritten;
            while (!value.TryFormat(_chars.Slice(_pos), out charsWritten, format: format, _provider)) // constrained call avoiding boxing for value types
            {
                Grow();
            }

            _pos += charsWritten;
        }

        public void AppendFormatted(ushort value, int alignment, string? format)
        {
            int startingPos = _pos;
            AppendFormatted(value, format);
            if (alignment != 0)
            {
                AppendOrInsertAlignmentIfNeeded(startingPos, alignment);
            }
        }

        public void AppendFormatted(int value)
        {
            // If there's a custom formatter, always use it.
            if (_hasCustomFormatter)
            {
                AppendCustomFormatter(value, format: null);
                return;
            }

            int charsWritten;
            while (!value.TryFormat(_chars.Slice(_pos), out charsWritten, format: null, _provider)) // constrained call avoiding boxing for value types
            {
                Grow();
            }

            _pos += charsWritten;
        }

        public void AppendFormatted(int value, int alignment)
        {
            int startingPos = _pos;
            AppendFormatted(value);
            if (alignment != 0)
            {
                AppendOrInsertAlignmentIfNeeded(startingPos, alignment);
            }
        }

        public void AppendFormatted(int value, string? format)
        {
            // If there's a custom formatter, always use it.
            if (_hasCustomFormatter)
            {
                AppendCustomFormatter(value, format);
                return;
            }

            int charsWritten;
            while (!value.TryFormat(_chars.Slice(_pos), out charsWritten, format: format, _provider)) // constrained call avoiding boxing for value types
            {
                Grow();
            }

            _pos += charsWritten;
        }

        public void AppendFormatted(int value, int alignment, string? format)
        {
            int startingPos = _pos;
            AppendFormatted(value, format);
            if (alignment != 0)
            {
                AppendOrInsertAlignmentIfNeeded(startingPos, alignment);
            }
        }

        public void AppendFormatted(uint value)
        {
            // If there's a custom formatter, always use it.
            if (_hasCustomFormatter)
            {
                AppendCustomFormatter(value, format: null);
                return;
            }

            int charsWritten;
            while (!value.TryFormat(_chars.Slice(_pos), out charsWritten, format: null, _provider)) // constrained call avoiding boxing for value types
            {
                Grow();
            }

            _pos += charsWritten;
        }

        public void AppendFormatted(uint value, int alignment)
        {
            int startingPos = _pos;
            AppendFormatted(value);
            if (alignment != 0)
            {
                AppendOrInsertAlignmentIfNeeded(startingPos, alignment);
            }
        }

        public void AppendFormatted(uint value, string? format)
        {
            // If there's a custom formatter, always use it.
            if (_hasCustomFormatter)
            {
                AppendCustomFormatter(value, format);
                return;
            }

            int charsWritten;
            while (!value.TryFormat(_chars.Slice(_pos), out charsWritten, format: format, _provider)) // constrained call avoiding boxing for value types
            {
                Grow();
            }

            _pos += charsWritten;
        }

        public void AppendFormatted(uint value, int alignment, string? format)
        {
            int startingPos = _pos;
            AppendFormatted(value, format);
            if (alignment != 0)
            {
                AppendOrInsertAlignmentIfNeeded(startingPos, alignment);
            }
        }

        public void AppendFormatted(long value)
        {
            // If there's a custom formatter, always use it.
            if (_hasCustomFormatter)
            {
                AppendCustomFormatter(value, format: null);
                return;
            }

            int charsWritten;
            while (!value.TryFormat(_chars.Slice(_pos), out charsWritten, format: null, _provider)) // constrained call avoiding boxing for value types
            {
                Grow();
            }

            _pos += charsWritten;
        }

        public void AppendFormatted(long value, int alignment)
        {
            int startingPos = _pos;
            AppendFormatted(value);
            if (alignment != 0)
            {
                AppendOrInsertAlignmentIfNeeded(startingPos, alignment);
            }
        }

        public void AppendFormatted(long value, string? format)
        {
            // If there's a custom formatter, always use it.
            if (_hasCustomFormatter)
            {
                AppendCustomFormatter(value, format);
                return;
            }

            int charsWritten;
            while (!value.TryFormat(_chars.Slice(_pos), out charsWritten, format: format, _provider)) // constrained call avoiding boxing for value types
            {
                Grow();
            }

            _pos += charsWritten;
        }

        public void AppendFormatted(long value, int alignment, string? format)
        {
            int startingPos = _pos;
            AppendFormatted(value, format);
            if (alignment != 0)
            {
                AppendOrInsertAlignmentIfNeeded(startingPos, alignment);
            }
        }

        public void AppendFormatted(ulong value)
        {
            // If there's a custom formatter, always use it.
            if (_hasCustomFormatter)
            {
                AppendCustomFormatter(value, format: null);
                return;
            }

            int charsWritten;
            while (!value.TryFormat(_chars.Slice(_pos), out charsWritten, format: null, _provider)) // constrained call avoiding boxing for value types
            {
                Grow();
            }

            _pos += charsWritten;
        }

        public void AppendFormatted(ulong value, int alignment)
        {
            int startingPos = _pos;
            AppendFormatted(value);
            if (alignment != 0)
            {
                AppendOrInsertAlignmentIfNeeded(startingPos, alignment);
            }
        }

        public void AppendFormatted(ulong value, string? format)
        {
            // If there's a custom formatter, always use it.
            if (_hasCustomFormatter)
            {
                AppendCustomFormatter(value, format);
                return;
            }

            int charsWritten;
            while (!value.TryFormat(_chars.Slice(_pos), out charsWritten, format: format, _provider)) // constrained call avoiding boxing for value types
            {
                Grow();
            }

            _pos += charsWritten;
        }

        public void AppendFormatted(ulong value, int alignment, string? format)
        {
            int startingPos = _pos;
            AppendFormatted(value, format);
            if (alignment != 0)
            {
                AppendOrInsertAlignmentIfNeeded(startingPos, alignment);
            }
        }

        public void AppendFormatted(float value)
        {
            // If there's a custom formatter, always use it.
            if (_hasCustomFormatter)
            {
                AppendCustomFormatter(value, format: null);
                return;
            }

            int charsWritten;
            while (!value.TryFormat(_chars.Slice(_pos), out charsWritten, format: null, _provider)) // constrained call avoiding boxing for value types
            {
                Grow();
            }

            _pos += charsWritten;
        }

        public void AppendFormatted(float value, int alignment)
        {
            int startingPos = _pos;
            AppendFormatted(value);
            if (alignment != 0)
            {
                AppendOrInsertAlignmentIfNeeded(startingPos, alignment);
            }
        }

        public void AppendFormatted(float value, string? format)
        {
            // If there's a custom formatter, always use it.
            if (_hasCustomFormatter)
            {
                AppendCustomFormatter(value, format);
                return;
            }

            int charsWritten;
            while (!value.TryFormat(_chars.Slice(_pos), out charsWritten, format: format, _provider)) // constrained call avoiding boxing for value types
            {
                Grow();
            }

            _pos += charsWritten;
        }

        public void AppendFormatted(float value, int alignment, string? format)
        {
            int startingPos = _pos;
            AppendFormatted(value, format);
            if (alignment != 0)
            {
                AppendOrInsertAlignmentIfNeeded(startingPos, alignment);
            }
        }

        public void AppendFormatted(double value)
        {
            // If there's a custom formatter, always use it.
            if (_hasCustomFormatter)
            {
                AppendCustomFormatter(value, format: null);
                return;
            }

            int charsWritten;
            while (!value.TryFormat(_chars.Slice(_pos), out charsWritten, format: null, _provider)) // constrained call avoiding boxing for value types
            {
                Grow();
            }

            _pos += charsWritten;
        }

        public void AppendFormatted(double value, int alignment)
        {
            int startingPos = _pos;
            AppendFormatted(value);
            if (alignment != 0)
            {
                AppendOrInsertAlignmentIfNeeded(startingPos, alignment);
            }
        }

        public void AppendFormatted(double value, string? format)
        {
            // If there's a custom formatter, always use it.
            if (_hasCustomFormatter)
            {
                AppendCustomFormatter(value, format);
                return;
            }

            int charsWritten;
            while (!value.TryFormat(_chars.Slice(_pos), out charsWritten, format: format, _provider)) // constrained call avoiding boxing for value types
            {
                Grow();
            }

            _pos += charsWritten;
        }

        public void AppendFormatted(double value, int alignment, string? format)
        {
            int startingPos = _pos;
            AppendFormatted(value, format);
            if (alignment != 0)
            {
                AppendOrInsertAlignmentIfNeeded(startingPos, alignment);
            }
        }

        public void AppendFormatted(decimal value)
        {
            // If there's a custom formatter, always use it.
            if (_hasCustomFormatter)
            {
                AppendCustomFormatter(value, format: null);
                return;
            }

            int charsWritten;
            while (!value.TryFormat(_chars.Slice(_pos), out charsWritten, format: null, _provider)) // constrained call avoiding boxing for value types
            {
                Grow();
            }

            _pos += charsWritten;
        }

        public void AppendFormatted(decimal value, int alignment)
        {
            int startingPos = _pos;
            AppendFormatted(value);
            if (alignment != 0)
            {
                AppendOrInsertAlignmentIfNeeded(startingPos, alignment);
            }
        }

        public void AppendFormatted(decimal value, string? format)
        {
            // If there's a custom formatter, always use it.
            if (_hasCustomFormatter)
            {
                AppendCustomFormatter(value, format);
                return;
            }

            int charsWritten;
            while (!value.TryFormat(_chars.Slice(_pos), out charsWritten, format: format, _provider)) // constrained call avoiding boxing for value types
            {
                Grow();
            }

            _pos += charsWritten;
        }

        public void AppendFormatted(decimal value, int alignment, string? format)
        {
            int startingPos = _pos;
            AppendFormatted(value, format);
            if (alignment != 0)
            {
                AppendOrInsertAlignmentIfNeeded(startingPos, alignment);
            }
        }

        public void AppendFormatted(DateTime value)
        {
            // If there's a custom formatter, always use it.
            if (_hasCustomFormatter)
            {
                AppendCustomFormatter(value, format: null);
                return;
            }

            int charsWritten;
            while (!value.TryFormat(_chars.Slice(_pos), out charsWritten, format: null, _provider)) // constrained call avoiding boxing for value types
            {
                Grow();
            }

            _pos += charsWritten;
        }

        public void AppendFormatted(DateTime value, int alignment)
        {
            int startingPos = _pos;
            AppendFormatted(value);
            if (alignment != 0)
            {
                AppendOrInsertAlignmentIfNeeded(startingPos, alignment);
            }
        }

        public void AppendFormatted(DateTime value, string? format)
        {
            // If there's a custom formatter, always use it.
            if (_hasCustomFormatter)
            {
                AppendCustomFormatter(value, format);
                return;
            }

            int charsWritten;
            while (!value.TryFormat(_chars.Slice(_pos), out charsWritten, format: format, _provider)) // constrained call avoiding boxing for value types
            {
                Grow();
            }

            _pos += charsWritten;
        }

        public void AppendFormatted(DateTime value, int alignment, string? format)
        {
            int startingPos = _pos;
            AppendFormatted(value, format);
            if (alignment != 0)
            {
                AppendOrInsertAlignmentIfNeeded(startingPos, alignment);
            }
        }

        public void AppendFormatted(DateTimeOffset value)
        {
            // If there's a custom formatter, always use it.
            if (_hasCustomFormatter)
            {
                AppendCustomFormatter(value, format: null);
                return;
            }

            int charsWritten;
            while (!value.TryFormat(_chars.Slice(_pos), out charsWritten, format: null, _provider)) // constrained call avoiding boxing for value types
            {
                Grow();
            }

            _pos += charsWritten;
        }

        public void AppendFormatted(DateTimeOffset value, int alignment)
        {
            int startingPos = _pos;
            AppendFormatted(value);
            if (alignment != 0)
            {
                AppendOrInsertAlignmentIfNeeded(startingPos, alignment);
            }
        }

        public void AppendFormatted(DateTimeOffset value, string? format)
        {
            // If there's a custom formatter, always use it.
            if (_hasCustomFormatter)
            {
                AppendCustomFormatter(value, format);
                return;
            }

            int charsWritten;
            while (!value.TryFormat(_chars.Slice(_pos), out charsWritten, format: format, _provider)) // constrained call avoiding boxing for value types
            {
                Grow();
            }

            _pos += charsWritten;
        }

        public void AppendFormatted(DateTimeOffset value, int alignment, string? format)
        {
            int startingPos = _pos;
            AppendFormatted(value, format);
            if (alignment != 0)
            {
                AppendOrInsertAlignmentIfNeeded(startingPos, alignment);
            }
        }

        public void AppendFormatted(TimeSpan value)
        {
            // If there's a custom formatter, always use it.
            if (_hasCustomFormatter)
            {
                AppendCustomFormatter(value, format: null);
                return;
            }

            int charsWritten;
            while (!value.TryFormat(_chars.Slice(_pos), out charsWritten, format: null, _provider)) // constrained call avoiding boxing for value types
            {
                Grow();
            }

            _pos += charsWritten;
        }

        public void AppendFormatted(TimeSpan value, int alignment)
        {
            int startingPos = _pos;
            AppendFormatted(value);
            if (alignment != 0)
            {
                AppendOrInsertAlignmentIfNeeded(startingPos, alignment);
            }
        }

        public void AppendFormatted(TimeSpan value, string? format)
        {
            // If there's a custom formatter, always use it.
            if (_hasCustomFormatter)
            {
                AppendCustomFormatter(value, format);
                return;
            }

            int charsWritten;
            while (!value.TryFormat(_chars.Slice(_pos), out charsWritten, format: format, _provider)) // constrained call avoiding boxing for value types
            {
                Grow();
            }

            _pos += charsWritten;
        }

        public void AppendFormatted(TimeSpan value, int alignment, string? format)
        {
            int startingPos = _pos;
            AppendFormatted(value, format);
            if (alignment != 0)
            {
                AppendOrInsertAlignmentIfNeeded(startingPos, alignment);
            }
        }

        public void AppendFormatted(Guid value)
        {
            // If there's a custom formatter, always use it.
            if (_hasCustomFormatter)
            {
                AppendCustomFormatter(value, format: null);
                return;
            }

            int charsWritten;
            while (!value.TryFormat(_chars.Slice(_pos), out charsWritten, format: null)) // constrained call avoiding boxing for value types
            {
                Grow();
            }

            _pos += charsWritten;
        }

        public void AppendFormatted(Guid value, int alignment)
        {
            int startingPos = _pos;
            AppendFormatted(value);
            if (alignment != 0)
            {
                AppendOrInsertAlignmentIfNeeded(startingPos, alignment);
            }
        }

        public void AppendFormatted(Guid value, string? format)
        {
            // If there's a custom formatter, always use it.
            if (_hasCustomFormatter)
            {
                AppendCustomFormatter(value, format);
                return;
            }

            int charsWritten;
            while (!value.TryFormat(_chars.Slice(_pos), out charsWritten, format: format)) // constrained call avoiding boxing for value types
            {
                Grow();
            }

            _pos += charsWritten;
        }

        public void AppendFormatted(Guid value, int alignment, string? format)
        {
            int startingPos = _pos;
            AppendFormatted(value, format);
            if (alignment != 0)
            {
                AppendOrInsertAlignmentIfNeeded(startingPos, alignment);
            }
        }

        public void AppendFormatted(bool value)
        {
            // If there's a custom formatter, always use it.
            if (_hasCustomFormatter)
            {
                AppendCustomFormatter(value, format: null);
                return;
            }

            int charsWritten;
            while (!value.TryFormat(_chars.Slice(_pos), out charsWritten)) // constrained call avoiding boxing for value types
            {
                Grow();
            }

            _pos += charsWritten;
        }

        public void AppendFormatted(bool value, int alignment)
        {
            int startingPos = _pos;
            AppendFormatted(value);
            if (alignment != 0)
            {
                AppendOrInsertAlignmentIfNeeded(startingPos, alignment);
            }
        }

        public void AppendFormatted(bool value, string? format)
        {
            // If there's a custom formatter, always use it.
            if (_hasCustomFormatter)
            {
                AppendCustomFormatter(value, format);
                return;
            }

            int charsWritten;
            while (!value.TryFormat(_chars.Slice(_pos), out charsWritten)) // constrained call avoiding boxing for value types
            {
                Grow();
            }

            _pos += charsWritten;
        }

        public void AppendFormatted(bool value, int alignment, string? format)
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

