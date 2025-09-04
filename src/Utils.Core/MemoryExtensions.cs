using System.Collections;
using System.Runtime.CompilerServices;

namespace AndanteTribe.Utils;

/// <summary>
/// <see cref="Memory{T}"/>, <see cref="ReadOnlyMemory{T}"/>の拡張メソッド.
/// </summary>
public static class MemoryExtensions
{
    /// <summary>
    /// <see cref="Memory{T}"/>のforeach対応.
    /// </summary>
    /// <remarks>
    /// <see cref="GetEnumerator{T}(in ReadOnlyMemory{T})"/>と使い方や機能は同じ.
    /// </remarks>
    /// <example>
    /// <code>
    /// <![CDATA[
    /// using System;
    /// using System.Collections;
    /// using AndanteTribe.Utils;
    /// using UnityEngine;
    ///
    /// public class MemoryExtensionsSample : MonoBehaviour
    /// {
    ///     private Memory<char> _text;
    ///
    ///     private void Start()
    ///     {
    ///         const string text = "Hello, World!";
    ///         _text = text.AsMemory().Slice(7, 5); // 'W', 'o', 'r', 'l', 'd'
    ///
    ///         StartCoroutine(UpdateText());
    ///     }
    ///
    ///     private IEnumerator UpdateText()
    ///     {
    ///         foreach (var c in _text)
    ///         {
    ///             Debug.Log(c);
    ///             yield return null; // Wait for next frame
    ///         }
    ///     }
    /// }
    /// ]]>
    /// </code>
    /// </example>
    /// <param name="memory">対象の<see cref="Memory{T}"/>.</param>
    /// <typeparam name="T">要素の型.</typeparam>
    /// <returns>要素を列挙する<see cref="MemoryExtensions.Enumerator{T}"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Enumerator<T> GetEnumerator<T>(in this Memory<T> memory) => new(memory);

    /// <summary>
    /// <see cref="ReadOnlyMemory{T}"/>のforeach対応.
    /// </summary>
    /// <example>
    /// <code>
    /// <![CDATA[
    /// using System;
    /// using System.Collections;
    /// using AndanteTribe.Utils;
    /// using UnityEngine;
    ///
    /// public class MemoryExtensionsSample : MonoBehaviour
    /// {
    ///     private ReadOnlyMemory<char> _text;
    ///
    ///     private void Start()
    ///     {
    ///         const string text = "Hello, World!";
    ///         _text = text.AsMemory().Slice(7, 5); // 'W', 'o', 'r', 'l', 'd'
    ///
    ///         StartCoroutine(UpdateText());
    ///     }
    ///
    ///     private IEnumerator UpdateText()
    ///     {
    ///         foreach (var c in _text)
    ///         {
    ///             Debug.Log(c);
    ///             yield return null; // Wait for next frame
    ///         }
    ///     }
    /// }
    /// ]]>
    /// </code>
    /// </example>
    /// <param name="memory">対象の<see cref="ReadOnlyMemory{T}"/>.</param>
    /// <typeparam name="T">要素の型.</typeparam>
    /// <returns>要素を列挙する<see cref="MemoryExtensions.Enumerator{T}"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Enumerator<T> GetEnumerator<T>(in this ReadOnlyMemory<T> memory) => new(memory);

    /// <summary>
    /// <see cref="ReadOnlyMemory{T}"/>のforeach対応.
    /// </summary>
    /// <typeparam name="T">要素の型.</typeparam>
    public struct Enumerator<T> : IEnumerator<T>
    {
        private readonly ReadOnlyMemory<T> _memory;
        private int _index;

        /// <inheritdoc/>
        public readonly T Current => _memory.Span[_index];

        /// <inheritdoc/>
        readonly object? IEnumerator.Current => Current;

        /// <summary>
        /// Initializes a new instance of the <see cref="Enumerator{T}"/> struct.
        /// </summary>
        /// <param name="memory"><see cref="ReadOnlyMemory{T}"/>.</param>
        internal Enumerator(in ReadOnlyMemory<T> memory)
        {
            this._memory = memory;
            _index = -1;
        }

        /// <inheritdoc/>
        public bool MoveNext() => _index < _memory.Length && ++_index < _memory.Length;

        /// <inheritdoc/>
        void IEnumerator.Reset() => _index = -1;

        /// <inheritdoc/>
        readonly void IDisposable.Dispose()
        {
        }
    }
}