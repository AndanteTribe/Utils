#if !NET6_0_OR_GREATER
#nullable enable

using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using AndanteTribe.Utils.BackPort.Internal;
using UnityEngine;

namespace AndanteTribe.Utils.Unity
{
    internal sealed class UnityCustomSpanFormatter : IServiceProvider
    {
        private readonly RuntimeTypeHandle _boundsTypeHandle = typeof(Bounds).TypeHandle;
        private readonly RuntimeTypeHandle _boundsIntTypeHandle = typeof(BoundsInt).TypeHandle;
        private readonly RuntimeTypeHandle _colorTypeHandle = typeof(Color).TypeHandle;
        private readonly RuntimeTypeHandle _color32TypeHandle = typeof(Color32).TypeHandle;
        private readonly RuntimeTypeHandle _matrixTypeHandle = typeof(Matrix4x4).TypeHandle;
        private readonly RuntimeTypeHandle _quaternionTypeHandle = typeof(Quaternion).TypeHandle;
        private readonly RuntimeTypeHandle _rayTypeHandle = typeof(Ray).TypeHandle;
        private readonly RuntimeTypeHandle _ray2DTypeHandle = typeof(Ray2D).TypeHandle;
        private readonly RuntimeTypeHandle _rectTypeHandle = typeof(Rect).TypeHandle;
        private readonly RuntimeTypeHandle _rectIntTypeHandle = typeof(RectInt).TypeHandle;
        private readonly RuntimeTypeHandle _rectOffsetTypeHandle = typeof(RectOffset).TypeHandle;
        private readonly RuntimeTypeHandle _vector2TypeHandle = typeof(Vector2).TypeHandle;
        private readonly RuntimeTypeHandle _vector2IntTypeHandle = typeof(Vector2Int).TypeHandle;
        private readonly RuntimeTypeHandle _vector3TypeHandle = typeof(Vector3).TypeHandle;
        private readonly RuntimeTypeHandle _vector3IntTypeHandle = typeof(Vector3Int).TypeHandle;
        private readonly RuntimeTypeHandle _vector4TypeHandle = typeof(Vector4).TypeHandle;

        /// <inheritdoc />
        object? IServiceProvider.GetService(Type serviceType)
        {
            var t = serviceType.TypeHandle;

            if (t.Equals(_boundsTypeHandle))
            {
                return new TryFormat<Bounds>(TryFormatBounds);
            }
            if (t.Equals(_boundsIntTypeHandle))
            {
                return new TryFormat<BoundsInt>(TryFormatBoundsInt);
            }
            if (t.Equals(_colorTypeHandle))
            {
                return new TryFormat<Color>(TryFormatColor);
            }
            if (t.Equals(_color32TypeHandle))
            {
                return new TryFormat<Color32>(TryFormatColor32);
            }
            if (t.Equals(_matrixTypeHandle))
            {
                return new TryFormat<Matrix4x4>(TryFormatMatrix4X4);
            }
            if (t.Equals(_quaternionTypeHandle))
            {
                return new TryFormat<Quaternion>(TryFormatQuaternion);
            }
            if (t.Equals(_rayTypeHandle))
            {
                return new TryFormat<Ray>(TryFormatRay);
            }
            if (t.Equals(_ray2DTypeHandle))
            {
                return new TryFormat<Ray2D>(TryFormatRay2D);
            }
            if (t.Equals(_rectTypeHandle))
            {
                return new TryFormat<Rect>(TryFormatRect);
            }
            if (t.Equals(_rectIntTypeHandle))
            {
                return new TryFormat<RectInt>(TryFormatRectInt);
            }
            if (t.Equals(_rectOffsetTypeHandle))
            {
                return new TryFormat<RectOffset>(TryFormatRectOffset);
            }
            if (t.Equals(_vector2TypeHandle))
            {
                return new TryFormat<Vector2>(TryFormatVector2);
            }
            if (t.Equals(_vector2IntTypeHandle))
            {
                return new TryFormat<Vector2Int>(TryFormatVector2Int);
            }
            if (t.Equals(_vector3TypeHandle))
            {
                return new TryFormat<Vector3>(TryFormatVector3);
            }
            if (t.Equals(_vector3IntTypeHandle))
            {
                return new TryFormat<Vector3Int>(TryFormatVector3Int);
            }
            if (t.Equals(_vector4TypeHandle))
            {
                return new TryFormat<Vector4>(TryFormatVector4);
            }

