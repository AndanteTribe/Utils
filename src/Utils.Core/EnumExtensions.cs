using System;
using System.Runtime.CompilerServices;

namespace AndanteTribe.Utils
{
    /// <summary>
    /// 列挙体の拡張メソッド.
    /// </summary>
    public static class EnumExtensions
    {
        /// <summary>
        /// 指定のビットフラグを持っているかどうかを判定します.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="flag"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <remarks>
        /// 指定する列挙体はint型が基になる型として指定されている必要があります.
        /// </remarks>
        public static bool HasBitFlags<T>(this T value, T flag) where T : struct, Enum
        {
            var v = Unsafe.As<T, int>(ref value);
            var f = Unsafe.As<T, int>(ref flag);
            return (v & f) == f;
        }

        /// <summary>
        /// 1つのビットフラグを持っているかどうかを判定します.
        /// </summary>
        /// <param name="value"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <remarks>
        /// 指定する列挙体はint型が基になる型として指定されている必要があります.
        /// </remarks>
        public static bool ConstructFlags<T>(this T value) where T : struct, Enum
        {
            var v = Unsafe.As<T, int>(ref value);
            return v != 0 && (v & (v - 1)) == 0;
        }

        /// <summary>
        /// 列挙体の全てのビットフラグを取得します.
        /// </summary>
        /// <param name="value"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <remarks>
        /// 指定する列挙体はint型が基になる型として指定されている必要があります.
        /// </remarks>
        public static Enumerator<T> GetEnumerator<T>(this T value) where T : struct, Enum => new(value);

        /// <summary>
        /// 列挙体の全てのビットフラグを取得します.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public struct Enumerator<T> where T : struct, Enum
        {
            private int _value;

            /// <summary>
            /// 現在の列挙体の値を取得します.
            /// </summary>
            public T Current { get; private set; }

            internal Enumerator(T value)
            {
                _value = Unsafe.As<T, int>(ref value);
                Current = default;
            }

            /// <summary>
            /// 列挙体の次のビットフラグを取得します.
            /// </summary>
            /// <returns></returns>
            public bool MoveNext()
            {
                if (_value == 0)
                {
                    return false;
                }

                var f = _value & -_value; // get lowest flag
                Current = Unsafe.As<int, T>(ref f);
                _value &= ~f;
                return true;
            }
        }
    }
}