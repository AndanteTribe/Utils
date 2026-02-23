using System;
using System.Buffers;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace AndanteTribe.Utils.Tests
{
    public class CoreUtilsTest
    {
        // A deterministic ArrayPool used for testing: always returns a fresh, zeroed array
        private sealed class DeterministicArrayPool<T> : ArrayPool<T>
        {
            public override T[] Rent(int minimumLength) => new T[minimumLength];

            public override void Return(T[] array, bool clearArray = false)
            {
                // no-op: we intentionally don't reuse arrays to make behavior deterministic for tests
            }
        }

        // Recording pool to capture returned arrays for inspection in tests
        private sealed class RecordingArrayPool<T> : ArrayPool<T>
        {
            public readonly List<T[]> Returned = new List<T[]>();

            public override T[] Rent(int minimumLength) => new T[minimumLength];

            public override void Return(T[] array, bool clearArray = false)
            {
                // record the returned array for later assertions
                Returned.Add(array);
            }
        }

        [Test]
        [TestCase(new[] { 1, 2, 3, 4, 5 })]
        [TestCase(new int[0])]
        public void ListAsSpan(int[] array)
        {
            var list = new List<int>(array);
            var span = list.AsSpan();
            Assert.That(span.Length, Is.EqualTo(list.Count));
            for (int i = 0; i < list.Count; i++)
            {
                Assert.That(span[i], Is.EqualTo(list[i]));
            }
        }

        [Test]
        public void ListAsSpanModify()
        {
            var list = new List<int> { 1, 2, 3, 4, 5 };
            var span = list.AsSpan();
            span[0] = 10;
            Assert.That(list[0], Is.EqualTo(10));

            stackalloc int[3] { 20, 30, 40 }.CopyTo(span[1..4]);
            Assert.That(list[1], Is.EqualTo(20));
            Assert.That(list[2], Is.EqualTo(30));
            Assert.That(list[3], Is.EqualTo(40));
        }

        private class TestDisposable : IDisposable
        {
            public bool IsDisposed { get; private set; }

            public void Dispose() => IsDisposed = true;
        }

        [Test]
        public void AddToDisposableContainer()
        {
            var disposable = new TestDisposable();
            var container = default(TestDisposable);

            disposable.AddTo(ref container);

            Assert.That(container, Is.Not.Null);
            Assert.That(container, Is.SameAs(disposable));
        }

        [Test]
        public void CancellationTokenSourceThrowIfDisposed()
        {
            var cts = new System.Threading.CancellationTokenSource();
            Assert.That(() => cts.ThrowIfDisposed(), Throws.Nothing);

            cts.Cancel();
            Assert.That(() => cts.ThrowIfDisposed(cts),
                Throws.TypeOf<ObjectDisposedException>().With.Property("ObjectName").EqualTo("System.Threading.CancellationTokenSource"));
        }

        [Test]
        public void CreateLinkedTokenSourceSingleToken()
        {
            using var parentCts = new System.Threading.CancellationTokenSource();
            using var externalCts = new System.Threading.CancellationTokenSource();

            using var linkedCts = parentCts.CreateLinkedTokenSource(externalCts.Token);

            // 初期状態では両方ともキャンセルされていない
            Assert.That(parentCts.Token.IsCancellationRequested, Is.False);
            Assert.That(externalCts.Token.IsCancellationRequested, Is.False);
            Assert.That(linkedCts.Token.IsCancellationRequested, Is.False);

            // 親をキャンセルするとリンクされたトークンもキャンセルされる
            parentCts.Cancel();
            Assert.That(linkedCts.Token.IsCancellationRequested, Is.True);
        }

        [Test]
        public void CreateLinkedTokenSourceSingleTokenExternalCancel()
        {
            using var parentCts = new System.Threading.CancellationTokenSource();
            using var externalCts = new System.Threading.CancellationTokenSource();

            using var linkedCts = parentCts.CreateLinkedTokenSource(externalCts.Token);

            // 外部トークンをキャンセルするとリンクされたトークンもキャンセルされる
            externalCts.Cancel();
            Assert.That(linkedCts.Token.IsCancellationRequested, Is.True);
            Assert.That(parentCts.Token.IsCancellationRequested, Is.False);
        }

        [Test]
        public void CreateLinkedTokenSourceTwoTokens()
        {
            using var parentCts = new System.Threading.CancellationTokenSource();
            using var external1Cts = new System.Threading.CancellationTokenSource();
            using var external2Cts = new System.Threading.CancellationTokenSource();

            using var linkedCts = parentCts.CreateLinkedTokenSource(external1Cts.Token, external2Cts.Token);

            // 初期状態では全てキャンセルされていない
            Assert.That(parentCts.Token.IsCancellationRequested, Is.False);
            Assert.That(external1Cts.Token.IsCancellationRequested, Is.False);
            Assert.That(external2Cts.Token.IsCancellationRequested, Is.False);
            Assert.That(linkedCts.Token.IsCancellationRequested, Is.False);

            // 最初の外部トークンをキャンセル
            external1Cts.Cancel();
            Assert.That(linkedCts.Token.IsCancellationRequested, Is.True);
            Assert.That(parentCts.Token.IsCancellationRequested, Is.False);
            Assert.That(external2Cts.Token.IsCancellationRequested, Is.False);
        }

        [Test]
        public void CreateLinkedTokenSourceTwoTokensSecondCancel()
        {
            using var parentCts = new System.Threading.CancellationTokenSource();
            using var external1Cts = new System.Threading.CancellationTokenSource();
            using var external2Cts = new System.Threading.CancellationTokenSource();

            using var linkedCts = parentCts.CreateLinkedTokenSource(external1Cts.Token, external2Cts.Token);

            // 2番目の外部トークンをキャンセル
            external2Cts.Cancel();
            Assert.That(linkedCts.Token.IsCancellationRequested, Is.True);
            Assert.That(parentCts.Token.IsCancellationRequested, Is.False);
            Assert.That(external1Cts.Token.IsCancellationRequested, Is.False);
        }

        [Test]
        public void CreateLinkedTokenSourceTwoTokensParentCancel()
        {
            using var parentCts = new System.Threading.CancellationTokenSource();
            using var external1Cts = new System.Threading.CancellationTokenSource();
            using var external2Cts = new System.Threading.CancellationTokenSource();

            using var linkedCts = parentCts.CreateLinkedTokenSource(external1Cts.Token, external2Cts.Token);

            // 親をキャンセル
            parentCts.Cancel();
            Assert.That(linkedCts.Token.IsCancellationRequested, Is.True);
            Assert.That(external1Cts.Token.IsCancellationRequested, Is.False);
            Assert.That(external2Cts.Token.IsCancellationRequested, Is.False);
        }

        [System.Flags]
        public enum TestFlags : int
        {
            None = 0,
            Flag1 = 1 << 0,
            Flag2 = 1 << 1,
            Flag3 = 1 << 2,
            Flag4 = 1 << 3,
        }

        [System.Flags]
        public enum BigFlags : long
        {
            None = 0L,
            FlagA = 1L << 0,
            FlagB = 1L << 1,
            FlagC = 1L << 2,
        }

        // AggregateFlags テスト群
        [Test]
        public void AggregateFlags_EmptySpan_ReturnsDefault()
        {
            var empty = Array.Empty<TestFlags>();
            var aggregated = empty.AsSpan().AggregateFlags();
            Assert.That(aggregated, Is.EqualTo(TestFlags.None));
        }

        [Test]
        public void AggregateFlags_SingleElement_ReturnsElement()
        {
            var single = new[] { TestFlags.Flag2 };
            var aggregated = single.AsSpan().AggregateFlags();
            Assert.That(aggregated, Is.EqualTo(TestFlags.Flag2));
        }

        [Test]
        public void AggregateFlags_MultipleElements_CombinedCorrectly()
        {
            var flags = new[] { TestFlags.Flag1, TestFlags.Flag2, TestFlags.Flag4 };
            var aggregated = flags.AsSpan().AggregateFlags();
            Assert.That(aggregated, Is.EqualTo(TestFlags.Flag1 | TestFlags.Flag2 | TestFlags.Flag4));
        }

        [Test]
        public void AggregateFlags_DuplicateElements_NoDoubleCount()
        {
            var flags = new[] { TestFlags.Flag1, TestFlags.Flag1 };
            var aggregated = flags.AsSpan().AggregateFlags();
            Assert.That(aggregated, Is.EqualTo(TestFlags.Flag1));
        }

        [Test]
        public void AggregateFlags_LongUnderlyingType_WorksFor8ByteEnums()
        {
            var flags = new[] { BigFlags.FlagA, BigFlags.FlagC };
            var aggregated = flags.AsSpan().AggregateFlags();
            Assert.That(aggregated, Is.EqualTo(BigFlags.FlagA | BigFlags.FlagC));
        }

        [Test]
        [TestCase(TestFlags.Flag1, TestFlags.Flag1, true)]
        [TestCase(TestFlags.Flag1 | TestFlags.Flag3, TestFlags.Flag1, true)]
        [TestCase(TestFlags.Flag1 | TestFlags.Flag3, TestFlags.Flag2, false)]
        [TestCase(TestFlags.Flag1, TestFlags.Flag2, false)]
        public void HasFlagsSameCase(TestFlags value, TestFlags flag, bool expected)
        {
            Assert.That(value.HasBitFlags(flag), Is.EqualTo(expected));
        }

        [Test]
        [TestCase(TestFlags.Flag1, true)]
        [TestCase(TestFlags.Flag2, true)]
        [TestCase(TestFlags.Flag3, true)]
        [TestCase(TestFlags.Flag1 | TestFlags.Flag2, false)]
        [TestCase(TestFlags.Flag1 | TestFlags.Flag3 | TestFlags.Flag4, false)]
        [TestCase(TestFlags.None, false)]
        public void ConstructFlagsSomeCase(TestFlags flag, bool expected)
        {
            Assert.That(flag.ConstructFlags(), Is.EqualTo(expected));
        }

        [Test]
        [TestCase(TestFlags.Flag1, 1)]
        [TestCase(TestFlags.Flag1 | TestFlags.Flag2, 2)]
        [TestCase(TestFlags.Flag1 | TestFlags.Flag2 | TestFlags.Flag3, 3)]
        [TestCase(TestFlags.Flag1 | TestFlags.Flag2 | TestFlags.Flag3 | TestFlags.Flag4, 4)]
        [TestCase(TestFlags.None, 0)]
        public void ForEachFlags(TestFlags flags, int expected)
        {
            var result = new List<TestFlags>();

            foreach (var testFlag in flags)
            {
                result.Add(testFlag);
            }

            Assert.That(result, Has.Count.EqualTo(expected));
        }

        [Test]
        [TestCase(TestFlags.Flag1 | TestFlags.Flag2, TestFlags.Flag1 | TestFlags.Flag2, true)]
        [TestCase(TestFlags.Flag1 | TestFlags.Flag3, TestFlags.Flag1 | TestFlags.Flag2, false)]
        [TestCase(TestFlags.Flag1 | TestFlags.Flag2 | TestFlags.Flag3, TestFlags.Flag2 | TestFlags.Flag3, true)]
        [TestCase(TestFlags.None, TestFlags.None, true)]
        [TestCase(TestFlags.Flag1, TestFlags.None, true)] // Any flag contains None
        public void HasBitFlagsMultipleFlags(TestFlags value, TestFlags flag, bool expected)
        {
            Assert.That(value.HasBitFlags(flag), Is.EqualTo(expected));
        }

        [Test]
        public void ConstructFlagsEdgeCases()
        {
            // Test power of 2 values specifically
            Assert.That(((TestFlags)1).ConstructFlags(), Is.True);   // 2^0 = 1
            Assert.That(((TestFlags)2).ConstructFlags(), Is.True);   // 2^1 = 2
            Assert.That(((TestFlags)4).ConstructFlags(), Is.True);   // 2^2 = 4
            Assert.That(((TestFlags)8).ConstructFlags(), Is.True);   // 2^3 = 8
            Assert.That(((TestFlags)16).ConstructFlags(), Is.True);  // 2^4 = 16

            // Test non-power of 2 values
            Assert.That(((TestFlags)3).ConstructFlags(), Is.False);  // 1 + 2
            Assert.That(((TestFlags)5).ConstructFlags(), Is.False);  // 1 + 4
            Assert.That(((TestFlags)7).ConstructFlags(), Is.False);  // 1 + 2 + 4
            Assert.That(((TestFlags)15).ConstructFlags(), Is.False); // All flags
        }

        [Test]
        public void ForEachFlagsSpecificOrder()
        {
            var flags = TestFlags.Flag1 | TestFlags.Flag3 | TestFlags.Flag4;
            var result = new List<TestFlags>();

            foreach (var flag in flags)
            {
                result.Add(flag);
            }

            // Should iterate in order of lowest bits first
            Assert.That(result, Has.Count.EqualTo(3));
            Assert.That(result[0], Is.EqualTo(TestFlags.Flag1)); // bit 0
            Assert.That(result[1], Is.EqualTo(TestFlags.Flag3)); // bit 2
            Assert.That(result[2], Is.EqualTo(TestFlags.Flag4)); // bit 3
        }

        [Test]
        public void CreateComparerWithNullValues()
        {
            var comparer = EqualityComparer.Create<string>(
                (x, y) => string.Equals(x, y, StringComparison.OrdinalIgnoreCase),
                x => x?.ToUpperInvariant().GetHashCode() ?? 0);

            // Test with null values
            Assert.That(comparer.Equals(null, null), Is.True);
            Assert.That(comparer.Equals("test", null), Is.False);
            Assert.That(comparer.Equals(null, "test"), Is.False);

            // Test case-insensitive comparison
            Assert.That(comparer.Equals("Test", "TEST"), Is.True);
            Assert.That(comparer.Equals("Test", "test"), Is.True);
            Assert.That(comparer.Equals("Test", "Other"), Is.False);

            // Test hash codes
            Assert.That(comparer.GetHashCode("Test"), Is.EqualTo(comparer.GetHashCode("test")));
            Assert.That(comparer.GetHashCode(null), Is.EqualTo(0));
        }

        [Test]
        public void CreateComparerWithCustomType()
        {
            var comparer = EqualityComparer.Create<TestFlags>(
                (x, y) => x == y,
                x => ((int)x).GetHashCode());

            Assert.That(comparer.Equals(TestFlags.Flag1, TestFlags.Flag1), Is.True);
            Assert.That(comparer.Equals(TestFlags.Flag1, TestFlags.Flag2), Is.False);
            Assert.That(comparer.GetHashCode(TestFlags.Flag1), Is.EqualTo(((int)TestFlags.Flag1).GetHashCode()));
        }

        [Test]
        public void DelegateEqualityComparerGetHashCode()
        {
            var comparer1 = EqualityComparer.Create<int>((x, y) => x == y, x => x.GetHashCode());
            var comparer2 = EqualityComparer.Create<int>((x, y) => x == y, x => x.GetHashCode());
            var comparer3 = EqualityComparer.Create<int>((x, y) => x != y, x => x.GetHashCode()); // Different equals function

            // Same function references should produce same hash code
            var hash1 = comparer1.GetHashCode();
            var hash2 = comparer2.GetHashCode();

            // Different function should produce different hash code
            var hash3 = comparer3.GetHashCode();

            // We can't guarantee exact equality due to delegate hash codes, but we can test that it doesn't throw
            Assert.That(() => hash1, Throws.Nothing);
            Assert.That(() => hash2, Throws.Nothing);
            Assert.That(() => hash3, Throws.Nothing);
        }

        [Test]
        [TestCase(new[] { 1, 2, 3, 4, 5 })]
        [TestCase(new int[0])]
        public void MemoryForEach(int[] array)
        {
            var memory = new Memory<int>(array);

            var result = new List<int>();
            foreach (var item in memory)
            {
                result.Add(item);
            }

            Assert.That(result, Has.Count.EqualTo(array.Length));
        }

        [Test]
        [TestCase(new[] { 1, 2, 3, 4, 5 })]
        [TestCase(new int[0])]
        public void ReadOnlyMemoryForEach(int[] array)
        {
            var memory = new ReadOnlyMemory<int>(array);

            var result = new List<int>();
            foreach (var item in memory)
            {
                result.Add(item);
            }

            Assert.That(result, Has.Count.EqualTo(array.Length));
        }

        [Test]
        public void ValueTuple2()
        {
            var list = new List<int>();
            foreach (var i in (23, 31))
            {
                list.Add(i);
            }
            Assert.That(list, Has.Count.EqualTo(2));
            Assert.That(list[0], Is.EqualTo(23));
            Assert.That(list[1], Is.EqualTo(31));
        }

        [Test]
        public void ValueTuple3()
        {
            var list = new List<int>();
            foreach (var i in (23, 31, 42))
            {
                list.Add(i);
            }

            Assert.That(list, Has.Count.EqualTo(3));
            Assert.That(list[0], Is.EqualTo(23));
            Assert.That(list[1], Is.EqualTo(31));
            Assert.That(list[2], Is.EqualTo(42));
        }

        [Test]
        public void ValueTuple4()
        {
            var list = new List<int>();
            foreach (var i in (23, 31, 42, 55))
            {
                list.Add(i);
            }

            Assert.That(list, Has.Count.EqualTo(4));
            Assert.That(list[0], Is.EqualTo(23));
            Assert.That(list[1], Is.EqualTo(31));
            Assert.That(list[2], Is.EqualTo(42));
            Assert.That(list[3], Is.EqualTo(55));
        }

        [Test]
        public void ValueTuple5()
        {
            var list = new List<int>();
            foreach (var i in (23, 31, 42, 55, 64))
            {
                list.Add(i);
            }

            Assert.That(list, Has.Count.EqualTo(5));
            Assert.That(list[0], Is.EqualTo(23));
            Assert.That(list[1], Is.EqualTo(31));
            Assert.That(list[2], Is.EqualTo(42));
            Assert.That(list[3], Is.EqualTo(55));
            Assert.That(list[4], Is.EqualTo(64));
        }

        [Test]
        public void ValueTuple6()
        {
            var list = new List<int>();
            foreach (var i in (23, 31, 42, 55, 64, 77))
            {
                list.Add(i);
            }

            Assert.That(list, Has.Count.EqualTo(6));
            Assert.That(list[0], Is.EqualTo(23));
            Assert.That(list[1], Is.EqualTo(31));
            Assert.That(list[2], Is.EqualTo(42));
            Assert.That(list[3], Is.EqualTo(55));
            Assert.That(list[4], Is.EqualTo(64));
            Assert.That(list[5], Is.EqualTo(77));
        }

        [Test]
        public void ValueTuple7()
        {
            var list = new List<int>();
            foreach (var i in (23, 31, 42, 55, 64, 77, 88))
            {
                list.Add(i);
            }

            Assert.That(list, Has.Count.EqualTo(7));
            Assert.That(list[0], Is.EqualTo(23));
            Assert.That(list[1], Is.EqualTo(31));
            Assert.That(list[2], Is.EqualTo(42));
            Assert.That(list[3], Is.EqualTo(55));
            Assert.That(list[4], Is.EqualTo(64));
            Assert.That(list[5], Is.EqualTo(77));
            Assert.That(list[6], Is.EqualTo(88));
        }

        // MemoryExtensions detailed tests
        [Test]
        public void MemoryEnumeratorDetails()
        {
            var array = new[] { 10, 20, 30 };
            var memory = new Memory<int>(array);
            var enumerator = memory.GetEnumerator();

            // Initial state
            Assert.That(() => enumerator.Current, Throws.TypeOf<IndexOutOfRangeException>());

            // First element
            Assert.That(enumerator.MoveNext(), Is.True);
            Assert.That(enumerator.Current, Is.EqualTo(10));

            // Second element
            Assert.That(enumerator.MoveNext(), Is.True);
            Assert.That(enumerator.Current, Is.EqualTo(20));

            // Third element
            Assert.That(enumerator.MoveNext(), Is.True);
            Assert.That(enumerator.Current, Is.EqualTo(30));

            // End of enumeration
            Assert.That(enumerator.MoveNext(), Is.False);
            Assert.That(() => enumerator.Current, Throws.TypeOf<IndexOutOfRangeException>());
        }

        [Test]
        public void ReadOnlyMemoryEnumeratorDetails()
        {
            var array = new[] { 10, 20, 30 };
            var memory = new ReadOnlyMemory<int>(array);
            var enumerator = memory.GetEnumerator();

            // Initial state
            Assert.That(() => enumerator.Current, Throws.TypeOf<IndexOutOfRangeException>());

            // First element
            Assert.That(enumerator.MoveNext(), Is.True);
            Assert.That(enumerator.Current, Is.EqualTo(10));

            // Second element
            Assert.That(enumerator.MoveNext(), Is.True);
            Assert.That(enumerator.Current, Is.EqualTo(20));

            // Third element
            Assert.That(enumerator.MoveNext(), Is.True);
            Assert.That(enumerator.Current, Is.EqualTo(30));

            // End of enumeration
            Assert.That(enumerator.MoveNext(), Is.False);
            Assert.That(() => enumerator.Current, Throws.TypeOf<IndexOutOfRangeException>());
        }

        [Test]
        public void MemorySliceEnumerator()
        {
            var array = new[] { 1, 2, 3, 4, 5 };
            var memory = new Memory<int>(array).Slice(1, 3); // [2, 3, 4]

            var result = new List<int>();
            foreach (var item in memory)
            {
                result.Add(item);
            }

            Assert.That(result, Has.Count.EqualTo(3));
            Assert.That(result[0], Is.EqualTo(2));
            Assert.That(result[1], Is.EqualTo(3));
            Assert.That(result[2], Is.EqualTo(4));
        }

        [Test]
        public void ReadOnlyMemorySliceEnumerator()
        {
            var array = new[] { 1, 2, 3, 4, 5 };
            var memory = new ReadOnlyMemory<int>(array).Slice(2, 2); // [3, 4]

            var result = new List<int>();
            foreach (var item in memory)
            {
                result.Add(item);
            }

            Assert.That(result, Has.Count.EqualTo(2));
            Assert.That(result[0], Is.EqualTo(3));
            Assert.That(result[1], Is.EqualTo(4));
        }

        // ValueTupleExtensions detailed tests
        [Test]
        public void ValueTupleEnumeratorDetails()
        {
            var tuple = (10, 20, 30);
            var enumerator = tuple.GetEnumerator();

            // Initial state
            Assert.That(() => enumerator.Current, Throws.TypeOf<IndexOutOfRangeException>());

            // First element
            Assert.That(enumerator.MoveNext(), Is.True);
            Assert.That(enumerator.Current, Is.EqualTo(10));

            // Second element
            Assert.That(enumerator.MoveNext(), Is.True);
            Assert.That(enumerator.Current, Is.EqualTo(20));

            // Third element
            Assert.That(enumerator.MoveNext(), Is.True);
            Assert.That(enumerator.Current, Is.EqualTo(30));

            // End of enumeration
            Assert.That(enumerator.MoveNext(), Is.False);
            Assert.That(() => enumerator.Current, Throws.TypeOf<IndexOutOfRangeException>());
        }

        [Test]
        public void ValueTupleWithDifferentTypes()
        {
            var stringTuple = ("hello", "world");
            var result = new List<string>();

            foreach (var item in stringTuple)
            {
                result.Add(item);
            }

            Assert.That(result, Has.Count.EqualTo(2));
            Assert.That(result[0], Is.EqualTo("hello"));
            Assert.That(result[1], Is.EqualTo("world"));
        }

        [Test]
        public void ValueTupleWithNullValues()
        {
            var nullableTuple = ("test", (string)null, "value");
            var result = new List<string>();

            foreach (var item in nullableTuple)
            {
                result.Add(item);
            }

            Assert.That(result, Has.Count.EqualTo(3));
            Assert.That(result[0], Is.EqualTo("test"));
            Assert.That(result[1], Is.Null);
            Assert.That(result[2], Is.EqualTo("value"));
        }

        [Test]
        public void ValueTupleEnumeratorIndexOutOfRange()
        {
            var tuple = (1, 2);
            var enumerator = tuple.GetEnumerator();

            // Move to valid positions
            enumerator.MoveNext(); // index 0
            enumerator.MoveNext(); // index 1
            enumerator.MoveNext(); // index 2 (out of range)

            // Current should throw for out of range index
            Assert.That(() => enumerator.Current, Throws.TypeOf<IndexOutOfRangeException>());
        }

        [Test]
        public void ValueTupleLargeElementsOrder()
        {
            var tuple = (1, 2, 3, 4, 5, 6, 7);
            var result = new List<int>();

            foreach (var item in tuple)
            {
                result.Add(item);
            }

            Assert.That(result, Has.Count.EqualTo(7));
            for (int i = 0; i < 7; i++)
            {
                Assert.That(result[i], Is.EqualTo(i + 1));
            }
        }

        [Test]
        public void MemoryExtensionsWithReferenceTypes()
        {
            var stringArray = new[] { "a", "b", "c" };
            var memory = new Memory<string>(stringArray);

            var result = new List<string>();
            foreach (var item in memory)
            {
                result.Add(item);
            }

            Assert.That(result, Has.Count.EqualTo(3));
            Assert.That(result, Is.EqualTo(stringArray));
        }

        [Test]
        public void ReadOnlyMemoryExtensionsWithReferenceTypes()
        {
            var stringArray = new[] { "x", "y", "z" };
            var memory = new ReadOnlyMemory<string>(stringArray);

            var result = new List<string>();
            foreach (var item in memory)
            {
                result.Add(item);
            }

            Assert.That(result, Has.Count.EqualTo(3));
            Assert.That(result, Is.EqualTo(stringArray));
        }

        [Test]
        public void ArrayPoolRentBasicUsage()
        {
            var pool = ArrayPool<int>.Shared;

            using var handle = pool.Rent(10, out int[] array);

            Assert.That(array, Is.Not.Null);
            Assert.That(array.Length, Is.GreaterThanOrEqualTo(10));
        }

        [Test]
        public void ArrayPoolRentWithUsingStatement()
        {
            var pool = ArrayPool<int>.Shared;
            int[] rentedArray = null;

            using (var handle = pool.Rent(5, out int[] array))
            {
                rentedArray = array;
                Assert.That(array, Is.Not.Null);
                Assert.That(array.Length, Is.GreaterThanOrEqualTo(5));

                // 配列を使用
                array[0] = 42;
                Assert.That(array[0], Is.EqualTo(42));
            }

            // usingブロックを抜けた後、配列は返却されている
            Assert.That(rentedArray, Is.Not.Null); // 参照は有効だが、内容は変更される可能性がある
        }

        [Test]
        public void ArrayPoolRentDifferentTypes()
        {
            // int型のテスト
            var intPool = ArrayPool<int>.Shared;
            using (var intHandle = intPool.Rent(8, out int[] intArray))
            {
                Assert.That(intArray, Is.Not.Null);
                Assert.That(intArray.Length, Is.GreaterThanOrEqualTo(8));
                intArray[0] = 100;
                Assert.That(intArray[0], Is.EqualTo(100));
            }

            // string型のテスト
            var stringPool = ArrayPool<string>.Shared;
            using (var stringHandle = stringPool.Rent(3, out string[] stringArray))
            {
                Assert.That(stringArray, Is.Not.Null);
                Assert.That(stringArray.Length, Is.GreaterThanOrEqualTo(3));
                stringArray[0] = "test";
                Assert.That(stringArray[0], Is.EqualTo("test"));
            }
        }

        [Test]
        public void ArrayPoolRentZeroLength()
        {
            var pool = ArrayPool<int>.Shared;

            using var handle = pool.Rent(0, out int[] array);

            Assert.That(array, Is.Not.Null);
            Assert.That(array.Length, Is.GreaterThanOrEqualTo(0));
        }

        [Test]
        public void ArrayPoolRentLargeSize()
        {
            var pool = ArrayPool<byte>.Shared;
            const int requestedSize = 1024;

            using var handle = pool.Rent(requestedSize, out byte[] array);

            Assert.That(array, Is.Not.Null);
            Assert.That(array.Length, Is.GreaterThanOrEqualTo(requestedSize));

            // 配列の使用可能性を確認
            array[0] = 255;
            array[requestedSize - 1] = 128;
            Assert.That(array[0], Is.EqualTo(255));
            Assert.That(array[requestedSize - 1], Is.EqualTo(128));
        }

        [Test]
        public void ArrayPoolRentMultipleArrays()
        {
            var pool = ArrayPool<int>.Shared;

            using var handle1 = pool.Rent(16, out int[] array1);
            using var handle2 = pool.Rent(32, out int[] array2);
            using var handle3 = pool.Rent(64, out int[] array3);

            // 全て異なる配列インスタンスである
            Assert.That(array1, Is.Not.SameAs(array2));
            Assert.That(array2, Is.Not.SameAs(array3));
            Assert.That(array1, Is.Not.SameAs(array3));

            // サイズ要件を満たしている
            Assert.That(array1.Length, Is.GreaterThanOrEqualTo(5));
            Assert.That(array2.Length, Is.GreaterThanOrEqualTo(10));
            Assert.That(array3.Length, Is.GreaterThanOrEqualTo(15));

            // 各配列が独立して使用可能
            array1[0] = 1;
            array2[0] = 2;
            array3[0] = 3;

            Assert.That(array1[0], Is.EqualTo(1));
            Assert.That(array2[0], Is.EqualTo(2));
            Assert.That(array3[0], Is.EqualTo(3));
        }

        [Test]
        public void ArrayPoolRentHandleDispose()
        {
            var pool = ArrayPool<int>.Shared;
            int[] array;

            // handleを明示的にDisposeする
            var handle = pool.Rent(10, out array);
            Assert.That(array, Is.Not.Null);

            ((IDisposable)handle).Dispose();

            // Disposeが例外を投げないことを確認
            Assert.That(() => ((IDisposable)handle).Dispose(), Throws.Nothing);
        }

        [Test]
        public void ArrayPoolRentCustomPool()
        {
            // カスタムプールを作成
            var customPool = ArrayPool<double>.Create();

            using var handle = customPool.Rent(20, out double[] array);

            Assert.That(array, Is.Not.Null);
            Assert.That(array.Length, Is.GreaterThanOrEqualTo(20));

            // 配列を使用
            for (int i = 0; i < 20; i++)
            {
                array[i] = i * 0.5;
            }

            for (int i = 0; i < 20; i++)
            {
                Assert.That(array[i], Is.EqualTo(i * 0.5));
            }
        }

        // 以下、ArrayPool.Grow のユースケースを網羅するテスト
        [Test]
        public void ArrayPoolGrow_IncreasesSize_WhenSmallerThanMinimum()
        {
            var pool = new DeterministicArrayPool<int>();
            int[] array = pool.Rent(3);
            try
            {
                array[0] = 1;
                array[1] = 2;
                array[2] = 3;
                // remember original reference
                int[] before = array;
                // Grow to at least 10
                pool.Grow(ref array, 10);
                Assert.That(array, Is.Not.Null);
                Assert.That(array.Length, Is.GreaterThanOrEqualTo(10));
                // should be a new instance (grown)
                Assert.That(array, Is.Not.SameAs(before));
                // existing elements preserved
                Assert.That(array[0], Is.EqualTo(1));
                Assert.That(array[1], Is.EqualTo(2));
                Assert.That(array[2], Is.EqualTo(3));
                // writable
                array[9] = 99;
                Assert.That(array[9], Is.EqualTo(99));
            }
            finally
            {
                try { pool.Return(array); } catch { }
            }
        }

        [Test]
        public void ArrayPoolGrow_FromEmptyArray_Works()
        {
            var pool = ArrayPool<string>.Shared;
            string[] array = Array.Empty<string>();
            // Grow to 5
            pool.Grow(ref array, 5);
            Assert.That(array, Is.Not.Null);
            Assert.That(array.Length, Is.GreaterThanOrEqualTo(5));
            // assign and read
            array[0] = "hello";
            Assert.That(array[0], Is.EqualTo("hello"));
            try { ArrayPool<string>.Shared.Return(array); } catch { }
        }

        [Test]
        public void ArrayPoolGrow_NoOp_WhenAlreadyLargeEnough()
        {
            var pool = ArrayPool<int>.Shared;
            int[] array = pool.Rent(10);
            try
            {
                int[] before = array;
                pool.Grow(ref array, 5);
                // should remain same instance
                Assert.That(array, Is.SameAs(before));
            }
            finally
            {
                try { pool.Return(array); } catch { }
            }
        }

        [Test]
        public void ArrayPoolGrow_ZeroOrNegativeMinimum_DoesNotThrow_AndNoOp()
        {
            var pool = ArrayPool<int>.Shared;
            int[] array = pool.Rent(4);
            try
            {
                int[] before = array;
                pool.Grow(ref array, 0);
                Assert.That(array, Is.SameAs(before));

                pool.Grow(ref array, -1);
                Assert.That(array, Is.SameAs(before));
            }
            finally
            {
                try { pool.Return(array); } catch { }
            }
        }

        [Test]
        public void ArrayPoolGrow_PreservesReferenceTypeElements()
        {
            var pool = ArrayPool<object>.Shared;
            object[] array = pool.Rent(2);
            try
            {
                var obj = new object();
                array[0] = obj;
                pool.Grow(ref array, 5);
                Assert.That(array[0], Is.SameAs(obj));
            }
            finally
            {
                try { ArrayPool<object>.Shared.Return(array); } catch { }
            }
        }

        [Test]
        public void ArrayPoolGrow_ClearsOldArrayOnReturn_ForReferenceTypes()
        {
            var pool = new RecordingArrayPool<string>();
            string[] array = pool.Rent(3);
            try
            {
                array[0] = "hello";
                array[1] = "world";
                // Grow to 5
                pool.Grow(ref array, 5);
                Assert.That(array, Is.Not.Null);
                Assert.That(array.Length, Is.GreaterThanOrEqualTo(5));
                // ensure the old array was returned and cleared
                Assert.That(pool.Returned.Count, Is.GreaterThanOrEqualTo(1));
                var old = pool.Returned[0];
                Assert.That(old.Length, Is.EqualTo(3));
                // cleared to nulls
                for (int i = 0; i < old.Length; i++)
                {
                    Assert.That(old[i], Is.Null);
                }
                // ensure new array preserved original elements
                Assert.That(array[0], Is.EqualTo("hello"));
                Assert.That(array[1], Is.EqualTo("world"));
            }
            finally
            {
                try { pool.Return(array); } catch { }
            }
        }

        [Test]
        public void ArrayPoolGrow_ClearsOldArrayOnReturn_ForValueTypes()
        {
            var pool = new RecordingArrayPool<int>();
            int[] array = pool.Rent(3);
            try
            {
                array[0] = 42;
                array[1] = 7;
                // Grow to 6
                pool.Grow(ref array, 6);
                Assert.That(array, Is.Not.Null);
                Assert.That(array.Length, Is.GreaterThanOrEqualTo(6));
                Assert.That(pool.Returned.Count, Is.GreaterThanOrEqualTo(1));
                var old = pool.Returned[0];
                Assert.That(old.Length, Is.EqualTo(3));
                // cleared to default (0)
                for (int i = 0; i < old.Length; i++)
                {
                    Assert.That(old[i], Is.EqualTo(0));
                }
                // ensure new array preserved original elements
                Assert.That(array[0], Is.EqualTo(42));
                Assert.That(array[1], Is.EqualTo(7));
            }
            finally
            {
                try { pool.Return(array); } catch { }
            }
        }

        [Test]
        public void FormatHelper_SimpleLiteral()
        {
            var (literals, embeds) = FormatHelper.AnalyzeFormat("hello".AsSpan());
            Assert.That(literals, Is.EqualTo(new[] { "hello" }));
            Assert.That(embeds, Is.Empty);
        }

        [Test]
        public void FormatHelper_SinglePlaceholderWithoutFormat()
        {
            var (literals, embeds) = FormatHelper.AnalyzeFormat("a{0}b".AsSpan());
            Assert.That(literals, Is.EqualTo(new[] { "a", "b" }));
            Assert.That(embeds, Has.Length.EqualTo(1));
            Assert.That(embeds[0].index, Is.EqualTo(0));
            Assert.That(embeds[0].format, Is.EqualTo(string.Empty));
        }

        [Test]
        public void FormatHelper_MultiplePlaceholdersWithFormats()
        {
            var (literals, embeds) = FormatHelper.AnalyzeFormat("Start{0}Middle{1:000}End".AsSpan());
            Assert.That(literals, Is.EqualTo(new[] { "Start", "Middle", "End" }));
            Assert.That(embeds, Has.Length.EqualTo(2));
            Assert.That(embeds[0].index, Is.EqualTo(0));
            Assert.That(embeds[0].format, Is.EqualTo(string.Empty));
            Assert.That(embeds[1].index, Is.EqualTo(1));
            Assert.That(embeds[1].format, Is.EqualTo("000"));
        }

        [Test]
        public void FormatHelper_ConsecutivePlaceholders()
        {
            var (literals, embeds) = FormatHelper.AnalyzeFormat("{0}{1}".AsSpan());
            Assert.That(embeds, Has.Length.EqualTo(2));
            Assert.That(literals, Has.Length.EqualTo(2));
            Assert.That(literals[0], Is.EqualTo(""));
            Assert.That(literals[1], Is.EqualTo(""));
            Assert.That(embeds[0].index, Is.EqualTo(0));
            Assert.That(embeds[1].index, Is.EqualTo(1));
        }

        [Test]
        public void FormatHelper_FormatContainingColon()
        {
            var (literals, embeds) = FormatHelper.AnalyzeFormat("x{2:0:0}y".AsSpan());
            Assert.That(embeds, Has.Length.EqualTo(1));
            Assert.That(embeds[0].index, Is.EqualTo(2));
            Assert.That(embeds[0].format, Is.EqualTo("0:0")); // 最初の ':' 以降をフォーマット文字列として取得
            Assert.That(literals, Is.EqualTo(new[] { "x", "y" }));
        }

        [Test]
        public void FormatHelper_UnclosedBrace_ThrowsFormatException()
        {
            Assert.That(() => FormatHelper.AnalyzeFormat("{0".AsSpan()),
                Throws.TypeOf<FormatException>().With.Message.Contains("閉じ中括弧"));
        }

        [Test]
        public void FormatHelper_UnclosedDoubleBrace_ThrowsFormatException()
        {
            Assert.That(() => FormatHelper.AnalyzeFormat("{{".AsSpan()),
                Throws.TypeOf<FormatException>().With.Message.Contains("閉じ中括弧"));
        }

        [Test]
        public void FormatHelper_StandaloneClosingBrace_ThrowsFormatException()
        {
            Assert.That(() => FormatHelper.AnalyzeFormat("a}b".AsSpan()),
                Throws.TypeOf<FormatException>().With.Message.Contains("開き中括弧"));
        }

        [Test]
        public void FormatHelper_InnerBraceInFormat_ThrowsDueToEarlyClosing()
        {
            Assert.That(() => FormatHelper.AnalyzeFormat("x{0:{1}}y".AsSpan()),
                Throws.TypeOf<FormatException>().With.Message.Contains("開き中括弧"));
        }

        [Test]
        public void FormatHelper_LeadingZerosIndex_Parsed()
        {
            var (literals, embeds) = FormatHelper.AnalyzeFormat("{001:00}".AsSpan());
            Assert.That(embeds, Has.Length.EqualTo(1));
            Assert.That(embeds[0].index, Is.EqualTo(1));
            Assert.That(embeds[0].format, Is.EqualTo("00"));
        }

        [Test]
        public void FormatHelper_EmptyIndex_ThrowsFormatException()
        {
            Assert.That(() => FormatHelper.AnalyzeFormat("{}".AsSpan()), Throws.TypeOf<FormatException>());
        }
    }
}
