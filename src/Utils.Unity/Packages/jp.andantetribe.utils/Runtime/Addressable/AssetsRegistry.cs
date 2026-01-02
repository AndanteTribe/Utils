#if ENABLE_ADDRESSABLES && ENABLE_UNITASK
#nullable enable

using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Assertions;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

namespace AndanteTribe.Utils.Unity.Addressable
{
    /// <summary>
    /// ロードしたアセットのハンドルをキャッシュしているレジストリ.
    /// </summary>
    public class AssetsRegistry : IDisposable
    {
        private static readonly IEqualityComparer<AsyncOperationHandle> s_equalityComparer = EqualityComparer.Create<AsyncOperationHandle>(
            static (x, y) => x.Equals(y), static x => x.GetHashCode());

        private readonly HashSet<AsyncOperationHandle> _handles = new(s_equalityComparer);

        public int Count => _handles.Count;

        /// <summary>
        /// アセットのロード.
        /// </summary>
        /// <param name="address"></param>
        /// <param name="cancellationToken"></param>
        /// <typeparam name="TObject"></typeparam>
        /// <returns></returns>
        public async UniTask<TObject> LoadAsync<TObject>(string address, CancellationToken cancellationToken) where TObject : Object
        {
            cancellationToken.ThrowIfCancellationRequested();
            var handle = Addressables.LoadAssetAsync<TObject>(address);
            _handles.Add(handle);
            return await handle.ToUniTask(cancellationToken: cancellationToken, autoReleaseWhenCanceled: true);
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
            cancellationToken.ThrowIfCancellationRequested();
            Assert.IsTrue(reference.IsValid());
            var handle = Addressables.LoadAssetAsync<TObject>(reference);
            _handles.Add(handle);
            return await handle.ToUniTask(cancellationToken: cancellationToken, autoReleaseWhenCanceled: true);
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
            var handle = Addressables.LoadAssetAsync<GameObject>(address);
            _handles.Add(handle);
            var obj = await handle.ToUniTask(cancellationToken: cancellationToken, autoReleaseWhenCanceled: true);
            if (!obj.TryGetComponent<TComponent>(out var component))
            {
                _handles.Remove(handle);
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
                _handles.Remove(handle);
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
            Assert.IsTrue(reference.IsValid());
            var handle = Addressables.LoadAssetAsync<GameObject>(reference);
            _handles.Add(handle);
            var obj = await handle.ToUniTask(cancellationToken: cancellationToken, autoReleaseWhenCanceled: true);
            if (!obj.TryGetComponent<TComponent>(out var component))
            {
                _handles.Remove(handle);
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
                _handles.Remove(handle);
                handle.Release();
                throw;
            }
        }

        /// <summary>
        /// キャッシュしているアセットを全てアンロード.
        /// </summary>
        public void Clear()
        {
            foreach (var handle in _handles)
            {
                if (handle.IsValid())
                {
                    handle.Release();
                }
            }
            _handles.Clear();
        }

        /// <inheritdoc/>
        public void Dispose() => Clear();
    }
}

#endif