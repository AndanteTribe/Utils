namespace AndanteTribe.Utils
{
    /// <summary>
    /// <see cref="EqualityComparer{T}"/>の拡張クラス.
    /// </summary>
    public static class EqualityComparer
    {
        /// <summary>
        /// 指定した比較処理を使用して<see cref="IEqualityComparer{T}"/>を生成します.
        /// </summary>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// using System.Collections.Generic;
        /// using MinimalUtility;
        ///
        /// public enum Fruits : int
        /// {
        ///     Apple,
        ///     Orange,
        ///     Banana,
        /// }
        ///
        /// public class EqualityComparerFactoryExample
        /// {
        ///     private readonly Dictionary<Fruits, string> _fruits;
        ///
        ///     public EqualityComparerFactoryExample()
        ///     {
        ///         var equalityComparer = EqualityComparer.Create<Fruits>(
        ///             static (x, y) => (int) x == (int) y,
        ///             static x => ((int) x).GetHashCode());
        ///         _fruits = new Dictionary<Fruits, string>(3, equalityComparer)
        ///         {
        ///             {Fruits.Apple, "Apple"},
        ///             {Fruits.Orange, "Orange"},
        ///             {Fruits.Banana, "Banana"},
        ///         };
        ///     }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="equals"><see cref="IEqualityComparer{T}.Equals(T, T)"/>に使用する処理.</param>
        /// <param name="getHashCode"><see cref="IEqualityComparer{T}.GetHashCode(T)"/>に使用する処理.</param>
        /// <typeparam name="T">比較対象の型.</typeparam>
        /// <returns>生成された<see cref="IEqualityComparer{T}"/>.</returns>
        /// <exception cref="ArgumentNullException"><see cref="equals"/>がnullです.</exception>
        /// <exception cref="NotSupportedException"><see cref="getHashCode"/>がnullです.</exception>
        public static IEqualityComparer<T> Create<T>(Func<T, T, bool> equals, Func<T, int>? getHashCode)
        {
            if (equals == null)
            {
                throw new ArgumentNullException(nameof(equals));
            }
            getHashCode ??= static _ => throw new NotSupportedException(nameof(getHashCode));
            return new DelegateEqualityComparer<T>(equals, getHashCode);
        }

        private sealed class DelegateEqualityComparer<T>(Func<T, T, bool> equals, Func<T, int> getHashCode) : IEqualityComparer<T>
        {
            public bool Equals(T? x, T? y) => equals(x!, y!);

            public int GetHashCode(T obj) => getHashCode(obj);

            public override int GetHashCode() => HashCode.Combine(equals.GetHashCode(), getHashCode.GetHashCode());
        }
    }
}