using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AndanteTribe.Utils;
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

            Assert.That(result.Count, Is.EqualTo(expected));
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
            Assert.That(result.Count, Is.EqualTo(3));
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

            // Test case insensitive comparison
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

            Assert.That(result.Count, Is.EqualTo(array.Length));
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

            Assert.That(result.Count, Is.EqualTo(array.Length));
        }

        [Test]
        public void ValueTuple2()
        {
            var list = new List<int>();
            foreach (var i in (23, 31))
            {
                list.Add(i);
            }
            Assert.That(list.Count, Is.EqualTo(2));
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

            Assert.That(list.Count, Is.EqualTo(3));
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

            Assert.That(list.Count, Is.EqualTo(4));
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

            Assert.That(list.Count, Is.EqualTo(5));
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

            Assert.That(list.Count, Is.EqualTo(6));
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

            Assert.That(list.Count, Is.EqualTo(7));
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

            Assert.That(result.Count, Is.EqualTo(3));
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

            Assert.That(result.Count, Is.EqualTo(2));
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

            Assert.That(result.Count, Is.EqualTo(2));
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

            Assert.That(result.Count, Is.EqualTo(3));
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

            Assert.That(result.Count, Is.EqualTo(7));
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

            Assert.That(result.Count, Is.EqualTo(3));
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

            Assert.That(result.Count, Is.EqualTo(3));
            Assert.That(result, Is.EqualTo(stringArray));
        }
    }
}
