#nullable enable

using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace AndanteTribe.Utils.Unity
{
    /// <summary>
    /// オブジェクト参照のインターフェース.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IObjectReference<T> : IDisposable where T : UnityEngine.Object
    {
        /// <summary>
        /// オブジェクトを非同期に読み込みます.
        /// </summary>
        /// <param name="cancellationToken">キャンセル用トークン.</param>
        ValueTask<T> LoadAsync(CancellationToken cancellationToken);

        /// <summary>
        /// オブジェクトを非同期に読み込みます(進捗報告付き).
        /// </summary>
        /// <param name="progress">進捗報告用コールバック.</param>
        /// <param name="cancellationToken">キャンセル用トークン.</param>
        ValueTask<T> LoadAsync(IProgress<float> progress, CancellationToken cancellationToken);
    }

    [Serializable]
    internal sealed class SerializableObjectReference<T> : IObjectReference<T> where T : UnityEngine.Object
    {
        [SerializeField]
        private T? _value = null!;

        /// <inheritdoc />
        public ValueTask<T> LoadAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (_value == null)
            {
                throw new NullReferenceException("Object reference is null.");
            }
            return new ValueTask<T>(_value);
        }

        /// <inheritdoc />
        public ValueTask<T> LoadAsync(IProgress<float> progress, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (_value == null)
            {
                throw new NullReferenceException("Object reference is null.");
            }
            progress.Report(1.0f);
            return new ValueTask<T>(_value);
        }

        /// <inheritdoc />
        public void Dispose()
        {
        }
    }
}