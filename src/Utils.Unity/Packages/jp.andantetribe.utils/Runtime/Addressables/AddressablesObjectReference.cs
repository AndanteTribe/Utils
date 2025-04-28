#if ENABLE_ADDRESSABLES && ENABLE_UNITASK
#nullable enable

using System;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace AndanteTribe.Utils.Addressables
{
    [Serializable]
    public sealed class AddressablesObjectReference<T> : IObjectReference<T> where T : UnityEngine.Object
    {
        [SerializeField]
        private AssetReferenceT<T>? _value;

        private T? _cached;

        public async ValueTask<T> LoadAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (_value == null)
            {
                throw new NullReferenceException("AssetReference is null.");
            }

            _cached ??= await _value.LoadAssetAsync<T>()
                .ToUniTask(cancellationToken: cancellationToken, autoReleaseWhenCanceled: true);
            return _cached;
        }

        public void Dispose()
        {
            if (_value != null)
            {
                _value.ReleaseAsset();
                _value = null;
            }
        }
    }
}

#endif