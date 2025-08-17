#if ENABLE_VCONTAINER
#nullable enable

using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.Pool;
using VContainer;
using VContainer.Diagnostics;
using VContainer.Unity;

namespace AndanteTribe.Utils.Unity.VContainer
{
    /// <summary>
    /// VContainer周りの拡張クラス.
    /// </summary>
    public static partial class VContainerExtensions
    {
        /// <summary>
        /// エントリーポイントのバインドを行うためのスコープを生成します.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="entryPointsBuilder">エントリーポイントをキュー登録する専用の<see cref="IContainerBuilder"/>.</param>
        /// <returns>エントリーポイントの登録を行うためのスコープ.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static EntryPointRegistrationScope RegisterEntryPoints(this IContainerBuilder builder, out IContainerBuilder entryPointsBuilder)
        {
            var scope = new EntryPointRegistrationScope(builder, Lifetime.Singleton);
            entryPointsBuilder = scope._entryPointsBuilder;
            return scope;
        }

        /// <summary>
        /// <see cref="IInitializable"/>を実装したエントリーポイントオブジェクトをキューに登録します.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="waitForCompletion">対象の型の初期化が完了するまで待機するかどうか.</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">
        /// RegisterEnqueueをRegisterEntryPointsを呼び出すことで生成されるスコープ内で使用しなかった場合に発生する警告.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RegistrationBuilder RegisterEnqueue<T>(this IContainerBuilder builder, bool waitForCompletion = true)
            where T : class, IInitializable
        {
            if (builder is EntryPointsQueueBuilder epBuilder)
            {
                return epBuilder.RegisterEnqueue<T>(waitForCompletion);
            }

            throw new InvalidOperationException("RegisterEnqueueはRegisterEntryPointsを呼び出すことで生成されるスコープ内で使用してください.");
        }
    }

    public readonly struct EntryPointRegistrationScope : IDisposable
    {
        private static readonly ObjectPool<EntryPointsQueueBuilder> s_pool = new (
            createFunc: static () => new EntryPointsQueueBuilder(), actionOnRelease: ep => ep.Queue.AsSpan().Clear());

        private readonly IContainerBuilder _builder;
        internal readonly EntryPointsQueueBuilder _entryPointsBuilder;

        internal EntryPointRegistrationScope(IContainerBuilder builder, in Lifetime lifetime)
        {
            _builder = builder;
            _entryPointsBuilder = s_pool.Get();
            _entryPointsBuilder._builder = builder;
            _entryPointsBuilder._lifetime = lifetime;
        }

        void IDisposable.Dispose()
        {
            _builder.RegisterEntryPoint<EntryPointContainer>().WithParameter(_entryPointsBuilder.Queue);
            s_pool.Release(_entryPointsBuilder);
        }
    }

    internal sealed class EntryPointsQueueBuilder : IContainerBuilder
    {
        internal IContainerBuilder _builder = null!;
        internal Lifetime _lifetime;

        public Func<IObjectResolver, CancellationToken, ValueTask>[] Queue =
            Array.Empty<Func<IObjectResolver, CancellationToken, ValueTask>>();

        private int _size = 0;

        internal RegistrationBuilder RegisterEnqueue<T>(bool waitForCompletion = true) where T : class, IInitializable
        {
            if (Queue.Length == _size)
            {
                var newQueue = ArrayPool<Func<IObjectResolver, CancellationToken, ValueTask>>.Shared.Rent(_size + 1);
                Queue.AsSpan().CopyTo(newQueue);
                ArrayPool<Func<IObjectResolver, CancellationToken, ValueTask>>.Shared.Return(Queue);
                Queue = newQueue;
            }
            if (waitForCompletion)
            {
                Queue[_size++] = static (resolver, token) => resolver.Resolve<T>().InitializeAsync(token);
            }
            else
            {
                Queue[_size++] = static (resolver, token) =>
                {
                    _ = resolver.Resolve<T>().InitializeAsync(token);
                    return default;
                };
            }

            return _builder.Register<IInitializable, T>(_lifetime);
        }

