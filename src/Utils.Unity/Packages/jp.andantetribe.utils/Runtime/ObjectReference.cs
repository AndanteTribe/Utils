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
        private T? _object;

        public ValueTask<T> LoadAsync(CancellationToken cancellationToken) =>
            _object != null ? new ValueTask<T>(_object) : throw new NullReferenceException("Object reference is null.");

        public void Dispose()
        {
        }
    }
}