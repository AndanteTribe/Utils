#if ENABLE_ADDRESSABLES && ENABLE_UNITASK
#nullable enable

using System;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Assertions;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace AndanteTribe.Utils.Unity.Addressable
{
    /// <summary>
    /// Addressableのアドレスを利用したオブジェクト参照.
    /// </summary>
    /// <example>
    /// <code>
    /// <![CDATA[
    /// using System.Threading;
    /// using AndanteTribe.Utils.Unity.Addressable;
    /// using Cysharp.Threading.Tasks;
    /// using UnityEngine;
    ///
    /// public class SimpleAddressableSample : MonoBehaviour
    /// {
    ///     // IObjectReference として保持
    ///     private readonly IObjectReference<GameObject> _reference
    ///         = new AddressableObjectReference<GameObject>("assets/prefabs/MyPrefab.prefab");
    ///
    ///     private async UniTask Start()
    ///     {
    ///         // IObjectReference 経由で読み込んでインスタンス化
    ///         var prefab = await _reference.LoadAsync(destoryCancellationToken);
    ///         var instance = Instantiate(prefab, Vector3.zero, Quaternion.identity);
    ///     }
    ///
    ///     private void OnDestroy()
    ///     {
    ///         _reference.Dispose();
    ///     }
    /// }
    /// ]]>
    /// </code>
    /// </example>
    /// <typeparam name="T"></typeparam>
    public sealed class AddressableObjectReference<T> : IObjectReference<T> where T : UnityEngine.Object
    {
        private readonly string _address;

        private AsyncLazy<T>? _cached;
        private AsyncOperationHandle<T> _handle;

        public AddressableObjectReference(string address)
        {
            _address = address ?? throw new ArgumentNullException(nameof(address), "Address cannot be null.");
        }

        public async ValueTask<T> LoadAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (_cached == null)
            {
                _handle = Addressables.LoadAssetAsync<T>(_address);
                _cached ??= _handle.ToUniTask(cancellationToken: cancellationToken, autoReleaseWhenCanceled: true).ToAsyncLazy();
            }
            return await _cached;
        }

        /// <inheritdoc />
        public async ValueTask<T> LoadAsync(IProgress<float> progress, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (_cached == null)
            {
                _handle = Addressables.LoadAssetAsync<T>(_address);
                _cached ??= _handle.ToUniTask(progress: progress, cancellationToken: cancellationToken, autoReleaseWhenCanceled: true).ToAsyncLazy();
            }
            return await _cached;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (_cached != null)
            {
                _handle.Release();
                _cached = null;
            }
        }
    }

    [Serializable]
    internal sealed class SerializableAddressableObjectReference<T> : IObjectReference<T> where T : UnityEngine.Object
    {
        [SerializeField]
        private AssetReferenceT<T> _value = null!;
        private AsyncLazy<T>? _cached;

        /// <inheritdoc />
        public async ValueTask<T> LoadAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Assert.IsTrue(_value.IsValid());

            _cached ??= _value.LoadAssetAsync<T>()
                .ToUniTask(cancellationToken: cancellationToken, autoReleaseWhenCanceled: true)
                .ToAsyncLazy();
            return await _cached;
        }

        /// <inheritdoc />
        public async ValueTask<T> LoadAsync(IProgress<float> progress, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Assert.IsTrue(_value.IsValid());

            _cached ??= _value.LoadAssetAsync<T>()
                .ToUniTask(progress: progress, cancellationToken: cancellationToken, autoReleaseWhenCanceled: true)
                .ToAsyncLazy();
            return await _cached;
        }

        public void Dispose()
        {
            if (_cached != null)
            {
                _value.ReleaseAsset();
                _cached = null;
            }
        }
    }
}

#endif