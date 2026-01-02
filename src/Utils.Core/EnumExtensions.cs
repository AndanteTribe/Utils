using System.Runtime.CompilerServices;

namespace AndanteTribe.Utils;

/// <summary>
/// 列挙体の拡張メソッド.
/// </summary>
public static class EnumExtensions
{
    /// <summary>
    /// 指定のビットフラグを持っているかどうかを判定します.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="flag"></param>
    /// <typeparam name="T"></typeparam>
    /// <example>
    /// <code>
    /// <![CDATA[
    /// /// <summary>
    /// /// ファイルアクセス権を定義するフラグ付き列挙体
    /// /// </summary>
    /// [System.Flags]
    /// public enum FileAccess
    /// {
    ///     None = 0,
    ///     Read = 1 << 0,      // 0001
    ///     Write = 1 << 1,     // 0010
    ///     Execute = 1 << 2,   // 0100
    ///     ReadWrite = Read | Write, // 0011
    /// }
    ///
    /// public class Sample
    /// {
    ///     public void Run()
    ///     {
    ///         // 現在の権限を設定
    ///         const FileAccess currentPermissions = FileAccess.ReadWrite;
    ///
    ///         // 1. 単一のフラグを確認する
    ///         // Read権限を持っているか？ (true)
    ///         var hasRead = currentPermissions.HasBitFlags(FileAccess.Read);
    ///         Console.WriteLine($"Read権限があるか: {hasRead}");
    ///
    ///         // Execute権限を持っているか？ (false)
    ///         var hasExecute = currentPermissions.HasBitFlags(FileAccess.Execute);
    ///         Console.WriteLine($"Execute権限があるか: {hasExecute}");
    ///
    ///         // 2. 複数のフラグをまとめて確認する
    ///         // ReadとWriteの両方の権限を持っているか？ (true)
    ///         var hasReadWrite = currentPermissions.HasBitFlags(FileAccess.ReadWrite);
    ///         Console.WriteLine($"ReadとWrite権限があるか: {hasReadWrite}");
    ///
    ///         // 3. 自身に含まれないフラグを確認する
    ///         const FileAccess fullAccess = FileAccess.Read | FileAccess.Write | FileAccess.Execute;
    ///
    ///         // すべての権限を持っているか？ (false)
    ///         var hasFullAccess = currentPermissions.HasBitFlags(fullAccess);
    ///         Console.WriteLine($"すべての権限があるか: {hasFullAccess}");
    ///     }
    /// }
    /// ]]>
    /// </code>
    /// </example>
    /// <returns></returns>
    /// <exception cref="NotSupportedException">対象の列挙体の基礎型がサポートされていない場合にスローされます.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool HasBitFlags<T>(this T value, T flag) where T : struct, Enum
    {
        var (v, f) = Unsafe.SizeOf<T>() switch
        {
            1 => (Unsafe.As<T, byte>(ref value), Unsafe.As<T, byte>(ref flag)),
            2 => (Unsafe.As<T, short>(ref value), Unsafe.As<T, short>(ref flag)),
            4 => (Unsafe.As<T, int>(ref value), Unsafe.As<T, int>(ref flag)),
            8 => (Unsafe.As<T, long>(ref value), Unsafe.As<T, long>(ref flag)),
            _ => throw new NotSupportedException("Unsupported enum underlying type size.")
        };
        return (v & f) == f;
    }

