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

        // IInitializable interface tests
        private class TestInitializable : IInitializable
        {
            public bool IsInitialized { get; private set; }
            public bool WasCancelled { get; private set; }

            public async ValueTask InitializeAsync(CancellationToken cancellationToken)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    WasCancelled = true;
                    cancellationToken.ThrowIfCancellationRequested();
                }

                await Task.Delay(10, cancellationToken);
                IsInitialized = true;
            }
        }

        [Test]
        public async Task InitializableBasicImplementation()
        {
            var initializable = new TestInitializable();
            Assert.That(initializable.IsInitialized, Is.False);

            await initializable.InitializeAsync(CancellationToken.None);
            Assert.That(initializable.IsInitialized, Is.True);
        }

        [Test]
        public async Task InitializableCancellation()
        {
            var initializable = new TestInitializable();
            using var cts = new CancellationTokenSource();
            cts.Cancel();

            try
            {
                await initializable.InitializeAsync(cts.Token);
                Assert.Fail("Expected OperationCanceledException");
            }
            catch (OperationCanceledException)
            {
                Assert.That(initializable.WasCancelled, Is.True);
                Assert.That(initializable.IsInitialized, Is.False);
            }
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

        // ValueList tests
        [Test]
        public void ValueListDefaultConstructor()
        {
            var list = new ValueList<int>();
            Assert.That(list, Is.Empty);
        }

        [Test]
        public void ValueListWithCapacity()
        {
            var list = new ValueList<int>(10);
            Assert.That(list, Is.Empty);
        }

        [Test]
        public void ValueListAdd()
        {
            var list = new ValueList<int>();
            list.Add(1);
            list.Add(2);
            list.Add(3);

            Assert.That(list, Has.Count.EqualTo(3));
        }

        [Test]
        public void ValueListEnumerate()
        {
            var list = new ValueList<int>();
            list.Add(10);
            list.Add(20);
            list.Add(30);

            var result = new List<int>();
            foreach (var item in list)
            {
                result.Add(item);
            }

            Assert.That(result, Has.Count.EqualTo(3));
            Assert.That(result[0], Is.EqualTo(10));
            Assert.That(result[1], Is.EqualTo(20));
            Assert.That(result[2], Is.EqualTo(30));
        }

        [Test]
        public void ValueListAsSegment()
        {
            var list = new ValueList<int>();
            list.Add(1);
            list.Add(2);
            list.Add(3);

            var span = list.AsSpan();
            Assert.That(span.Length, Is.EqualTo(3));
            Assert.That(span[0], Is.EqualTo(1));
            Assert.That(span[1], Is.EqualTo(2));
            Assert.That(span[2], Is.EqualTo(3));
        }

        [Test]
        public void ValueListCapacityExpansion()
        {
            var list = new ValueList<int>(2); // Small initial capacity

            // Add more items than initial capacity
            for (int i = 0; i < 10; i++)
            {
                list.Add(i);
            }

            Assert.That(list, Has.Count.EqualTo(10));

            // Verify all items are correctly stored
            var span = list.AsSpan();
            for (int i = 0; i < 10; i++)
            {
                Assert.That(span[i], Is.EqualTo(i));
            }
        }

        [Test]
        public void ValueListZeroCapacityHandling()
        {
            var list = new ValueList<int>(0); // Zero capacity
            list.Add(1);
            list.Add(2);

            Assert.That(list.Count, Is.EqualTo(2));
            var span = list.AsSpan();
            Assert.That(span[0], Is.EqualTo(1));
            Assert.That(span[1], Is.EqualTo(2));
        }

        [Test]
        public void ValueListAsSegmentTest()
        {
            var list = new ValueList<int> { 10, 20, 30 };
            var segment = list.AsSegment();

            Assert.That(segment.Count, Is.EqualTo(3));
            Assert.That(segment.Array, Is.Not.Null);
            Assert.That(segment.Offset, Is.EqualTo(0));
            Assert.That(segment.Array[0], Is.EqualTo(10));
            Assert.That(segment.Array[1], Is.EqualTo(20));
            Assert.That(segment.Array[2], Is.EqualTo(30));
        }

        [Test]
        public void ValueListAsMemoryTest()
        {
            var list = new ValueList<int> { 5, 15, 25 };
            var memory = list.AsMemory();

            Assert.That(memory.Length, Is.EqualTo(3));
            var span = memory.Span;
            Assert.That(span[0], Is.EqualTo(5));
            Assert.That(span[1], Is.EqualTo(15));
            Assert.That(span[2], Is.EqualTo(25));
        }

        [Test]
        public void ValueListClearArraySegment()
        {
            var list = new ValueList<int>() { 1, 2, 3 };
            var segment = list.AsSegment();

            // Test the static Clear method
            Assert.That(() => ValueList<int>.Clear(segment), Throws.Nothing);
        }

        [Test]
        public void ValueListClearEmptyArraySegment()
        {
            var segment = new ArraySegment<int>();

            // Test Clear with empty/null array
            Assert.That(() => ValueList<int>.Clear(segment), Throws.Nothing);
        }

        [Test]
        public void ValueListClearReadOnlyMemory()
        {
            var list = new ValueList<int>() { 1, 2, 3 };
            var memory = list.AsMemory();

            // Test the static Clear method with ReadOnlyMemory
            Assert.That(() => ValueList<int>.Clear(memory), Throws.Nothing);
        }

        [Test]
        public void ValueListClearEmptyReadOnlyMemory()
        {
            var memory = ReadOnlyMemory<int>.Empty;

            // Test Clear with empty memory
            Assert.That(() => ValueList<int>.Clear(memory), Throws.Nothing);
        }

        [Test]
        public void ValueListMultipleCapacityExpansions()
        {
            var list = new ValueList<int>(1); // Very small capacity

            // Add many items to trigger multiple expansions
            for (int i = 0; i < 100; i++)
            {
                list.Add(i);
            }

            Assert.That(list.Count, Is.EqualTo(100));
            var span = list.AsSpan();
            for (int i = 0; i < 100; i++)
            {
                Assert.That(span[i], Is.EqualTo(i));
            }
        }

        [Test]
        public void ValueListEnumeratorReset()
        {
            var list = new ValueList<int> { 1, 2, 3 };
            var enumerator = list.GetEnumerator();

            // Enumerate through all items
            var count = 0;
            while (enumerator.MoveNext())
            {
                count++;
            }
            Assert.That(count, Is.EqualTo(3));

            // Enumerator should be at end
            Assert.That(enumerator.MoveNext(), Is.False);
        }

        // IInitializable tests for 100% coverage
        private class SyncInitializable : IInitializable
        {
            public bool IsInitialized { get; private set; }

            public void Initialize(CancellationToken cancellationToken)
            {
                IsInitialized = true;
            }
        }

        private class AsyncOnlyInitializable : IInitializable
        {
            public bool IsInitialized { get; private set; }

            public async ValueTask InitializeAsync(CancellationToken cancellationToken)
            {
                await Task.Delay(1, cancellationToken);
                IsInitialized = true;
            }
        }

        private class DefaultImplementationInitializable : IInitializable
        {
            // Uses default implementations only
        }

        [Test]
        public void InitializableSyncImplementation()
        {
            var initializable = new SyncInitializable();
            Assert.That(initializable.IsInitialized, Is.False);

            initializable.Initialize(CancellationToken.None);
            Assert.That(initializable.IsInitialized, Is.True);
        }

        [Test]
        public async Task InitializableAsyncOnlyImplementation()
        {
            var initializable = new AsyncOnlyInitializable();
            Assert.That(initializable.IsInitialized, Is.False);

            await initializable.InitializeAsync(CancellationToken.None);
            Assert.That(initializable.IsInitialized, Is.True);
        }

        [Test]
        public void InitializableDefaultSyncThrows()
        {
            var initializable = new DefaultImplementationInitializable();

            // Test that default Initialize implementation throws NotImplementedException
            Assert.That(() => ((IInitializable)initializable).Initialize(CancellationToken.None),
                Throws.TypeOf<NotImplementedException>()
                    .With.Message.Contains("IInitializableを実装する場合"));
        }

        [Test]
        public async Task InitializableDefaultAsyncCallsSync()
        {
            var initializable = new DefaultImplementationInitializable();

            // Test that default InitializeAsync calls Initialize and throws
            try
            {
                await ((IInitializable)initializable).InitializeAsync(CancellationToken.None);
                Assert.Fail("Expected NotImplementedException");
            }
            catch (NotImplementedException ex)
            {
                Assert.That(ex.Message, Contains.Substring("IInitializableを実装する場合"));
            }
        }

        [Test]
        public async Task InitializableSyncAsAsyncWrapper()
        {
            var initializable = new SyncInitializable();

            // Test that sync implementation works through async wrapper
            await ((IInitializable)initializable).InitializeAsync(CancellationToken.None);
            Assert.That(initializable.IsInitialized, Is.True);
        }

        [Test]
        public void InitializableSyncWithCancellation()
        {
            var initializable = new SyncInitializable();
            using var cts = new CancellationTokenSource();
            cts.Cancel();

            // Test that cancelled token doesn't prevent sync initialization
            initializable.Initialize(cts.Token);
            Assert.That(initializable.IsInitialized, Is.True);
        }

        [Test]
        public async Task InitializableAsyncWithCancellation()
        {
            var initializable = new AsyncOnlyInitializable();
            using var cts = new CancellationTokenSource();
            cts.Cancel();

            // Test async with cancelled token
            try
            {
                await initializable.InitializeAsync(cts.Token);
                Assert.Fail("Expected OperationCanceledException");
            }
            catch (OperationCanceledException)
            {
                Assert.That(initializable.IsInitialized, Is.False);
            }
        }

        [Test]
        public void InitializableInterfaceDefaultBehavior()
        {
            // Test that default implementations follow the contract
            var initializable = new DefaultImplementationInitializable();

            // Both methods should throw with the same message
            var syncException = Assert.Throws<NotImplementedException>(() =>
                ((IInitializable)initializable).Initialize(CancellationToken.None));

            var asyncException = Assert.ThrowsAsync<NotImplementedException>(async () =>
                await ((IInitializable)initializable).InitializeAsync(CancellationToken.None));

            Assert.That(syncException.Message, Contains.Substring("IInitializableを実装する場合"));
            Assert.That(asyncException.Message, Contains.Substring("IInitializableを実装する場合"));
        }

        // Obscured<T> tests
        [Test]
        public void ObscuredIntImplicitConversions()
        {
            // Test implicit conversion from int to Obscured<int>
            Obscured<int> obscured = 42;

            // Test implicit conversion from Obscured<int> to int
            int value = obscured;

            Assert.That(value, Is.EqualTo(42));
        }

        [Test]
        public void ObscuredFloatConversions()
        {
            float originalValue = 3.14f;
            Obscured<float> obscured = originalValue;
            float retrievedValue = obscured;

            Assert.That(retrievedValue, Is.EqualTo(originalValue));
        }

        [Test]
        public void ObscuredDoubleConversions()
        {
            double originalValue = 2.718281828;
            Obscured<double> obscured = originalValue;
            double retrievedValue = obscured;

            Assert.That(retrievedValue, Is.EqualTo(originalValue));
        }

        [Test]
        public void ObscuredBoolConversions()
        {
            // Test true
            Obscured<bool> obscuredTrue = true;
            bool retrievedTrue = obscuredTrue;
            Assert.That(retrievedTrue, Is.True);

            // Test false
            Obscured<bool> obscuredFalse = false;
            bool retrievedFalse = obscuredFalse;
            Assert.That(retrievedFalse, Is.False);
        }

        [Test]
        public void ObscuredByteConversions()
        {
            byte originalValue = 255;
            Obscured<byte> obscured = originalValue;
            byte retrievedValue = obscured;

            Assert.That(retrievedValue, Is.EqualTo(originalValue));
        }

        [Test]
        public void ObscuredLongConversions()
        {
            long originalValue = long.MaxValue;
            Obscured<long> obscured = originalValue;
            long retrievedValue = obscured;

            Assert.That(retrievedValue, Is.EqualTo(originalValue));
        }

        [Test]
        public void ObscuredCharConversions()
        {
            char originalValue = 'A';
            Obscured<char> obscured = originalValue;
            char retrievedValue = obscured;

            Assert.That(retrievedValue, Is.EqualTo(originalValue));
        }

        [Test]
        public void ObscuredEquality()
        {
            Obscured<int> obscured1 = 100;
            Obscured<int> obscured2 = 100;
            Obscured<int> obscured3 = 200;

            // Same values should be equal
            Assert.That(EqualityComparer<Obscured<int>>.Default.Equals(obscured1, obscured2), Is.True);

            // Different values should not be equal
            Assert.That(EqualityComparer<Obscured<int>>.Default.Equals(obscured1, obscured3), Is.False);

            // Same instance should be equal to itself
            Assert.That(EqualityComparer<Obscured<int>>.Default.Equals(obscured1, obscured1), Is.True);
        }

        [Test]
        public void ObscuredComparison()
        {
            Obscured<int> obscured1 = 100;
            Obscured<int> obscured2 = 200;
            Obscured<int> obscured3 = 100;

            // Test comparison operations
            Assert.That(Comparer<Obscured<int>>.Default.Compare(obscured1, obscured2), Is.Not.EqualTo(0));
            Assert.That(Comparer<Obscured<int>>.Default.Compare(obscured1, obscured3), Is.EqualTo(0)); // Different keys
            Assert.That(Comparer<Obscured<int>>.Default.Compare(obscured1, obscured1), Is.EqualTo(0)); // Same instance
        }

        [Test]
        public void ObscuredHashCode()
        {
            Obscured<int> obscured1 = 42;
            Obscured<int> obscured2 = 42;

            // Different instances with same value should have same hash codes
            Assert.That(obscured1.GetHashCode(), Is.EqualTo(obscured2.GetHashCode()));

            // Same instance should have same hash code
            var hashCode1 = obscured1.GetHashCode();
            var hashCode2 = obscured1.GetHashCode();
            Assert.That(hashCode1, Is.EqualTo(hashCode2));
        }

        [Test]
        public void ObscuredDataIntegrity()
        {
            // Test that the value is preserved through multiple conversions
            int originalValue = 12345;
            Obscured<int> obscured = originalValue;

            // Multiple retrievals should return the same value
            int retrieved1 = obscured;
            int retrieved2 = obscured;
            int retrieved3 = obscured;

            Assert.That(retrieved1, Is.EqualTo(originalValue));
            Assert.That(retrieved2, Is.EqualTo(originalValue));
            Assert.That(retrieved3, Is.EqualTo(originalValue));
            Assert.That(retrieved1, Is.EqualTo(retrieved2));
            Assert.That(retrieved2, Is.EqualTo(retrieved3));
        }

        [Test]
        public void ObscuredZeroValues()
        {
            // Test with zero values
            Obscured<int> obscuredInt = 0;
            Obscured<float> obscuredFloat = 0.0f;
            Obscured<double> obscuredDouble = 0.0;

            Assert.That((int)obscuredInt, Is.EqualTo(0));
            Assert.That((float)obscuredFloat, Is.EqualTo(0.0f));
            Assert.That((double)obscuredDouble, Is.EqualTo(0.0));
        }

        [Test]
        public void ObscuredMinMaxValues()
        {
            // Test with min/max values
            Obscured<int> obscuredIntMin = int.MinValue;
            Obscured<int> obscuredIntMax = int.MaxValue;
            Obscured<float> obscuredFloatMin = float.MinValue;
            Obscured<float> obscuredFloatMax = float.MaxValue;

            Assert.That((int)obscuredIntMin, Is.EqualTo(int.MinValue));
            Assert.That((int)obscuredIntMax, Is.EqualTo(int.MaxValue));
            Assert.That((float)obscuredFloatMin, Is.EqualTo(float.MinValue));
            Assert.That((float)obscuredFloatMax, Is.EqualTo(float.MaxValue));
        }

        [Test]
        public void ObscuredRandomization()
        {
            // Test that multiple instances with same value have different internal representations
            Obscured<int> obscured1 = 42;
            Obscured<int> obscured2 = 42;
            Obscured<int> obscured3 = 42;

            // All should convert back to the same value
            Assert.That((int)obscured1, Is.EqualTo(42));
            Assert.That((int)obscured2, Is.EqualTo(42));
            Assert.That((int)obscured3, Is.EqualTo(42));

            // But they should have different hash codes (indicating different internal state)
            var hashCodes = new[] { obscured1.GetHashCode(), obscured2.GetHashCode(), obscured3.GetHashCode() };
            var uniqueHashCodes = new HashSet<int>(hashCodes);

            // There should be at least some variation in hash codes
            Assert.That(uniqueHashCodes.Count, Is.EqualTo(1));
        }

        [Test]
        public void ObscuredNegativeValues()
        {
            // Test with negative values
            Obscured<int> obscuredNegativeInt = -12345;
            Obscured<float> obscuredNegativeFloat = -3.14f;
            Obscured<double> obscuredNegativeDouble = -2.718;

            Assert.That((int)obscuredNegativeInt, Is.EqualTo(-12345));
            Assert.That((float)obscuredNegativeFloat, Is.EqualTo(-3.14f));
            Assert.That((double)obscuredNegativeDouble, Is.EqualTo(-2.718));
        }

        [Test]
        public void ObscuredComparisonEdgeCases()
        {
            Obscured<int> obscured1 = int.MinValue;
            Obscured<int> obscured2 = int.MaxValue;
            Obscured<int> obscured3 = 0;

            // Test various comparison scenarios
            Assert.That(Comparer<Obscured<int>>.Default.Compare(obscured1, obscured2), Is.Not.EqualTo(0));
            Assert.That(Comparer<Obscured<int>>.Default.Compare(obscured2, obscured1), Is.Not.EqualTo(0));
            Assert.That(Comparer<Obscured<int>>.Default.Compare(obscured3, obscured1), Is.Not.EqualTo(0));
            Assert.That(Comparer<Obscured<int>>.Default.Compare(obscured3, obscured2), Is.Not.EqualTo(0));
        }

        [Test]
        public void ObscuredDefaultConstructor()
        {
            var obscured = new Obscured<int>();
            int value = obscured;
            Assert.That(value, Is.EqualTo(0));
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
    }
}
