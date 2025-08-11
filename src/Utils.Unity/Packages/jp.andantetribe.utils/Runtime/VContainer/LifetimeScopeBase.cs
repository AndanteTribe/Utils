#if ENABLE_VCONTAINER
#nullable enable

using System;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace AndanteTribe.Utils.Unity.VContainer
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
#if UNITY_EDITOR || DEVELOP_BUILD
                if (component == null || !component.gameObject.scene.IsValid())
                {
                    Debug.LogError("autoBindComponentsにnullまたはシーン外のコンポーネントが指定されています");
                    continue;
                }
#endif
                builder.RegisterInstance(component).As(component.GetType()).AsImplementedInterfaces();
            }
        }
    }
}
#endif
