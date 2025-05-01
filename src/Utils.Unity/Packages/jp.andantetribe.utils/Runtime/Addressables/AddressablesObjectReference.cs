#if ENABLE_ADDRESSABLES && ENABLE_UNITASK
#nullable enable

using System;
using System.Threading;
using System.Threading.Tasks;
using AndanteTribe.Utils.Internal;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace AndanteTribe.Utils.Addressables
{
    [Serializable]
    public sealed class AddressablesObjectReference<T> : ITrackableDisposable, IObjectReference<T> where T : UnityEngine.Object
    {
        [SerializeField]
        private AssetReferenceT<T>? _value;

        private AsyncLazy<T>? _cached;

        public bool IsDisposed { get; private set; }

        public async ValueTask<T> LoadAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (_value == null)
            {
                throw new NullReferenceException("AssetReference is null.");
            }
            ThrowHelper.ThrowIfDisposedException(IsDisposed, this);

            _cached ??= _value.LoadAssetAsync<T>()
                .ToUniTask(cancellationToken: cancellationToken, autoReleaseWhenCanceled: true).ToAsyncLazy();
            return await _cached;
        }

        public void Dispose()
        {
            if (!IsDisposed)
            {
                _value?.ReleaseAsset();
                IsDisposed = true;
            }
        }
    }
}

#endif