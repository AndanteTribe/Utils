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
    /// /// /// <summary>
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
    /// <remarks>
    /// 指定する列挙体はint型が基になる型として指定されている必要があります.
    /// </remarks>
    public static bool HasBitFlags<T>(this T value, T flag) where T : struct, Enum
    {
        var v = Unsafe.As<T, int>(ref value);
        var f = Unsafe.As<T, int>(ref flag);
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
    /// <remarks>
    /// 指定する列挙体はint型が基になる型として指定されている必要があります.
    /// </remarks>
    public static bool ConstructFlags<T>(this T value) where T : struct, Enum
    {
        var v = Unsafe.As<T, int>(ref value);
        return v != 0 && (v & (v - 1)) == 0;
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
    /// <remarks>
    /// 指定する列挙体はint型が基になる型として指定されている必要があります.
    /// </remarks>
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