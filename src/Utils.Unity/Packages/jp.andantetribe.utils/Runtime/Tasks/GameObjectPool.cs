#if ENABLE_UNITASK
#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AndanteTribe.Utils.Unity.Tasks
{
    public sealed class GameObjectPool<T> : IDisposable, IReadOnlyCollection<T> where T : MonoBehaviour
    {
        /// <summary>
        /// プールのルートオブジェクト.
        /// </summary>
        private readonly Transform _root;

        /// <summary>
        /// プールのオリジナルの参照.
        /// </summary>
        private readonly IObjectReference<T> _reference;

        /// <summary>
        /// プールのスタック.
        /// </summary>
        private readonly List<T> _pool;

        /// <summary>
        /// キャンセルトークンソース.
        /// </summary>
        private readonly CancellationTokenSource _cancellationDisposable = new();

        /// <inheritdoc/>
        public int Count => _pool.Count;

        public GameObjectPool(Transform root, IObjectReference<T> reference, int capacity)
        {
            _root = root;
            _reference = reference;
            _pool = new List<T>(capacity);
        }

        /// <summary>
        /// 事前にプールを確保する.
        /// </summary>
        /// <param name="count"></param>
        /// <param name="cancellationToken"></param>
        public async UniTask PreallocateAsync(int count, CancellationToken cancellationToken = default)
        {
            _cancellationDisposable.ThrowIfDisposed(this);
            cancellationToken.ThrowIfCancellationRequested();
            using var cts = _cancellationDisposable.CreateLinkedTokenSource(cancellationToken);
            var original = await _reference.LoadAsync(cts.Token);
            var instances = await Object.InstantiateAsync(original, count, _root).WithCancellation(cts.Token);
            foreach (var instance in instances)
            {
                instance.gameObject.SetActive(false);
                _pool.Add(instance);
            }
            _pool.TrimExcess();
        }

        /// <summary>
        /// プールからオブジェクトを取得する.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async UniTask<T> RentAsync(CancellationToken cancellationToken = default)
        {
            _cancellationDisposable.ThrowIfDisposed(this);
            cancellationToken.ThrowIfCancellationRequested();
            if (_pool.Count > 0)
            {
                var instance = _pool[0];
                _pool.RemoveAt(0);
                instance.gameObject.SetActive(true);
                return instance;
            }

            using var cts = _cancellationDisposable.CreateLinkedTokenSource(cancellationToken);
            var original = await _reference.LoadAsync(cts.Token);
            var results = await Object.InstantiateAsync(original, _root).WithCancellation(cts.Token);
            return results[0];
        }

        /// <summary>
        /// プールから非同期でインスタンスを取得する(usingステートメントで使用するためのメソッド).
        /// </summary>
        /// <param name="cancellationToken">キャンセルトークン.</param>
        /// <returns>取得したインスタンスと解放用の<see cref="System.IDisposable"/>.</returns>
        public async UniTask<Handle> RentAsyncHandle(CancellationToken cancellationToken = default)
        {
            var instance = await RentAsync(cancellationToken);
            return new Handle(this, instance);
        }

        /// <summary>
        /// プールにオブジェクトを返却する.
        /// </summary>
        /// <param name="element">返却するインスタンス.</param>
        public void Return(T element)
        {
            _cancellationDisposable.ThrowIfDisposed(this);
            element.gameObject.SetActive(false);
            element.transform.SetParent(_root);
            _pool.Add(element);
        }

        /// <summary>
        /// プールをクリアする.
        /// </summary>
        public void Clear()
        {
            _cancellationDisposable.ThrowIfDisposed(this);
            foreach (var item in _pool.AsSpan())
            {
                Object.Destroy(item.gameObject);
            }
            _pool.Clear();
        }

        public List<T>.Enumerator GetEnumerator() => _pool.GetEnumerator();

        /// <inheritdoc/>
        public void Dispose()
        {
            Clear();
            _reference.Dispose();
            _cancellationDisposable.Cancel();
            _cancellationDisposable.Dispose();
        }

        /// <inheritdoc/>
        IEnumerator<T> IEnumerable<T>.GetEnumerator() => ((IEnumerable<T>)_pool).GetEnumerator();

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// プールから取得したインスタンスのハンドル構造体.
        /// </summary>
        /// <remarks>
        /// 基本的にusingスコープで囲んで使用.
        /// </remarks>
        public readonly struct Handle : IDisposable
        {
            private readonly GameObjectPool<T> _gameObjectPool;
            private readonly T _instance;

            internal Handle(GameObjectPool<T> gameObjectPool, T instance)
            {
                _gameObjectPool = gameObjectPool;
                _instance = instance;
            }

            void IDisposable.Dispose() => _gameObjectPool.Return(_instance);
        }
    }
}

#endif