    /// <summary>
    /// 1つのビットフラグを持っているかどうかを判定します.
    /// </summary>
    /// <param name="value"></param>
    /// <typeparam name="T"></typeparam>
    /// <example>
    /// <code>
    /// <![CDATA[
    /// /// <summary>
    /// /// ファイルアクセス権を定義するフラグ付き列挙体
    /// /// </summary>
    /// [System.Flags]
    /// public enum FileAccess
    /// {
    ///     None = 0,
    ///     Read = 1 << 0,      // 0001
    ///     Write = 1 << 1,     // 0010
    ///     Execute = 1 << 2,   // 0100
    ///     ReadWrite = Read | Write, // 0011
    ///     All = Read | Write | Execute
    /// }
    ///
    /// public class Sample
    /// {
    ///     public void Run()
    ///     {
    ///         // Readのみなので単一フラグ
    ///         // 単一フラグか: (true)
    ///         Console.WriteLine($"単一フラグか:{FileAccess.Read.ConstructFlags()}");
    ///
    ///         // ReadとWrite２つのフラグを持っているので単一フラグではない
    ///         // 単一フラグか: (false)
    ///         Console.WriteLine($"単一フラグか:{FileAccess.ReadWrite.ConstructFlags()}");
    ///
    ///         // ReadとWriteとExecute3つのフラグを持っているので単一フラグではない
    ///         // 単一フラグか: (false)
    ///         Console.WriteLine($"単一フラグか:{FileAccess.All.ConstructFlags()}");
    ///
    ///         // 何もフラグを持っていないので単一フラグではない
    ///         // 単一フラグか: (false)
    ///         Console.WriteLine($"単一フラグか:{FileAccess.None.ConstructFlags()}");
    ///
    ///     }
    /// }
    /// ]]>
    /// </code>
    /// </example>
    /// <returns></returns>
    /// <exception cref="NotSupportedException">対象の列挙体の基礎型がサポートされていない場合にスローされます.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool ConstructFlags<T>(this T value) where T : struct, Enum
    {
        var v = Unsafe.SizeOf<T>() switch
        {
            1 => Unsafe.As<T, byte>(ref value),
            2 => Unsafe.As<T, short>(ref value),
            4 => Unsafe.As<T, int>(ref value),
            8 => Unsafe.As<T, long>(ref value),
            _ => throw new NotSupportedException("Unsupported enum underlying type size.")
        };
        return v != 0 && (v & (v - 1)) == 0;
    }

    /// <summary>
    /// 指定のビットフラグを集約します.
    /// </summary>
    /// <param name="flags"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    /// <exception cref="NotSupportedException">対象の列挙体の基礎型がサポートされていない場合にスローされます.</exception>
    /// <example>
    /// <code>
    /// <![CDATA[
    /// [System.Flags]
    /// public enum FileAccess
    /// {
    ///     None = 0,
    ///     Read = 1 << 0,      // 0001
    ///     Write = 1 << 1,     // 0010
    ///     Execute = 1 << 2,   // 0100
    ///     ReadWrite = Read | Write,
    ///     All = Read | Write | Execute
    /// }
    ///
    /// public class Sample
    /// {
    ///     public void Run()
    ///     {
    ///         // 配列から ReadOnlySpan を作って集約する例
    ///         var flags = new[] { FileAccess.Read, FileAccess.Write };
    ///         var aggregated = flags.AsSpan().AggregateFlags();
    ///         // 出力: ReadWrite
    ///         Console.WriteLine(aggregated);
    ///
    ///         // 単一要素
    ///         var single = new[] { FileAccess.Execute };
    ///         var aggregatedSingle = single.AsSpan().AggregateFlags();
    ///         // 出力: Execute
    ///         Console.WriteLine(aggregatedSingle);
    ///
    ///         // 空配列 -> デフォルト(= None)
    ///         var empty = Array.Empty<FileAccess>();
    ///         var aggregatedEmpty = empty.AsSpan().AggregateFlags();
    ///         // 出力: None
    ///         Console.WriteLine(aggregatedEmpty);
    ///     }
    /// }
    /// ]]>
    /// </code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T AggregateFlags<T>(this Span<T> flags) where T : struct, Enum => AggregateFlags((ReadOnlySpan<T>)flags);

