#if ENABLE_VCONTAINER
#nullable enable

using System;
using System.Runtime.CompilerServices;
using AndanteTribe.Utils.GameServices;
using VContainer;
using VContainer.Unity;

namespace AndanteTribe.Utils.Unity.VContainer
{
    /// <summary>
    /// 使い捨ての<see cref="IInstaller"/>のプール.
    /// </summary>
    public static class InstallerPool
    {
        /// <summary>
        /// 一時的なスコープ用の<see cref="IInstaller"/>を取得します.
        /// </summary>
        /// <param name="provider"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TempInstaller RentScope(Provider provider)
        {
            return TempInstaller.Create(provider);
        }

        public sealed class TempInstaller : IInstaller, IDisposable
        {
            private static TempInstaller? s_head;

            private Provider _provider;
            private TempInstaller? _next;

            private TempInstaller()
            {
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static TempInstaller Create(Provider provider)
            {
                if (s_head is null)
                {
                    return new TempInstaller { _provider = provider };
                }

                var installer = s_head;
                s_head = s_head._next;
                installer._next = null;
                installer._provider = provider;
                return installer;
            }

            /// <inheritdoc />
            void IInstaller.Install(IContainerBuilder builder)
            {
                foreach (var (type, instance) in _provider.Bindings)
                {
                    builder.RegisterInstance(instance, type);
                }
            }

            /// <inheritdoc />
            void IDisposable.Dispose()
            {
                _provider = Provider.Empty;
                _next = s_head;
                s_head = this;
            }
        }
    }
}

#endif