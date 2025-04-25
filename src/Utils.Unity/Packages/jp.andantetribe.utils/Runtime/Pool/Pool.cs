#if ENABLE_UNITASK
#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AndanteTribe.Utils
{
    public sealed class Pool<T> : IDisposable, IReadOnlyCollection<T> where T : MonoBehaviour
    {
        /// <summary>
        /// プールのルートオブジェクト.
        /// </summary>
        private readonly Transform _root;

        /// <summary>
        /// プールのオリジナルの参照.
        /// </summary>
        private readonly IOriginalReference<T> _reference;

        /// <summary>
        /// プールのスタック.
        /// </summary>
        private readonly Stack<T> _pool;

        /// <summary>
        /// <see cref="Dispose"/>されたら破棄されるトークン.
        /// </summary>
        private readonly CancellationTokenSource _disposableTokenSource = new();

        /// <inheritdoc/>
        public int Count => _pool.Count;

        /// <summary>
        /// 破棄済みかどうか.
        /// </summary>
        public bool IsDisposed => _disposableTokenSource.IsCancellationRequested;

        public Pool(Transform root, IOriginalReference<T> reference, int capacity)
        {
            _root = root;
            _reference = reference;
            _pool = new Stack<T>(capacity);
        }

        /// <summary>
        /// 事前にプールを確保する.
        /// </summary>
        /// <param name="count"></param>
        /// <param name="cancellationToken"></param>
        public async UniTask PreallocateAsync(int count, CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            cancellationToken.ThrowIfCancellationRequested();
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(_disposableTokenSource.Token, cancellationToken);
            var original = await _reference.LoadAsync(cts.Token);
            var instances = await Object.InstantiateAsync(original, count, _root).WithCancellation(cts.Token);
            foreach (var instance in instances)
            {
                instance.gameObject.SetActive(false);
                _pool.Push(instance);
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
            ThrowIfDisposed();
            cancellationToken.ThrowIfCancellationRequested();
            if (_pool.TryPop(out var v))
            {
                return v;
            }

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(_disposableTokenSource.Token, cancellationToken);
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
            ThrowIfDisposed();
            element.gameObject.SetActive(false);
            element.transform.SetParent(_root);
            _pool.Push(element);
        }

        /// <summary>
        /// 破棄済みかどうかをチェックする.
        /// </summary>
        /// <exception cref="ObjectDisposedException">破棄済みの場合にスローされる例外.</exception>
        public void ThrowIfDisposed()
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(Pool<T>));
            }
        }

        /// <summary>
        /// プールをクリアする.
        /// </summary>
        public void Clear()
        {
            ThrowIfDisposed();
            foreach (var item in _pool)
            {
                Object.Destroy(item.gameObject);
            }
            _pool.Clear();
        }

        public Stack<T>.Enumerator GetEnumerator() => _pool.GetEnumerator();

        /// <inheritdoc/>
        public void Dispose()
        {
            if (!IsDisposed)
            {
                Clear();
                _disposableTokenSource.Cancel();
                _disposableTokenSource.Dispose();
            }
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
            private readonly Pool<T> _pool;
            private readonly T _instance;

            internal Handle(Pool<T> pool, T instance)
            {
                _pool = pool;
                _instance = instance;
            }

            void IDisposable.Dispose() => _pool.Return(_instance);
        }
    }
}

#endif
