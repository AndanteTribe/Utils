using System.Runtime.CompilerServices;

namespace AndanteTribe.Utils;

/// <summary>
/// <see cref="ValueTuple"/>の拡張メソッド.
/// </summary>
public static class ValueTupleExtensions
{
    /// <summary>
    /// <see cref="ValueTuple{T1, T2}"/>のforeach対応.
    /// </summary>
    /// <example>
    /// <code>
    /// <![CDATA[
    /// using UnityEngine;
    ///
    /// public class Sample : MonoBehaviour
    /// {
    ///     [SerializeField] private GameObject _button;
    ///     [SerializeField] private GameObject _button1;
    ///
    ///     private void Start()
    ///     {
    ///         //タプルでforeachが回せる
    ///         foreach (var obj in (_button,_button1))
    ///         {
    ///             obj.SetActive(false);
    ///         }
    ///     }
    /// }
    /// ]]>
    /// </code>
    /// </example>
    /// <param name="tuple">対象の<see cref="ValueTuple{T1, T2}"/>.</param>
    /// <typeparam name="T">要素の型.</typeparam>
    /// <returns>要素を列挙する<see cref="Enumerator{T}"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Enumerator<T> GetEnumerator<T>(in this (T, T) tuple)
        => new((tuple.Item1, tuple.Item2, default, default, default, default, default)!, 2);

    /// <summary>
    /// <see cref="ValueTuple{T1, T2, T3}"/>のforeach対応.
    /// </summary>
    /// <example>
    /// <code>
    /// <![CDATA[
    /// using UnityEngine;
    ///
    /// public class Sample : MonoBehaviour
    /// {
    ///     [SerializeField] private GameObject _button;
    ///     [SerializeField] private GameObject _button1;
    ///     [SerializeField] private GameObject _button2;
    ///
    ///     private void Start()
    ///     {
    ///         //タプルでforeachが回せる
    ///         foreach (var obj in (_button,_button1,_button2))
    ///         {
    ///             obj.SetActive(false);
    ///         }
    ///     }
    /// }
    /// ]]>
    /// </code>
    /// </example>
    /// <param name="tuple">対象の<see cref="ValueTuple{T1, T2, T3}"/>.</param>
    /// <typeparam name="T">要素の型.</typeparam>
    /// <returns>要素を列挙する<see cref="Enumerator{T}"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Enumerator<T> GetEnumerator<T>(in this (T, T, T) tuple)
        => new((tuple.Item1, tuple.Item2, tuple.Item3, default, default, default, default)!, 3);

    /// <summary>
    /// <see cref="ValueTuple{T1, T2, T3, T4}"/>のforeach対応.
    /// </summary>
    /// <example>
    /// <code>
    /// <![CDATA[
    /// using UnityEngine;
    ///
    /// public class Sample : MonoBehaviour
    /// {
    ///     [SerializeField] private GameObject _button;
    ///     [SerializeField] private GameObject _button1;
    ///     [SerializeField] private GameObject _button2;
    ///     [SerializeField] private GameObject _button3;
    ///
    ///     private void Start()
    ///     {
    ///         //タプルでforeachが回せる
    ///         foreach (var obj in (_button,_button1,_button2,_button3))
    ///         {
    ///             obj.SetActive(false);
    ///         }
    ///     }
    /// }
    /// ]]>
    /// </code>
    /// </example>
    /// <param name="tuple">対象の<see cref="ValueTuple{T1, T2, T3, T4}"/>.</param>
    /// <typeparam name="T">要素の型.</typeparam>
    /// <returns>要素を列挙する<see cref="Enumerator{T}"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Enumerator<T> GetEnumerator<T>(in this (T, T, T, T) tuple)
        => new((tuple.Item1, tuple.Item2, tuple.Item3, tuple.Item4, default, default, default)!, 4);

    /// <summary>
    /// <see cref="ValueTuple{T1, T2, T3, T4, T5}"/>のforeach対応.
    /// </summary>
    /// <example>
    /// <code>
    /// <![CDATA[
    /// using UnityEngine;
    ///
    /// public class Sample : MonoBehaviour
    /// {
    ///     [SerializeField] private GameObject _button;
    ///     [SerializeField] private GameObject _button1;
    ///     [SerializeField] private GameObject _button2;
    ///     [SerializeField] private GameObject _button3;
    ///     [SerializeField] private GameObject _button4;
    ///
    ///     private void Start()
    ///     {
    ///         //タプルでforeachが回せる
    ///         foreach (var obj in (_button,_button1,_button2,_button3,_button4))
    ///         {
    ///             obj.SetActive(false);
    ///         }
    ///     }
    /// }
    /// ]]>
    /// </code>
    /// </example>
    /// <param name="tuple">対象の<see cref="ValueTuple{T1, T2, T3, T4, T5}"/>.</param>
    /// <typeparam name="T">要素の型.</typeparam>
    /// <returns>要素を列挙する<see cref="Enumerator{T}"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Enumerator<T> GetEnumerator<T>(in this (T, T, T, T, T) tuple)
        => new((tuple.Item1, tuple.Item2, tuple.Item3, tuple.Item4, tuple.Item5, default, default)!, 5);

