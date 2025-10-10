#nullable enable

using System.Globalization;
using System.Threading;
using UnityEngine;

[assembly: UnityEngine.Scripting.AlwaysLinkAssembly]

namespace AndanteTribe.Utils.Unity
{
#if UNITY_EDITOR
    [UnityEditor.InitializeOnLoad]
#endif
    internal static class Initializer
    {
        static Initializer() => SetDefaultCulture();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void SetDefaultCulture()
        {
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
        }
    }
}