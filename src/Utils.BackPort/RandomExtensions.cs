#if !NET6_0_OR_GREATER

using System.Runtime.CompilerServices;

namespace System;

public static class RandomExtensions
{
    private static readonly Random s_shared = new ThreadSafeRandom();

    /// <summary>
    /// <see cref="Random"/> extensions.
    /// </summary>
    extension(Random)
    {
        /// <summary>
        /// Provides a thread-safe <see cref="Random"/> instance that may be used concurrently from any thread.
        /// </summary>
        public static Random Shared
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => s_shared;
        }
    }

    private sealed class ThreadSafeRandom : Random
    {
        private readonly object _lock = new object();

        public override int Next()
        {
            lock (_lock)
            {
                return base.Next();
            }
        }

        public override int Next(int maxValue)
        {
            lock (_lock)
            {
                return base.Next(maxValue);
            }
        }

        public override int Next(int minValue, int maxValue)
        {
            lock (_lock)
            {
                return base.Next(minValue, maxValue);
            }
        }

        public override void NextBytes(Span<byte> buffer)
        {
            lock (_lock)
            {
                base.NextBytes(buffer);
            }
        }

        public override void NextBytes(byte[] buffer)
        {
            lock (_lock)
            {
                base.NextBytes(buffer);
            }
        }

        public override double NextDouble()
        {
            lock (_lock)
            {
                return base.NextDouble();
            }
        }
    }
}

#endif