        /// <inheritdoc />
        /// <exception cref="NotSupportedException">
        /// RegisterEnqueue以外のメソッドを使用する誤使用に発生する警告.
        /// </exception>
        T IContainerBuilder.Register<T>(T registrationBuilder)
            => throw NotSupportedExceptionForEntryPointsQueueBuilder();

        /// <inheritdoc />
        /// <exception cref="NotSupportedException">
        /// RegisterEnqueue以外のメソッドを使用する誤使用に発生する警告.
        /// </exception>
        void IContainerBuilder.RegisterBuildCallback(Action<IObjectResolver> container) =>
            throw NotSupportedExceptionForEntryPointsQueueBuilder();

        /// <inheritdoc />
        /// <exception cref="NotSupportedException">
        /// RegisterEnqueue以外のメソッドを使用する誤使用に発生する警告.
        /// </exception>
        bool IContainerBuilder.Exists(Type type, bool includeInterfaceTypes, bool findParentScopes) =>
            throw NotSupportedExceptionForEntryPointsQueueBuilder();

        /// <inheritdoc />
        /// <exception cref="NotSupportedException">
        /// RegisterEnqueue以外のメソッドを使用する誤使用に発生する警告.
        /// </exception>
        object IContainerBuilder.ApplicationOrigin
        {
            get => throw NotSupportedExceptionForEntryPointsQueueBuilder();
            set => throw NotSupportedExceptionForEntryPointsQueueBuilder();
        }

        /// <inheritdoc />
        /// <exception cref="NotSupportedException">
        /// RegisterEnqueue以外のメソッドを使用する誤使用に発生する警告.
        /// </exception>
        DiagnosticsCollector IContainerBuilder.Diagnostics
        {
            get => throw NotSupportedExceptionForEntryPointsQueueBuilder();
            set => throw NotSupportedExceptionForEntryPointsQueueBuilder();
        }

        /// <inheritdoc />
        /// <exception cref="NotSupportedException">
        /// RegisterEnqueue以外のメソッドを使用する誤使用に発生する警告.
        /// </exception>
        int IContainerBuilder.Count => throw NotSupportedExceptionForEntryPointsQueueBuilder();

        /// <inheritdoc />
        /// <exception cref="NotSupportedException">
        /// RegisterEnqueue以外のメソッドを使用する誤使用に発生する警告.
        /// </exception>
        RegistrationBuilder IContainerBuilder.this[int index]
        {
            get => throw NotSupportedExceptionForEntryPointsQueueBuilder();
            set => throw NotSupportedExceptionForEntryPointsQueueBuilder();
        }

        private static Exception NotSupportedExceptionForEntryPointsQueueBuilder() =>
            new NotSupportedException("RegisterEnqueue以外のメソッドを使用しないでください。");
    }

    internal sealed class EntryPointContainer : IStartable, IDisposable
    {
        private readonly IObjectResolver _resolver;
        private readonly CancellationTokenSource _cancellationDisposable = new();
        private readonly List<Func<IObjectResolver, CancellationToken, ValueTask>> _entryPoints;

        public EntryPointContainer(IObjectResolver resolver, List<Func<IObjectResolver, CancellationToken, ValueTask>> entryPoints)
        {
            _resolver = resolver;
            _entryPoints = entryPoints;
        }

        /// <inheritdoc />
        void IStartable.Start()
        {
            _cancellationDisposable.ThrowIfDisposed(this);
            _ = StartAsync();

            async ValueTask StartAsync()
            {
                foreach (var entrypoint in _entryPoints)
                {
                    await entrypoint(_resolver, _cancellationDisposable.Token);
                }
            }
        }

        /// <inheritdoc />
        void IDisposable.Dispose()
        {
            if (!_cancellationDisposable.IsCancellationRequested)
            {
                ListPool<Func<IObjectResolver, CancellationToken, ValueTask>>.Release(_entryPoints);
                _cancellationDisposable.Cancel();
                _cancellationDisposable.Dispose();
            }
        }
    }
}

#endif
