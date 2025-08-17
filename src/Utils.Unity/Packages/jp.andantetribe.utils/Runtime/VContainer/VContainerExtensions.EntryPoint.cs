#if ENABLE_VCONTAINER
#nullable enable

using System;
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
        /// エントリーポイント制御関数.
        /// </summary>
        /// <param name="builder"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static EntryPointsQueueBuilder RegisterEntryPoints(this IContainerBuilder builder) =>
            new(builder, Lifetime.Singleton);
    }

    /// <summary>
    /// <see cref="IInitializable"/>を実装するオブジェクトを、バインド順にエントリー発火もするBuilder
    /// </summary>
    /// <remarks>
    /// 基本的にusingスコープで囲んで使う.
    /// </remarks>
    public readonly struct EntryPointsQueueBuilder : IContainerBuilder, IDisposable
    {
        private readonly IContainerBuilder _builder;
        private readonly Lifetime _lifetime;
        private readonly List<Func<IObjectResolver, CancellationToken, ValueTask>> _queue;

        /// <summary>
        /// Initialize a new instance of <see cref="EntryPointsQueueBuilder"/>.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="lifetime"></param>
        internal EntryPointsQueueBuilder(IContainerBuilder builder, Lifetime lifetime)
        {
            _builder = builder;
            _lifetime = lifetime;
            _queue = ListPool<Func<IObjectResolver, CancellationToken, ValueTask>>.Get();
        }

        /// <summary>
        /// <see cref="IInitializable"/>を実装するオブジェクトをバインドし、エントリーポイントとしてキューに追加します.
        /// </summary>
        /// <param name="waitForCompletion">対象の型の初期化処理の終了を待機して、次の対象の初期化処理を実行するかどうか.</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public RegistrationBuilder RegisterEnqueue<T>(bool waitForCompletion = true) where T : class, IInitializable
        {
            if (waitForCompletion)
            {
                _queue.Add(static (resolver, token) => resolver.Resolve<T>().InitializeAsync(token));
            }
            else
            {
                _queue.Add(static (resolver, token) =>
                {
                    _ = resolver.Resolve<T>().InitializeAsync(token);
                    return default;
                });
            }

            return _builder.Register<IInitializable, T>(_lifetime);
        }

        void IDisposable.Dispose() =>
            _builder.RegisterEntryPoint<EntryPointContainer>().WithParameter(_queue);

        /// <inheritdoc />
        /// <exception cref="NotSupportedException">
        /// EntryPointsQueueBuilderをわざわざキャストして使用するなどの誤使用に発生する警告.
        /// </exception>
        T IContainerBuilder.Register<T>(T registrationBuilder)
            => throw NotSupportedExceptionForEntryPointsQueueBuilder();

        /// <inheritdoc />
        /// <exception cref="NotSupportedException">
        /// EntryPointsQueueBuilderをわざわざキャストして使用するなどの誤使用に発生する警告.
        /// </exception>
        void IContainerBuilder.RegisterBuildCallback(Action<IObjectResolver> container) =>
            throw NotSupportedExceptionForEntryPointsQueueBuilder();

        /// <inheritdoc />
        /// <exception cref="NotSupportedException">
        /// EntryPointsQueueBuilderをわざわざキャストして使用するなどの誤使用に発生する警告.
        /// </exception>
        bool IContainerBuilder.Exists(Type type, bool includeInterfaceTypes, bool findParentScopes) =>
            throw NotSupportedExceptionForEntryPointsQueueBuilder();

        /// <inheritdoc />
        /// <exception cref="NotSupportedException">
        /// EntryPointsQueueBuilderをわざわざキャストして使用するなどの誤使用に発生する警告.
        /// </exception>
        object IContainerBuilder.ApplicationOrigin
        {
            get => throw NotSupportedExceptionForEntryPointsQueueBuilder();
            set => throw NotSupportedExceptionForEntryPointsQueueBuilder();
        }

        /// <inheritdoc />
        /// <exception cref="NotSupportedException">
        /// EntryPointsQueueBuilderをわざわざキャストして使用するなどの誤使用に発生する警告.
        /// </exception>
        DiagnosticsCollector IContainerBuilder.Diagnostics
        {
            get => throw NotSupportedExceptionForEntryPointsQueueBuilder();
            set => throw NotSupportedExceptionForEntryPointsQueueBuilder();
        }

        /// <inheritdoc />
        /// <exception cref="NotSupportedException">
        /// EntryPointsQueueBuilderをわざわざキャストして使用するなどの誤使用に発生する警告.
        /// </exception>
        int IContainerBuilder.Count => throw NotSupportedExceptionForEntryPointsQueueBuilder();

        /// <inheritdoc />
        /// <exception cref="NotSupportedException">
        /// EntryPointsQueueBuilderをわざわざキャストして使用するなどの誤使用に発生する警告.
        /// </exception>
        RegistrationBuilder IContainerBuilder.this[int index]
        {
            get => throw NotSupportedExceptionForEntryPointsQueueBuilder();
            set => throw NotSupportedExceptionForEntryPointsQueueBuilder();
        }

        private static Exception NotSupportedExceptionForEntryPointsQueueBuilder() =>
            new NotSupportedException("EntryPointsQueueBuilderをわざわざキャストして使わないでください。使い方を間違えています。");
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