    /// <summary>
    /// <see cref="ValueTuple{T1, T2, T3, T4, T5, T6}"/>のforeach対応.
    /// </summary>
    /// <example>
    /// <code>
    /// <![CDATA[
    /// using UnityEngine;
    ///
    /// public class Sample : MonoBehaviour
    /// {
    ///     [SerializeField] private GameObject _button;
    ///     [SerializeField] private GameObject _button1;
    ///     [SerializeField] private GameObject _button2;
    ///     [SerializeField] private GameObject _button3;
    ///     [SerializeField] private GameObject _button4;
    ///     [SerializeField] private GameObject _button5;
    ///
    ///     private void Start()
    ///     {
    ///         //タプルでforeachが回せる
    ///         foreach (var obj in (_button,_button1,_button2,_button3,_button4,_button5))
    ///         {
    ///             obj.SetActive(false);
    ///         }
    ///     }
    /// }
    /// ]]>
    /// </code>
    /// </example>
    /// <param name="tuple">対象の<see cref="ValueTuple{T1, T2, T3, T4, T5, T6}"/>.</param>
    /// <typeparam name="T">要素の型.</typeparam>
    /// <returns>要素を列挙する<see cref="Enumerator{T}"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Enumerator<T> GetEnumerator<T>(in this (T, T, T, T, T, T) tuple)
        => new((tuple.Item1, tuple.Item2, tuple.Item3, tuple.Item4, tuple.Item5, tuple.Item6, default)!, 6);

    /// <summary>
    /// <see cref="ValueTuple{T1, T2, T3, T4, T5, T6, T7}"/>のforeach対応.
    /// </summary>
    /// <example>
    /// <code>
    /// <![CDATA[
    /// using UnityEngine;
    ///
    /// public class Sample : MonoBehaviour
    /// {
    ///     [SerializeField] private GameObject _button;
    ///     [SerializeField] private GameObject _button1;
    ///     [SerializeField] private GameObject _button2;
    ///     [SerializeField] private GameObject _button3;
    ///     [SerializeField] private GameObject _button4;
    ///     [SerializeField] private GameObject _button5;
    ///     [SerializeField] private GameObject _button6;
    ///
    ///     private void Start()
    ///     {
    ///         //タプルでforeachが回せる
    ///         foreach (var obj in (_button,_button1,_button2,_button3,_button4,_button5,_button6))
    ///         {
    ///             obj.SetActive(false);
    ///         }
    ///     }
    /// }
    /// ]]>
    /// </code>
    /// </example>
    /// <param name="tuple">対象の<see cref="ValueTuple{T1, T2, T3, T4, T5, T6, T7}"/>.</param>
    /// <typeparam name="T">要素の型.</typeparam>
    /// <returns>要素を列挙する<see cref="Enumerator{T}"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Enumerator<T> GetEnumerator<T>(in this (T, T, T, T, T, T, T) tuple) => new(tuple, 7);

    /// <summary>
    /// ValueTupleのforeach対応.
    /// </summary>
    /// <typeparam name="T">要素の型.</typeparam>
    public struct Enumerator<T>
    {
        private readonly Buffer7<T> _buffer;
        private readonly byte _length;
        private sbyte _index;
        private bool _moveNext;

        /// <summary>
        /// <see cref="System.Collections.Generic.IEnumerator{T}.Current"/>に同じ.
        /// </summary>
        public T Current => _index switch
        {
            0 => _buffer[0],
            1 => _buffer[1],
            2 when _moveNext => _buffer[2],
            3 when _moveNext => _buffer[3],
            4 when _moveNext => _buffer[4],
            5 when _moveNext => _buffer[5],
            6 when _moveNext => _buffer[6],
            _ => throw new IndexOutOfRangeException(),
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="Enumerator{T}"/> struct.
        /// </summary>
        /// <param name="tuple"></param>
        /// <param name="length"></param>
        internal Enumerator(in (T, T, T, T, T, T, T) tuple, byte length)
        {
            _buffer = new Buffer7<T>(tuple);
            _length = length;
            _index = -1;
        }

        /// <summary>
        /// <see cref="System.Collections.Generic.IEnumerator{T}.MoveNext"/>に同じ.
        /// </summary>
        /// <returns>列挙が可能な場合はtrue.</returns>
        public bool MoveNext() => _moveNext = _index <= _length && ++_index < _length;
    }

#if NET8_0_OR_GREATER
    [System.Runtime.CompilerServices.InlineArray(7)]
    internal struct Buffer7<T>
    {
        private T _element;

        internal Buffer7((T, T, T, T, T, T, T) tuple)
        {
            this[0] = tuple.Item1;
            this[1] = tuple.Item2;
            this[2] = tuple.Item3;
            this[3] = tuple.Item4;
            this[4] = tuple.Item5;
            this[5] = tuple.Item6;
            this[6] = tuple.Item7;
        }
    }
#else
    internal struct Buffer7<T>
    {
        private T _e0, _e1, _e2, _e3, _e4, _e5, _e6;

        internal Buffer7((T, T, T, T, T, T, T) tuple)
        {
            _e0 = tuple.Item1;
            _e1 = tuple.Item2;
            _e2 = tuple.Item3;
            _e3 = tuple.Item4;
            _e4 = tuple.Item5;
            _e5 = tuple.Item6;
            _e6 = tuple.Item7;
        }

        internal T this[int index] => index switch
        {
            0 => _e0,
            1 => _e1,
            2 => _e2,
            3 => _e3,
            4 => _e4,
            5 => _e5,
            6 => _e6,
            _ => throw new IndexOutOfRangeException(),
        };
    }
#endif

}