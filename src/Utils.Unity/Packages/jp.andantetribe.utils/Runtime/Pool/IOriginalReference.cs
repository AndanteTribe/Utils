#if ENABLE_UNITASK
#nullable enable

using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace AndanteTribe.Utils
{
    public interface IOriginalReference<T> : IDisposable where T : MonoBehaviour
    {
        UniTask<T> LoadAsync(CancellationToken cancellationToken);
    }
}

#endif