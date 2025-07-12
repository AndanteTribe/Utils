#if ENABLE_VCONTAINER
#nullable enable

using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace AndanteTribe.Utils.VContainer
{
    /// <summary>
    /// <see cref="LifetimeScope"/>の共通基底クラス.
    /// </summary>
    public class LifetimeScopeBase : LifetimeScope
    {
        [SerializeField]
        [Tooltip("Bind も Inject もする")]
        private Component[] _autoBindComponents = Array.Empty<Component>();

        /// <inheritdoc/>
        protected override void Configure(IContainerBuilder builder)
        {
            foreach (var component in _autoBindComponents.AsSpan())
            {
                if (component == null || !component.gameObject.scene.IsValid())
                {
                    Debug.LogError("autoBindComponentsにnullまたはシーン外のコンポーネントが指定されています");
                    continue;
                }
                builder.Register(new AnonymousBuilder(component)).AsSelf().AsImplementedInterfaces();
            }
        }

        private sealed class AnonymousBuilder : RegistrationBuilder
        {
            private readonly object _instance;

            public AnonymousBuilder(object instance) : base(instance.GetType(), Lifetime.Singleton) => _instance = instance;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public override Registration Build()
            {
                var spawner = new AnonymousProvider(_instance);
                return new Registration(ImplementationType, Lifetime, InterfaceTypes, spawner);
            }
        }

        private sealed class AnonymousProvider : IInstanceProvider
        {
            private readonly object _instance;

            public AnonymousProvider(object instance) => _instance = instance;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public object SpawnInstance(IObjectResolver resolver) => _instance;
        }
    }
}
#endif
