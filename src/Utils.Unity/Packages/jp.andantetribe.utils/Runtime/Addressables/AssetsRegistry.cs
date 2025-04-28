#if ENABLE_ADDRESSABLES && ENABLE_UNITASK
#nullable enable

using System;
using System.Collections.Generic;
using System.Threading;
using AndanteTribe.Utils.Internal;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Pool;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

namespace AndanteTribe.Utils.Addressables
{
    using Addressables = UnityEngine.AddressableAssets.Addressables;

    /// <summary>
    /// ロードしたアセットのハンドルをキャッシュしているレジストリ.
    /// </summary>
    public class AssetsRegistry : IDisposable
    {
        private readonly List<AsyncOperationHandle> _handles = ListPool<AsyncOperationHandle>.Get();

        public bool IsDisposed { get; private set; }

        public int Count => _handles.Count;

        /// <summary>
        /// アセットのロード.
        /// </summary>
        /// <param name="address"></param>
        /// <param name="cancellationToken"></param>
        /// <typeparam name="TObject"></typeparam>
        /// <returns></returns>
        public async UniTask<TObject> LoadAsync<TObject>(
            string address, CancellationToken cancellationToken) where TObject : Object
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowHelper.ThrowIfObjectDisposedException(IsDisposed, this);
            var handle = Addressables.LoadAssetAsync<TObject>(address);
            var result = await handle.ToUniTask(cancellationToken: cancellationToken, autoReleaseWhenCanceled: true);
            _handles.Add(handle);
            return result;
        }

        /// <summary>
        /// アセットのロード.
        /// </summary>
        /// <param name="reference"></param>
        /// <param name="cancellationToken"></param>
        /// <typeparam name="TObject"></typeparam>
        /// <returns></returns>
        public async UniTask<TObject> LoadAsync<TObject>(
            AssetReferenceT<TObject> reference, CancellationToken cancellationToken) where TObject : Object
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowHelper.ThrowIfObjectDisposedException(IsDisposed, this);
            var handle = Addressables.LoadAssetAsync<TObject>(reference);
            var result = await handle.ToUniTask(cancellationToken: cancellationToken, autoReleaseWhenCanceled: true);
            _handles.Add(handle);
            return result;
        }

        /// <summary>
        /// アセットをロードして生成、指定のコンポーネントで返却.
        /// </summary>
        /// <remarks>
        /// Prefab生成用.
        /// </remarks>
        /// <param name="address"></param>
        /// <param name="parent"></param>
        /// <param name="cancellationToken"></param>
        /// <typeparam name="TComponent"></typeparam>
        /// <returns></returns>
        public async UniTask<TComponent> InstantiateAsync<TComponent>(
            string address, Transform parent, CancellationToken cancellationToken) where TComponent : Component
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowHelper.ThrowIfObjectDisposedException(IsDisposed, this);
            var handle = Addressables.LoadAssetAsync<GameObject>(address);
            var obj = await handle.ToUniTask(cancellationToken: cancellationToken, autoReleaseWhenCanceled: true);
            if (!obj.TryGetComponent<TComponent>(out var component))
            {
                handle.Release();
                throw new InvalidOperationException($"指定の型 {typeof(TComponent)} は {handle.DebugName} から取得できませんでした。");
            }
            try
            {
                var result = await Object.InstantiateAsync(component, parent).WithCancellation(cancellationToken);
                return result[0];
            }
            catch (OperationCanceledException e) when(e.CancellationToken == cancellationToken)
            {
                handle.Release();
                throw;
            }
        }

        /// <summary>
        /// アセットをロードして生成、指定のコンポーネントで返却.
        /// </summary>
        /// <param name="reference"></param>
        /// <param name="parent"></param>
        /// <param name="cancellationToken"></param>
        /// <typeparam name="TComponent"></typeparam>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public async UniTask<TComponent> InstantiateAsync<TComponent>(
            AssetReferenceT<GameObject> reference, Transform parent, CancellationToken cancellationToken) where TComponent : Component
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowHelper.ThrowIfObjectDisposedException(IsDisposed, this);
            var handle = Addressables.LoadAssetAsync<GameObject>(reference);
            var obj = await handle.ToUniTask(cancellationToken: cancellationToken, autoReleaseWhenCanceled: true);
            if (!obj.TryGetComponent<TComponent>(out var component))
            {
                handle.Release();
                throw new InvalidOperationException($"指定の型 {typeof(TComponent)} は {handle.DebugName} から取得できませんでした。");
            }
            try
            {
                var result = await Object.InstantiateAsync(component, parent).WithCancellation(cancellationToken);
                return result[0];
            }
            catch (OperationCanceledException e) when(e.CancellationToken == cancellationToken)
            {
                handle.Release();
                throw;
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            foreach (var handle in _handles.AsSpan())
            {
                if (handle.IsValid())
                {
                    handle.Release();
                }
            }
            ListPool<AsyncOperationHandle>.Release(_handles);
            IsDisposed = true;
        }
    }
}

#endif