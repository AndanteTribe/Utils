#if ENABLE_ADDRESSABLES && ENABLE_UNITASK
#nullable enable

using System;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace AndanteTribe.Utils.Unity.Addressable
{
    public sealed class AddressableObjectReference<T> : IObjectReference<T> where T : UnityEngine.Object
    {
        private readonly string _address;
        private readonly CancellationTokenSource _cancellationDisposable;

        private AsyncLazy<T>? _cached;
        private AsyncOperationHandle<T> _handle;

        public AddressableObjectReference(string address, CancellationTokenSource? cancellationDisposable = null)
        {
            _address = address ?? throw new ArgumentNullException(nameof(address), "Address cannot be null.");
            _cancellationDisposable = cancellationDisposable ?? new CancellationTokenSource();
        }

        public async ValueTask<T> LoadAsync(CancellationToken cancellationToken)
        {
            _cancellationDisposable.ThrowIfDisposed(this);
            cancellationToken.ThrowIfCancellationRequested();
            if (_address == null)
            {
                throw new NullReferenceException("AssetReference is null.");
            }
            using var cts = _cancellationDisposable.CreateLinkedTokenSource(cancellationToken);

            if (_cached == null)
            {
                _handle = Addressables.LoadAssetAsync<T>(_address);
                _cached ??= _handle.ToUniTask(cancellationToken: cts.Token, autoReleaseWhenCanceled: true).ToAsyncLazy();
            }
            return await _cached;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _cancellationDisposable.ThrowIfDisposed(this);
            if (_cached != null)
            {
                _handle.Release();
            }
            _cancellationDisposable.Cancel();
            _cancellationDisposable.Dispose();
        }
    }

    [Serializable]
    public sealed class SerializableAddressableObjectReference<T> : IObjectReference<T> where T : UnityEngine.Object
    {
        private readonly CancellationTokenSource _cancellationDisposable = new();

        [SerializeField]
        private AssetReferenceT<T>? _value;
        private AsyncLazy<T>? _cached;

        public async ValueTask<T> LoadAsync(CancellationToken cancellationToken)
        {
            _cancellationDisposable.ThrowIfDisposed(this);
            cancellationToken.ThrowIfCancellationRequested();
            if (_value == null)
            {
                throw new NullReferenceException("AssetReference is null.");
            }
            using var cts = _cancellationDisposable.CreateLinkedTokenSource(cancellationToken);

            _cached ??= _value.LoadAssetAsync<T>()
                .ToUniTask(cancellationToken: cts.Token, autoReleaseWhenCanceled: true)
                .ToAsyncLazy();
            return await _cached;
        }

        public void Dispose()
        {
            _cancellationDisposable.ThrowIfDisposed(this);
            _value?.ReleaseAsset();
            _cancellationDisposable.Cancel();
            _cancellationDisposable.Dispose();
        }
    }
}

#endif