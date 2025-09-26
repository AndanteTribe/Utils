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
        /// <remarks>
        /// 登録した<see cref="T"/>型は自動的に<see cref="T"/>型としてもバインドされます.
        /// </remarks>
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

        /// <summary>
        /// <see cref="IInitializable"/>を実装したエントリーポイントオブジェクトをキューに登録します.
        /// </summary>
        /// <remarks>
        /// 登録した<see cref="TImplement"/>型でバインドされず、<see cref="TInterface"/>型としてバインドされます.
        /// </remarks>
        /// <param name="builder"></param>
        /// <param name="waitForCompletion">対象の型の初期化が完了するまで待機するかどうか.</param>
        /// <typeparam name="TInterface"></typeparam>
        /// <typeparam name="TImplement"></typeparam>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">
        /// RegisterEnqueueをRegisterEntryPointsを呼び出すことで生成されるスコープ内で使用しなかった場合に発生する警告.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RegistrationBuilder RegisterEnqueue<TInterface, TImplement>(this IContainerBuilder builder, bool waitForCompletion = true)
            where TImplement : class, TInterface, IInitializable
        {
            if (builder is EntryPointsQueueBuilder epBuilder)
            {
                return epBuilder.RegisterEnqueue<TInterface, TImplement>(waitForCompletion);
            }

            throw new InvalidOperationException("RegisterEnqueueはRegisterEntryPointsを呼び出すことで生成されるスコープ内で使用してください.");
        }

        /// <summary>
        /// <see cref="IInitializable"/>を実装したエントリーポイントオブジェクトのインスタンスをキューに登録します.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="instance"></param>
        /// <param name="waitForCompletion">対象の型の初期化が完了するまで待機するかどうか.</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">
        /// RegisterEnqueueをRegisterEntryPointsを呼び出すことで生成されるスコープ内で使用しなかった場合に発生する警告.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RegistrationBuilder RegisterInstanceEnqueue<T>(this IContainerBuilder builder, T instance, bool waitForCompletion = true)
            where T : class, IInitializable
        {
            if (builder is EntryPointsQueueBuilder epBuilder)
            {
                return epBuilder.RegisterInstanceEnqueue(instance, waitForCompletion);
            }

            throw new InvalidOperationException("RegisterInstanceEnqueueはRegisterEntryPointsを呼び出すことで生成されるスコープ内で使用してください.");
        }

        /// <summary>
        /// <see cref="IInitializable"/>を実装したエントリーポイントオブジェクトのインスタンスをキューに登録します.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="instance"></param>
        /// <param name="waitForCompletion">対象の型の初期化が完了するまで待機するかどうか.</param>
        /// <typeparam name="TInterface"></typeparam>
        /// <typeparam name="TImplement"></typeparam>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">
        /// RegisterEnqueueをRegisterEntryPointsを呼び出すことで生成されるスコープ内で使用しなかった場合に発生する警告.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RegistrationBuilder RegisterInstanceEnqueue<TInterface, TImplement>(this IContainerBuilder builder, TImplement instance, bool waitForCompletion = true)
            where TImplement : class, TInterface, IInitializable
        {
            if (builder is EntryPointsQueueBuilder epBuilder)
            {
                return epBuilder.RegisterInstanceEnqueue<TInterface, TImplement>(instance, waitForCompletion);
            }

            throw new InvalidOperationException("RegisterInstanceEnqueueはRegisterEntryPointsを呼び出すことで生成されるスコープ内で使用してください.");
        }

        /// <summary>
        /// <see cref="IInitializable"/>を実装したエントリーポイントオブジェクトのインスタンスをキューに登録します.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="component"></param>
        /// <param name="waitForCompletion">対象の型の初期化が完了するまで待機するかどうか.</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">
        /// RegisterEnqueueをRegisterEntryPointsを呼び出すことで生成されるスコープ内で使用しなかった場合に発生する警告.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RegistrationBuilder RegisterComponentEnqueue<T>(this IContainerBuilder builder, T component, bool waitForCompletion = true)
            where T : UnityEngine.MonoBehaviour, IInitializable
        {
            if (builder is EntryPointsQueueBuilder epBuilder)
            {
                return epBuilder.RegisterComponentEnqueue(component, waitForCompletion);
            }

            throw new InvalidOperationException("RegisterComponentEnqueueはRegisterEntryPointsを呼び出すことで生成されるスコープ内で使用してください.");
        }
    }

    public readonly struct EntryPointRegistrationScope : IDisposable
    {
        private static readonly ObjectPool<EntryPointsQueueBuilder> s_pool = new (
            static () => new EntryPointsQueueBuilder(), static ep => ep.Reset());

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
        public List<Func<IObjectResolver, CancellationToken, ValueTask>> Queue { get; private set; } = new();
        internal IContainerBuilder _builder = null!;
        internal Lifetime _lifetime;

        public void Reset() => Queue = ListPool<Func<IObjectResolver, CancellationToken, ValueTask>>.Get();

        public RegistrationBuilder RegisterEnqueue<T>(bool waitForCompletion = true) where T : class, IInitializable
        {
            RegisterEnqueueInternal<T>(waitForCompletion);
            return _builder.Register<IInitializable, T>(_lifetime).AsSelf();
        }

        public RegistrationBuilder RegisterEnqueue<TInterface, TImplement>(bool waitForCompletion = true)
            where TImplement : class, TInterface, IInitializable
        {
            RegisterEnqueueInternal<TInterface, TImplement>(waitForCompletion);
            return _builder.Register<IInitializable, TImplement>(_lifetime).As<TInterface>();
        }

        public RegistrationBuilder RegisterInstanceEnqueue<T>(T instance, bool waitForCompletion = true) where T : class, IInitializable
        {
            RegisterEnqueueInternal<T>(waitForCompletion);
            return _builder.RegisterInstance<IInitializable, T>(instance).AsSelf();
        }

        public RegistrationBuilder RegisterInstanceEnqueue<TInterface, TImplement>(TImplement instance, bool waitForCompletion = true)
            where TImplement : class, TInterface, IInitializable
        {
            RegisterEnqueueInternal<TInterface, TImplement>(waitForCompletion);
            return _builder.RegisterInstance<IInitializable, TImplement>(instance).As<TInterface>();
        }

        public RegistrationBuilder RegisterComponentEnqueue<T>(T component, bool waitForCompletion = true) where T : UnityEngine.MonoBehaviour, IInitializable
        {
            RegisterEnqueueInternal<T>(waitForCompletion);
            return _builder.RegisterComponent(component).As<IInitializable>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void RegisterEnqueueInternal<T>(bool waitForCompletion = true) where T : class, IInitializable
        {
            if (waitForCompletion)
            {
                Queue.Add(static (resolver, token) => resolver.Resolve<T>().InitializeAsync(token));
            }
            else
            {
                Queue.Add(static (resolver, token) =>
                {
                    _ = resolver.Resolve<T>().InitializeAsync(token);
                    return default;
                });
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void RegisterEnqueueInternal<TInterface, TImplement>(bool waitForCompletion = true)
            where TImplement : class, TInterface, IInitializable
        {
            if (waitForCompletion)
            {
                Queue.Add(static (resolver, token) =>
                {
                    if (resolver.Resolve<TInterface>() is IInitializable initializable)
                    {
                        return initializable.InitializeAsync(token);
                    }

                    throw new InvalidOperationException(typeof(TImplement).FullName + " is not IInitializable.");
                });
            }
            else
            {
                Queue.Add(static (resolver, token) =>
                {
                    if (resolver.Resolve<TInterface>() is IInitializable initializable)
                    {
                        _ = initializable.InitializeAsync(token);
                        return default;
                    }

                    throw new InvalidOperationException(typeof(TImplement).FullName + " is not IInitializable.");
                });
            }
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
        private readonly CancellationToken _cancellationToken;
        private readonly List<Func<IObjectResolver, CancellationToken, ValueTask>> _entryPoints;

        public EntryPointContainer(IObjectResolver resolver, LifetimeScope lifetimeScope, List<Func<IObjectResolver, CancellationToken, ValueTask>> entryPoints)
        {
            _resolver = resolver;
            _cancellationToken = lifetimeScope.destroyCancellationToken;
            _entryPoints = entryPoints;
        }

        /// <inheritdoc />
        void IStartable.Start()
        {
            _ = StartAsync();

            async ValueTask StartAsync()
            {
                foreach (var entrypoint in _entryPoints)
                {
                    await entrypoint(_resolver, _cancellationToken);
                }
            }
        }

        /// <inheritdoc />
        void IDisposable.Dispose()
        {
            if (!_cancellationToken.IsCancellationRequested)
            {
                ListPool<Func<IObjectResolver, CancellationToken, ValueTask>>.Release(_entryPoints);
            }
        }
    }
}

#endif