            return null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool TryFormatBounds(Bounds value, in Span<char> destination, out int written, in ReadOnlySpan<char> format, IFormatProvider? provider)
        {
            written = 0;

            if (!stackalloc char[8] { 'C', 'e', 'n', 't', 'e', 'r', ':', ' ' }.TryCopyTo(destination[written..]))
            {
                return false;
            }
            written += 8;

            if (!TryFormatVector3(value.center, destination[written..], out var centerWritten, format, provider))
            {
                return false;
            }
            written += centerWritten;

            if (!stackalloc char[11] { ',', ' ', 'E', 'x', 't', 'e', 'n', 't', 's', ':', ' ' }.TryCopyTo(destination[written..]))
            {
                return false;
            }
            written += 11;

            if (!TryFormatVector3(value.extents, destination[written..], out var extentsWritten, format, provider))
            {
                return false;
            }
            written += extentsWritten;

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool TryFormatBoundsInt(BoundsInt value, in Span<char> destination, out int written, in ReadOnlySpan<char> format, IFormatProvider? provider)
        {
            written = 0;

            if (!stackalloc char[10] { 'P', 'o', 's', 'i', 't', 'i', 'o', 'n', ':', ' ' }.TryCopyTo(destination[written..]))
            {
                return false;
            }
            written += 10;

            if (!TryFormatVector3Int(value.position, destination[written..], out var positionWritten, format, provider))
            {
                return false;
            }
            written += positionWritten;

            if (!stackalloc char[8] { ',', ' ', 'S', 'i', 'z', 'e', ':', ' ' }.TryCopyTo(destination[written..]))
            {
                return false;
            }
            written += 8;

            if (!TryFormatVector3Int(value.size, destination[written..], out var sizeWritten, format, provider))
            {
                return false;
            }
            written += sizeWritten;

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool TryFormatColor(Color value, in Span<char> destination, out int written, in ReadOnlySpan<char> format, IFormatProvider? provider)
        {
            var f = format.IsEmpty ? "F3" : format;
            provider ??= CultureInfo.InvariantCulture.NumberFormat;
            written = 0;

                if (!stackalloc char[5] { 'R', 'G', 'B', 'A', '(' }.TryCopyTo(destination[written..]))
            {
                return false;
            }
            written += 5;

            if (!value.r.TryFormat(destination[written..], out var rWritten, f, provider))
            {
                return false;
            }
            written += rWritten;

            if (!stackalloc char[2] { ',', ' ' }.TryCopyTo(destination[written..]))
            {
                return false;
            }
            written += 2;

            if (!value.g.TryFormat(destination[written..], out var gWritten, f, provider))
            {
                return false;
            }
            written += gWritten;

            if (!stackalloc char[2] { ',', ' ' }.TryCopyTo(destination[written..]))
            {
                return false;
            }
            written += 2;

            if (!value.b.TryFormat(destination[written..], out var bWritten, f, provider))
            {
                return false;
            }
            written += bWritten;

            if (!stackalloc char[2] { ',', ' ' }.TryCopyTo(destination[written..]))
            {
                return false;
            }
            written += 2;

            if (!value.a.TryFormat(destination[written..], out var aWritten, f, provider))
            {
                return false;
            }
            written += aWritten;

            if (destination.Length < written + 1)
            {
                return false;
            }
            destination[written] = ')';
            written++;

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool TryFormatColor32(Color32 value, in Span<char> destination, out int written, in ReadOnlySpan<char> format, IFormatProvider? provider)
        {
            provider ??= CultureInfo.InvariantCulture.NumberFormat;
            written = 0;

            if (!stackalloc char[5] { 'R', 'G', 'B', 'A', '(' }.TryCopyTo(destination[written..]))
            {
                return false;
            }
            written += 5;

            if (!value.r.TryFormat(destination[written..], out var rWritten, format, provider))
            {
                return false;
            }
            written += rWritten;

            if (!stackalloc char[2] { ',', ' ' }.TryCopyTo(destination[written..]))
            {
                return false;
            }
            written += 2;

            if (!value.g.TryFormat(destination[written..], out var gWritten, format, provider))
            {
                return false;
            }
            written += gWritten;

            if (!stackalloc char[2] { ',', ' ' }.TryCopyTo(destination[written..]))
            {
                return false;
            }
            written += 2;

            if (!value.b.TryFormat(destination[written..], out var bWritten, format, provider))
            {
                return false;
            }
            written += bWritten;

            if (!stackalloc char[2] { ',', ' ' }.TryCopyTo(destination[written..]))
            {
                return false;
            }
            written += 2;

            if (!value.a.TryFormat(destination[written..], out var aWritten, format, provider))
            {
                return false;
            }
            written += aWritten;

            if (destination.Length < written + 1)
            {
                return false;
            }
            destination[written] = ')';
            written++;

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool TryFormatMatrix4X4(Matrix4x4 value, in Span<char> destination, out int written, in ReadOnlySpan<char> format, IFormatProvider? provider)
        {
            var f = format.IsEmpty ? "F5" : format;
            provider ??= CultureInfo.InvariantCulture.NumberFormat;
            written = 0;

            if (!value.m00.TryFormat(destination[written..], out var m00Written, f, provider))
            {
                return false;
            }
            written += m00Written;

            if (destination.Length < written + 1)
            {
                return false;
            }
            destination[written] = '\t';
            written++;

            if (!value.m01.TryFormat(destination[written..], out var m01Written, f, provider))
            {
                return false;
            }
            written += m01Written;

            if (destination.Length < written + 1)
            {
                return false;
            }
            destination[written] = '\t';
            written++;

            if (!value.m02.TryFormat(destination[written..], out var m02Written, f, provider))
            {
                return false;
            }
            written += m02Written;

            if (destination.Length < written + 1)
            {
                return false;
            }
            destination[written] = '\t';
            written++;

            if (!value.m03.TryFormat(destination[written..], out var m03Written, f, provider))
            {
                return false;
            }
            written += m03Written;

            if (destination.Length < written + 1)
            {
                return false;
            }
            destination[written] = '\n';
            written++;

            if (!value.m10.TryFormat(destination[written..], out var m10Written, f, provider))
            {
                return false;
            }
            written += m10Written;

            if (destination.Length < written + 1)
            {
                return false;
            }
            destination[written] = '\t';
            written++;

            if (!value.m11.TryFormat(destination[written..], out var m11Written, f, provider))
            {
                return false;
            }
            written += m11Written;

            if (destination.Length < written + 1)
            {
                return false;
            }
            destination[written] = '\t';
            written++;

            if (!value.m12.TryFormat(destination[written..], out var m12Written, f, provider))
            {
                return false;
            }
            written += m12Written;

            if (destination.Length < written + 1)
            {
                return false;
            }
            destination[written] = '\t';
            written++;

            if (!value.m13.TryFormat(destination[written..], out var m13Written, f, provider))
            {
                return false;
            }
            written += m13Written;

            if (destination.Length < written + 1)
            {
                return false;
            }
            destination[written] = '\n';
            written++;

            if (!value.m20.TryFormat(destination[written..], out var m20Written, f, provider))
            {
                return false;
            }
            written += m20Written;

            if (destination.Length < written + 1)
            {
                return false;
            }
            destination[written] = '\t';
            written++;

            if (!value.m21.TryFormat(destination[written..], out var m21Written, f, provider))
            {
                return false;
            }
            written += m21Written;

            if (destination.Length < written + 1)
            {
                return false;
            }
            destination[written] = '\t';
            written++;

            if (!value.m22.TryFormat(destination[written..], out var m22Written, f, provider))
            {
                return false;
            }
            written += m22Written;

            if (destination.Length < written + 1)
            {
                return false;
            }
            destination[written] = '\t';
            written++;

            if (!value.m23.TryFormat(destination[written..], out var m23Written, f, provider))
            {
                return false;
            }
            written += m23Written;

            if (destination.Length < written + 1)
            {
                return false;
            }
            destination[written] = '\n';
            written++;

            if (!value.m30.TryFormat(destination[written..], out var m30Written, f, provider))
            {
                return false;
            }
            written += m30Written;

            if (destination.Length < written + 1)
            {
                return false;
            }
            destination[written] = '\t';
            written++;

            if (!value.m31.TryFormat(destination[written..], out var m31Written, f, provider))
            {
                return false;
            }
            written += m31Written;

            if (destination.Length < written + 1)
            {
                return false;
            }
            destination[written] = '\t';
            written++;

            if (!value.m32.TryFormat(destination[written..], out var m32Written, f, provider))
            {
                return false;
            }
            written += m32Written;

            if (destination.Length < written + 1)
            {
                return false;
            }
            destination[written] = '\t';
            written++;

            if (!value.m33.TryFormat(destination[written..], out var m33Written, f, provider))
            {
                return false;
            }
            written += m33Written;

            if (destination.Length < written + 1)
            {
                return false;
            }
            destination[written] = '\n';
            written++;

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool TryFormatQuaternion(Quaternion value, in Span<char> destination, out int written, in ReadOnlySpan<char> format, IFormatProvider? provider)
        {
            var f = format.IsEmpty ? "F5" : format;
            provider ??= CultureInfo.InvariantCulture.NumberFormat;
            written = 0;

            if (destination.Length < 1)
            {
                return false;
            }
            destination[0] = '(';
            written++;

            if (!value.x.TryFormat(destination[written..], out var xWritten, f, provider))
            {
                return false;
            }
            written += xWritten;

            if (!stackalloc char[2] { ',', ' ' }.TryCopyTo(destination[written..]))
            {
                return false;
            }
            written += 2;

            if (!value.y.TryFormat(destination[written..], out var yWritten, f, provider))
            {
                return false;
            }
            written += yWritten;

            if (!stackalloc char[2] { ',', ' ' }.TryCopyTo(destination[written..]))
            {
                return false;
            }
            written += 2;

            if (!value.z.TryFormat(destination[written..], out var zWritten, f, provider))
            {
                return false;
            }
            written += zWritten;

            if (!stackalloc char[2] { ',', ' ' }.TryCopyTo(destination[written..]))
            {
                return false;
            }
            written += 2;

            if (!value.w.TryFormat(destination[written..], out var wWritten, f, provider))
            {
                return false;
            }
            written += wWritten;

            if (destination.Length < written + 1)
            {
                return false;
            }
            destination[written] = ')';
            written++;

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool TryFormatRay(Ray value, in Span<char> destination, out int written, in ReadOnlySpan<char> format, IFormatProvider? provider)
        {
            written = 0;

            if (!stackalloc char[8] { 'O', 'r', 'i', 'g', 'i', 'n', ':', ' ' }.TryCopyTo(destination[written..]))
            {
                return false;
            }
            written += 8;

            if (!TryFormatVector3(value.origin, destination[written..], out var originWritten, format, provider))
            {
                return false;
            }
            written += originWritten;

            if (!stackalloc char[7] { ',', ' ', 'D', 'i', 'r', ':', ' ' }.TryCopyTo(destination[written..]))
            {
                return false;
            }
            written += 7;

            if (!TryFormatVector3(value.direction, destination[written..], out var directionWritten, format, provider))
            {
                return false;
            }
            written += directionWritten;

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool TryFormatRay2D(Ray2D value, in Span<char> destination, out int written, in ReadOnlySpan<char> format, IFormatProvider? provider)
        {
            written = 0;

            if (!stackalloc char[8] { 'O', 'r', 'i', 'g', 'i', 'n', ':', ' ' }.TryCopyTo(destination[written..]))
            {
                return false;
            }
            written += 8;

            if (!TryFormatVector2(value.origin, destination[written..], out var originWritten, format, provider))
            {
                return false;
            }
            written += originWritten;

            if (!stackalloc char[7] { ',', ' ', 'D', 'i', 'r', ':', ' ' }.TryCopyTo(destination[written..]))
            {
                return false;
            }
            written += 7;

            if (!TryFormatVector2(value.direction, destination[written..], out var directionWritten, format, provider))
            {
                return false;
            }
            written += directionWritten;

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool TryFormatRect(Rect value, in Span<char> destination, out int written, in ReadOnlySpan<char> format, IFormatProvider? provider)
        {
            var f = format.IsEmpty ? "F2" : format;
            provider ??= CultureInfo.InvariantCulture.NumberFormat;
            written = 0;

            if (!stackalloc char[3] { '(', 'x', ':' }.TryCopyTo(destination[written..]))
            {
                return false;
            }
            written += 3;

            if (!value.x.TryFormat(destination[written..], out var xWritten, f, provider))
            {
                return false;
            }
            written += xWritten;

            if (!stackalloc char[5] { ',', ' ', 'y', ':', ' ' }.TryCopyTo(destination[written..]))
            {
                return false;
            }
            written += 5;

            if (!value.y.TryFormat(destination[written..], out var yWritten, f, provider))
            {
                return false;
            }
            written += yWritten;

            if (!stackalloc char[9] { ',', ' ', 'w', 'i', 'd', 't', 'h', ':', ' ' }.TryCopyTo(destination[written..]))
            {
                return false;
            }
            written += 9;

            if (!value.width.TryFormat(destination[written..], out var wWritten, f, provider))
            {
                return false;
            }
            written += wWritten;

            if (!stackalloc char[10] { ',', ' ', 'h', 'e', 'i', 'g', 'h', 't', ':', ' ' }.TryCopyTo(destination[written..]))
            {
                return false;
            }
            written += 10;

            if (!value.height.TryFormat(destination[written..], out var hWritten, f, provider))
            {
                return false;
            }
            written += hWritten;

            if (destination.Length < written + 1)
            {
                return false;
            }
            destination[written] = ')';
            written++;

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool TryFormatRectInt(RectInt value, in Span<char> destination, out int written, in ReadOnlySpan<char> format, IFormatProvider? provider)
        {
            provider ??= CultureInfo.InvariantCulture.NumberFormat;
            written = 0;

            if (!stackalloc char[3] { '(', 'x', ':' }.TryCopyTo(destination[written..]))
            {
                return false;
            }
            written += 3;

            if (!value.x.TryFormat(destination[written..], out var xWritten, format, provider))
            {
                return false;
            }
            written += xWritten;

            if (!stackalloc char[5] { ',', ' ', 'y', ':', ' ' }.TryCopyTo(destination[written..]))
            {
                return false;
            }
            written += 5;

            if (!value.y.TryFormat(destination[written..], out var yWritten, format, provider))
            {
                return false;
            }
            written += yWritten;

            if (!stackalloc char[9] { ',', ' ', 'w', 'i', 'd', 't', 'h', ':', ' ' }.TryCopyTo(destination[written..]))
            {
                return false;
            }
            written += 9;

            if (!value.width.TryFormat(destination[written..], out var wWritten, format, provider))
            {
                return false;
            }
            written += wWritten;

            if (!stackalloc char[10] { ',', ' ', 'h', 'e', 'i', 'g', 'h', 't', ':', ' ' }.TryCopyTo(destination[written..]))
            {
                return false;
            }
            written += 10;

            if (!value.height.TryFormat(destination[written..], out var hWritten, format, provider))
            {
                return false;
            }
            written += hWritten;

            if (destination.Length < written + 1)
            {
                return false;
            }
            destination[written] = ')';
            written++;

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool TryFormatRectOffset(RectOffset value, in Span<char> destination, out int written, in ReadOnlySpan<char> format, IFormatProvider? provider)
        {
            provider ??= CultureInfo.InvariantCulture.NumberFormat;
            written = 0;

            if (!stackalloc char[13] { 'R', 'e', 'c', 't', 'O', 'f', 'f', 's', 'e', 't', ' ', '(', 'l' }.TryCopyTo(destination[written..]))
            {
                return false;
            }
            written += 13;

            if (destination.Length < written + 1)
            {
                return false;
            }
            destination[written] = ':';
            written++;

            if (!value.left.TryFormat(destination[written..], out var leftWritten, format, provider))
            {
                return false;
            }
            written += leftWritten;

            if (!stackalloc char[3] { ' ', 'r', ':' }.TryCopyTo(destination[written..]))
            {
                return false;
            }
            written += 3;

            if (!value.right.TryFormat(destination[written..], out var rightWritten, format, provider))
            {
                return false;
            }
            written += rightWritten;

            if (!stackalloc char[3] { ' ', 't', ':' }.TryCopyTo(destination[written..]))
            {
                return false;
            }
            written += 3;

            if (!value.top.TryFormat(destination[written..], out var topWritten, format, provider))
            {
                return false;
            }
            written += topWritten;

            if (!stackalloc char[3] { ' ', 'b', ':' }.TryCopyTo(destination[written..]))
            {
                return false;
            }
            written += 3;

            if (!value.bottom.TryFormat(destination[written..], out var bottomWritten, format, provider))
            {
                return false;
            }
            written += bottomWritten;

            if (destination.Length < written + 1)
            {
                return false;
            }
            destination[written] = ')';
            written++;

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool TryFormatVector2(Vector2 value, in Span<char> destination, out int written, in ReadOnlySpan<char> format, IFormatProvider? provider)
        {
            var f = format.IsEmpty ? "F2" : format;
            provider ??= CultureInfo.InvariantCulture.NumberFormat;
            written = 0;

            if (destination.Length < 1)
            {
                return false;
            }
            destination[0] = '(';
            written++;

            if (!value.x.TryFormat(destination[written..], out var xWritten, f, provider))
            {
                return false;
            }
            written += xWritten;

            if (!stackalloc char[2]{ ',', ' ' }.TryCopyTo(destination[written..]))
            {
                return false;
            }
            written += 2;

            if (!value.y.TryFormat(destination[written..], out var yWritten, f, provider))
            {
                return false;
            }
            written += yWritten;

            if (destination.Length < written + 1)
            {
                return false;
            }
            destination[written] = ')';
            written++;

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool TryFormatVector2Int(Vector2Int value, in Span<char> destination, out int written, in ReadOnlySpan<char> format, IFormatProvider? provider)
        {
            var f = format.IsEmpty ? "F2" : format;
            provider ??= CultureInfo.InvariantCulture.NumberFormat;
            written = 0;

            if (destination.Length < 1)
            {
                return false;
            }
            destination[0] = '(';
            written++;

            if (!value.x.TryFormat(destination[written..], out var xWritten, f, provider))
            {
                return false;
            }
            written += xWritten;

            if (!stackalloc char[2]{ ',', ' ' }.TryCopyTo(destination[written..]))
            {
                return false;
            }
            written += 2;

            if (!value.y.TryFormat(destination[written..], out var yWritten, f, provider))
            {
                return false;
            }
            written += yWritten;

            if (destination.Length < written + 1)
            {
                return false;
            }
            destination[written] = ')';
            written++;

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool TryFormatVector3(Vector3 value, in Span<char> destination, out int written, in ReadOnlySpan<char> format, IFormatProvider? provider)
        {
            var f = format.IsEmpty ? "F2" : format;
            provider ??= CultureInfo.InvariantCulture.NumberFormat;
            written = 0;

            if (destination.Length < 1)
            {
                return false;
            }
            destination[0] = '(';
            written++;

            if (!value.x.TryFormat(destination[written..], out var xWritten, f, provider))
            {
                return false;
            }
            written += xWritten;

            if (!stackalloc char[2]{ ',', ' ' }.TryCopyTo(destination[written..]))
            {
                return false;
            }
            written += 2;

            if (!value.y.TryFormat(destination[written..], out var yWritten, f, provider))
            {
                return false;
            }
            written += yWritten;

            if (!stackalloc char[2]{ ',', ' ' }.TryCopyTo(destination[written..]))
            {
                return false;
            }
            written += 2;

            if (!value.z.TryFormat(destination[written..], out var zWritten, f, provider))
            {
                return false;
            }
            written += zWritten;

            if (destination.Length < written + 1)
            {
                return false;
            }
            destination[written] = ')';
            written++;

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool TryFormatVector3Int(Vector3Int value, in Span<char> destination, out int written, in ReadOnlySpan<char> format, IFormatProvider? provider)
        {
            var f = format.IsEmpty ? "F2" : format;
            provider ??= CultureInfo.InvariantCulture.NumberFormat;
            written = 0;

            if (destination.Length < 1)
            {
                return false;
            }
            destination[0] = '(';
            written++;

            if (!value.x.TryFormat(destination[written..], out var xWritten, f, provider))
            {
                return false;
            }
            written += xWritten;

            if (!stackalloc char[2]{ ',', ' ' }.TryCopyTo(destination[written..]))
            {
                return false;
            }
            written += 2;

            if (!value.y.TryFormat(destination[written..], out var yWritten, f, provider))
            {
                return false;
            }
            written += yWritten;

            if (!stackalloc char[2]{ ',', ' ' }.TryCopyTo(destination[written..]))
            {
                return false;
            }
            written += 2;

            if (!value.z.TryFormat(destination[written..], out var zWritten, f, provider))
            {
                return false;
            }
            written += zWritten;

            if (destination.Length < written + 1)
            {
                return false;
            }
            destination[written] = ')';
            written++;

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool TryFormatVector4(Vector4 value, in Span<char> destination, out int written, in ReadOnlySpan<char> format, IFormatProvider? provider)
        {
            var f = format.IsEmpty ? "F2" : format;
            provider ??= CultureInfo.InvariantCulture.NumberFormat;
            written = 0;

            if (destination.Length < 1)
            {
                return false;
            }
            destination[0] = '(';
            written++;

            if (!value.x.TryFormat(destination[written..], out var xWritten, f, provider))
            {
                return false;
            }
            written += xWritten;

            if (!stackalloc char[2]{ ',', ' ' }.TryCopyTo(destination[written..]))
            {
                return false;
            }
            written += 2;

            if (!value.y.TryFormat(destination[written..], out var yWritten, f, provider))
            {
                return false;
            }
            written += yWritten;

            if (!stackalloc char[2]{ ',', ' ' }.TryCopyTo(destination[written..]))
            {
                return false;
            }
            written += 2;

            if (!value.z.TryFormat(destination[written..], out var zWritten, f, provider))
            {
                return false;
            }
            written += zWritten;

            if (!stackalloc char[2]{ ',', ' ' }.TryCopyTo(destination[written..]))
            {
                return false;
            }
            written += 2;

            if (!value.w.TryFormat(destination[written..], out var wWritten, f, provider))
            {
                return false;
            }
            written += wWritten;

            if (destination.Length < written + 1)
            {
                return false;
            }
            destination[written] = ')';
            written++;

            return true;
        }
    }
}

#endif