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
    public class AssetsRegistry : CancellationDisposable
    {
        private readonly List<AsyncOperationHandle> _handles = ListPool<AsyncOperationHandle>.Get();

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
            ThrowHelper.ThrowIfDisposedException(IsDisposed, this);
            using var _ = CreateLinkedToken(cancellationToken, out var ct);
            var handle = Addressables.LoadAssetAsync<TObject>(address);
            var result = await handle.ToUniTask(cancellationToken: ct, autoReleaseWhenCanceled: true);
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
            ThrowHelper.ThrowIfDisposedException(IsDisposed, this);
            using var _ = CreateLinkedToken(cancellationToken, out var ct);
            var handle = Addressables.LoadAssetAsync<TObject>(reference);
            var result = await handle.ToUniTask(cancellationToken: ct, autoReleaseWhenCanceled: true);
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
            ThrowHelper.ThrowIfDisposedException(IsDisposed, this);
            using var _ = CreateLinkedToken(cancellationToken, out var ct);
            var handle = Addressables.LoadAssetAsync<GameObject>(address);
            var obj = await handle.ToUniTask(cancellationToken: ct, autoReleaseWhenCanceled: true);
            if (!obj.TryGetComponent<TComponent>(out var component))
            {
                handle.Release();
                throw new InvalidOperationException($"指定の型 {typeof(TComponent)} は {handle.DebugName} から取得できませんでした。");
            }
            try
            {
                var result = await Object.InstantiateAsync(component, parent).WithCancellation(ct);
                _handles.Add(handle);
                return result[0];
            }
            catch (OperationCanceledException e) when(e.CancellationToken == ct)
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
            ThrowHelper.ThrowIfDisposedException(IsDisposed, this);
            using var _ = CreateLinkedToken(cancellationToken, out var ct);
            var handle = Addressables.LoadAssetAsync<GameObject>(reference);
            var obj = await handle.ToUniTask(cancellationToken: ct, autoReleaseWhenCanceled: true);
            if (!obj.TryGetComponent<TComponent>(out var component))
            {
                handle.Release();
                throw new InvalidOperationException($"指定の型 {typeof(TComponent)} は {handle.DebugName} から取得できませんでした。");
            }
            try
            {
                var result = await Object.InstantiateAsync(component, parent).WithCancellation(ct);
                _handles.Add(handle);
                return result[0];
            }
            catch (OperationCanceledException e) when(e.CancellationToken == ct)
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
            ThrowHelper.ThrowIfDisposedException(IsDisposed, this);
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
        public override void Dispose()
        {
            if (!IsDisposed)
            {
                Clear();
                ListPool<AsyncOperationHandle>.Release(_handles);
            }
            base.Dispose();
        }
    }
}

#endif