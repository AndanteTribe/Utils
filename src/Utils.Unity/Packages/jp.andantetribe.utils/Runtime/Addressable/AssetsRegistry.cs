#if ENABLE_ADDRESSABLES && ENABLE_UNITASK
#nullable enable

using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Pool;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

namespace AndanteTribe.Utils.Unity.Addressable
{
    /// <summary>
    /// ロードしたアセットのハンドルをキャッシュしているレジストリ.
    /// </summary>
    public class AssetsRegistry : IDisposable
    {
        private readonly List<AsyncOperationHandle> _handles = ListPool<AsyncOperationHandle>.Get();
        private readonly CancellationTokenSource _cancellationDisposable;

        public int Count => _handles.Count;

        public AssetsRegistry(CancellationTokenSource? cancellationDisposable = null) =>
            _cancellationDisposable = cancellationDisposable ?? new CancellationTokenSource();

        /// <summary>
        /// アセットのロード.
        /// </summary>
        /// <param name="address"></param>
        /// <param name="cancellationToken"></param>
        /// <typeparam name="TObject"></typeparam>
        /// <returns></returns>
        public async UniTask<TObject> LoadAsync<TObject>(string address, CancellationToken cancellationToken) where TObject : Object
        {
            _cancellationDisposable.ThrowIfDisposed(this);
            cancellationToken.ThrowIfCancellationRequested();
            using var cts = _cancellationDisposable.CreateLinkedTokenSource(_cancellationDisposable.Token);
            return await LoadAsyncInternal<TObject>(address, cts.Token);
        }

        internal async UniTask<TObject> LoadAsyncInternal<TObject>(string address, CancellationToken cancellationToken) where TObject : Object
        {
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
        public async UniTask<TObject> LoadAsync<TObject>(AssetReferenceT<TObject> reference, CancellationToken cancellationToken) where TObject : Object
        {
            _cancellationDisposable.ThrowIfDisposed(this);
            cancellationToken.ThrowIfCancellationRequested();
            using var cts = _cancellationDisposable.CreateLinkedTokenSource(_cancellationDisposable.Token);
            var handle = Addressables.LoadAssetAsync<TObject>(reference);
            var result = await handle.ToUniTask(cancellationToken: cts.Token, autoReleaseWhenCanceled: true);
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
            _cancellationDisposable.ThrowIfDisposed(this);
            cancellationToken.ThrowIfCancellationRequested();
            using var cts = _cancellationDisposable.CreateLinkedTokenSource(_cancellationDisposable.Token);
            var handle = Addressables.LoadAssetAsync<GameObject>(address);
            var obj = await handle.ToUniTask(cancellationToken: cts.Token, autoReleaseWhenCanceled: true);
            if (!obj.TryGetComponent<TComponent>(out var component))
            {
                handle.Release();
                throw new InvalidOperationException($"指定の型 {typeof(TComponent)} は {handle.DebugName} から取得できませんでした。");
            }
            try
            {
                var result = await Object.InstantiateAsync(component, parent).WithCancellation(cts.Token);
                _handles.Add(handle);
                return result[0];
            }
            catch (OperationCanceledException e) when(e.CancellationToken == cts.Token)
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
            _cancellationDisposable.ThrowIfDisposed(this);
            cancellationToken.ThrowIfCancellationRequested();
            using var cts = _cancellationDisposable.CreateLinkedTokenSource(_cancellationDisposable.Token);
            var handle = Addressables.LoadAssetAsync<GameObject>(reference);
            var obj = await handle.ToUniTask(cancellationToken: cts.Token, autoReleaseWhenCanceled: true);
            if (!obj.TryGetComponent<TComponent>(out var component))
            {
                handle.Release();
                throw new InvalidOperationException($"指定の型 {typeof(TComponent)} は {handle.DebugName} から取得できませんでした。");
            }
            try
            {
                var result = await Object.InstantiateAsync(component, parent).WithCancellation(cts.Token);
                _handles.Add(handle);
                return result[0];
            }
            catch (OperationCanceledException e) when(e.CancellationToken == cts.Token)
            {
                handle.Release();
                throw;
            }
        }

        /// <summary>
        /// キャッシュしているアセットを全てアンロード.
        /// </summary>
        public void Clear()
        {
            _cancellationDisposable.ThrowIfDisposed(this);
            foreach (var handle in _handles.AsSpan())
            {
                if (handle.IsValid())
                {
                    handle.Release();
                }
            }
            _handles.Clear();
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Clear();
            ListPool<AsyncOperationHandle>.Release(_handles);
            _cancellationDisposable.Cancel();
            _cancellationDisposable.Dispose();
        }
    }
}

#endif