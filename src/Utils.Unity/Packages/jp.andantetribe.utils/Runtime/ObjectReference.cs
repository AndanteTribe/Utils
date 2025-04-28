#nullable enable

using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace AndanteTribe.Utils
{
    public interface IObjectReference<T> : IDisposable where T : UnityEngine.Object
    {
        ValueTask<T> LoadAsync(CancellationToken cancellationToken);
    }

    [Serializable]
    public sealed class SerializableObjectReference<T> : IObjectReference<T> where T : UnityEngine.Object
    {
        [SerializeField]
        private T? _value;

        public ValueTask<T> LoadAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (_value == null)
            {
                throw new NullReferenceException("Object reference is null.");
            }
            return new ValueTask<T>(_value);
        }

        public void Dispose()
        {
        }
    }
}