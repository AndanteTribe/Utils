#if ENABLE_VCONTAINER
#nullable enable

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.Pool;
using VContainer;
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
        /// <param name="configuration"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RegisterEntryPoints(this IContainerBuilder builder, Action<EntryPointsQueueBuilder> configuration)
        {
            var entryPointsBuilder = new EntryPointsQueueBuilder(builder, Lifetime.Singleton);
            configuration(entryPointsBuilder);
            builder.RegisterEntryPoint<EntryPointContainer>().WithParameter(entryPointsBuilder.GetQueue);
        }
    }

    /// <summary>
    /// <see cref="IInitializable"/>を実装するオブジェクトを、バインド順にエントリー発火もするBuilder
    /// </summary>
    public readonly struct EntryPointsQueueBuilder
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
        /// <param name="waitForCompletion"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public RegistrationBuilder Add<T>(bool waitForCompletion = false) where T : class, IInitializable
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

        internal List<Func<IObjectResolver, CancellationToken, ValueTask>> GetQueue() => _queue;
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
                Dispose();
            }
        }

        /// <inheritdoc />
        public void Dispose()
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