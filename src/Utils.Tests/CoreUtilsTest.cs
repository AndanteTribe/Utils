using System;
using System.Collections.Generic;
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
        public void CreateValidComparer()
        {
            // 比較関数
            Func<int, int, bool> equals = (x, y) => x == y;
            Func<int, int> getHashCode = x => x.GetHashCode();

            // 比較器を作成
            var comparer = EqualityComparer.Create(equals, getHashCode);

            // 比較テスト
            Assert.That(comparer.Equals(1, 1), Is.True);
            Assert.That(comparer.Equals(1, 2), Is.False);

            // ハッシュコードテスト
            Assert.That(comparer.GetHashCode(1), Is.EqualTo(1.GetHashCode()));
            Assert.That(comparer.GetHashCode(2), Is.EqualTo(2.GetHashCode()));
        }

        // equals が null の場合、ArgumentNullException がスローされることを確認
        [Test]
        public void CreateEqualsIsNullArgumentNullException()
        {
            Assert.That(() => EqualityComparer.Create<int>(null, x => x.GetHashCode()),
                Throws.ArgumentNullException.With.Property("ParamName").EqualTo("equals"));
        }

        // getHashCode が null の場合、NotSupportedException がスローされることを確認
        [Test]
        public void CreateGetHashCodeIsNullNotSupportedException()
        {
            Assert.That(() => EqualityComparer.Create<int>((x, y) => x == y, null).GetHashCode(13),
                Throws.TypeOf<NotSupportedException>().With.Message.EqualTo("getHashCode"));
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
        [TestCase("apple", "banana", "cherry")]
        [TestCase("")]
        [TestCase()]
        public void StringUtilsJoin(params string[] array)
        {
            var result = StringUtils.Join(',', array);
            Assert.That(result, Is.EqualTo(string.Join(',', array)));
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
    }
}