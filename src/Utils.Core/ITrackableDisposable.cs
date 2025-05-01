using System;

namespace AndanteTribe.Utils
{
    public interface ITrackableDisposable : IDisposable
    {
        bool IsDisposed { get; }
    }
}