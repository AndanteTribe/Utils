using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace AndanteTribe.Utils.Unity.Tasks.Internal
{
    /// <summary>
    /// スレッドセーフなプールを持つ参照タプル. <see cref="System.ValueTuple"/>でボクシングするような場面での使用を想定.
    /// </summary>
    internal static class StateTuple
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static StateTuple<T1> Create<T1>(T1 item1) => StateTuple<T1>.Create(item1);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static StateTuple<T1, T2> Create<T1, T2>(T1 item1, T2 item2) => StateTuple<T1, T2>.Create(item1, item2);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static StateTuple<T1, T2, T3> Create<T1, T2, T3>(T1 item1, T2 item2, T3 item3) => StateTuple<T1, T2, T3>.Create(item1, item2, item3);
    }

    internal sealed class StateTuple<T1>
    {
        private static readonly ConcurrentQueue<StateTuple<T1>> s_pool = new();

        private T1 _item1 = default!;

        private StateTuple()
        {
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static StateTuple<T1> Create(T1 item1)
        {
            if (s_pool.TryDequeue(out var result))
            {
                result._item1 = item1;
                return result;
            }
            return new StateTuple<T1> { _item1 = item1 };
        }

        public void Deconstruct(out T1 item1)
        {
            item1 = _item1;
            _item1 = default!;
            s_pool.Enqueue(this);
        }
    }

    internal sealed class StateTuple<T1, T2>
    {
        private static readonly ConcurrentQueue<StateTuple<T1, T2>> s_pool = new();

        private T1 _item1 = default!;
        private T2 _item2 = default!;

        private StateTuple()
        {
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static StateTuple<T1, T2> Create(T1 item1, T2 item2)
        {
            if (s_pool.TryDequeue(out var result))
            {
                result._item1 = item1;
                result._item2 = item2;
                return result;
            }
            return new StateTuple<T1, T2> { _item1 = item1, _item2 = item2 };
        }

        public void Deconstruct(out T1 item1, out T2 item2)
        {
            item1 = _item1;
            item2 = _item2;
            _item1 = default!;
            _item2 = default!;
            s_pool.Enqueue(this);
        }
    }

    internal sealed class StateTuple<T1, T2, T3>
    {
        private static readonly ConcurrentQueue<StateTuple<T1, T2, T3>> s_pool = new();

        private T1 _item1 = default!;
        private T2 _item2 = default!;
        private T3 _item3 = default!;

        private StateTuple()
        {
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static StateTuple<T1, T2, T3> Create(T1 item1, T2 item2, T3 item3)
        {
            if (s_pool.TryDequeue(out var result))
            {
                result._item1 = item1;
                result._item2 = item2;
                result._item3 = item3;
                return result;
            }
            return new StateTuple<T1, T2, T3> { _item1 = item1, _item2 = item2, _item3 = item3 };
        }

        public void Deconstruct(out T1 item1, out T2 item2, out T3 item3)
        {
            item1 = _item1;
            item2 = _item2;
            item3 = _item3;
            _item1 = default!;
            _item2 = default!;
            _item3 = default!;
            s_pool.Enqueue(this);
        }
    }
}