    /// <summary>
    /// 指定のビットフラグを集約します.
    /// </summary>
    /// <param name="flags"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    /// <exception cref="NotSupportedException">対象の列挙体の基礎型がサポートされていない場合にスローされます.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T AggregateFlags<T>(this ReadOnlySpan<T> flags) where T : struct, Enum
    {
        switch (Unsafe.SizeOf<T>())
        {
            case 1:
                var r1 = default(byte);
                var e1 = flags.GetEnumerator();
                while (e1.MoveNext())
                {
                    var flag = e1.Current;
                    var f = Unsafe.As<T, byte>(ref flag);
                    r1 = (byte)(r1 | f);
                }
                return Unsafe.As<byte, T>(ref r1);
            case 2:
                var r2 = default(short);
                var e2 = flags.GetEnumerator();
                while (e2.MoveNext())
                {
                    var flag = e2.Current;
                    var f = Unsafe.As<T, short>(ref flag);
                    r2 = (short)(r2 | f);
                }
                return Unsafe.As<short, T>(ref r2);
            case 4:
                var r4 = 0;
                var e4 = flags.GetEnumerator();
                while (e4.MoveNext())
                {
                    var flag = e4.Current;
                    var f = Unsafe.As<T, int>(ref flag);
                    r4 |= f;
                }
                return Unsafe.As<int, T>(ref r4);
            case 8:
                var r8 = 0L;
                var e8 = flags.GetEnumerator();
                while (e8.MoveNext())
                {
                    var flag = e8.Current;
                    var f = Unsafe.As<T, long>(ref flag);
                    r8 |= f;
                }
                return Unsafe.As<long, T>(ref r8);
            default:
                throw new NotSupportedException("Unsupported enum underlying type size.");
        }
    }

    /// <summary>
    /// 列挙体の全てのビットフラグを取得します.
    /// </summary>
    /// <param name="value"></param>
    /// <typeparam name="T"></typeparam>
    /// <example>
    /// <code>
    /// <![CDATA[
    /// [System.Flags]
    /// public enum FileAccess
    /// {
    ///     None = 0,
    ///     Read = 1 << 0,      // 0001
    ///     Write = 1 << 1,     // 0010
    ///     Execute = 1 << 2,   // 0100
    ///     ReadWrite = Read | Write, // 0011
    ///     All = Read | Write | Execute
    /// }
    ///
    /// public class Sample
    /// {
    ///     public void Run()
    ///     {
    ///         FileAccess currentPermissions = FileAccess.Read | FileAccess.Write;
    ///
    ///         //foreachが実行可能になる
    ///         foreach (var flag in currentPermissions)
    ///         {
    ///             // 実行結果
    ///             // Read
    ///             // Write
    ///             Console.WriteLine(flag);
    ///         }
    ///
    ///         currentPermissions = FileAccess.None | FileAccess.All ;
    ///
    ///         foreach (var flag in currentPermissions)
    ///         {
    ///             // 実行結果 Noneは出力されない
    ///             // Read
    ///             // Write
    ///             // Execute
    ///             Console.WriteLine(flag);
    ///         }
    ///
    ///     }
    /// }
    /// ]]>
    /// </code>
    /// </example>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Enumerator<T> GetEnumerator<T>(this T value) where T : struct, Enum => new(value);

    /// <summary>
    /// 列挙体の全てのビットフラグを取得します.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public struct Enumerator<T> where T : struct, Enum
    {
        private int _value;

        /// <summary>
        /// 現在の列挙体の値を取得します.
        /// </summary>
        public T Current { get; private set; }

        internal Enumerator(T value)
        {
            _value = Unsafe.As<T, int>(ref value);
            Current = default;
        }

        /// <summary>
        /// 列挙体の次のビットフラグを取得します.
        /// </summary>
        /// <returns></returns>
        public bool MoveNext()
        {
            if (_value == 0)
            {
                return false;
            }

            var f = _value & -_value; // get lowest flag
            Current = Unsafe.As<int, T>(ref f);
            _value &= ~f;
            return true;
        }
    }
}