using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using AndanteTribe.Utils.Csv;
using MasterMemory;
using MasterMemory.Meta;
using MessagePack;

namespace AndanteTribe.Utils.MasterServices
{
    public class MasterConverter
    {
        private sealed class Builder(IFormatterResolver? resolver = null) : DatabaseBuilderBase(resolver);

        /// <summary>
        /// 入力ディレクトリパス.
        /// </summary>
        public readonly string InputDirectoryPath;

        /// <summary>
        /// マスターメタデータ.
        /// </summary>
        public readonly MetaDatabase Metadata;

        /// <summary>
        /// 出力の際に暗号化するかどうか.
        /// </summary>
        public bool IsEncrypt { get; init; } = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="MasterConverter"/> class.
        /// </summary>
        /// <param name="inputDirectoryPath">入力ディレクトリパス.</param>
        /// <param name="metadata">マスターメタデータ.</param>
        public MasterConverter(string inputDirectoryPath, MetaDatabase metadata)
        {
            InputDirectoryPath = inputDirectoryPath;
            Metadata = metadata;
        }

        /// <summary>
        /// マスターデータをコンバートして、指定されたパスにバイナリを出力します.
        /// </summary>
        /// <param name="outputPath"></param>
        /// <param name="cancellationToken"></param>
        public async ValueTask BuildAsync(string outputPath, CancellationToken cancellationToken = default)
        {
            var bin = await LoadAsync(cancellationToken);

            await using var fileStream = File.Create(outputPath, 4096, FileOptions.Asynchronous);
            await fileStream.WriteAsync(bin, cancellationToken);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<byte[]> LoadAsync<TEnum>(TEnum language, CancellationToken cancellationToken = default) where TEnum : unmanaged, Enum
        {

        }

        /// <summary>
        /// マスターデータをコンバートして、バイナリを生成します.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<byte[]> LoadAsync(CancellationToken cancellationToken = default)
        {
            var builder = new Builder();
            var tasks = new List<Task<(Type, IList<object>)>>();
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            foreach (var tableInfo in Metadata.GetTableInfos())
            {
                var fileName = tableInfo.DataType.GetCustomAttribute<FileNameAttribute>()?.Name;
                if (fileName == null)
                {
                    throw new InvalidOperationException($"FileReferenceAttribute not found on {tableInfo.DataType.Name}.");
                }

                var fullPath = Path.Combine(InputDirectoryPath, fileName);

                var task = CreateNonLocalizedLoadTask(tableInfo, fullPath, cts);
                tasks.Add(task);
            }

            var results = await Task.WhenAll(tasks);
            foreach (var (type, data) in results.AsSpan())
            {
                builder.AppendDynamic(type, data);
            }

            return builder.Build();
        }

        // ビットフラグ列挙体から全言語を取得.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private IEnumerable<TEnum> GetAllBitLanguage<TEnum>(TEnum language) where TEnum : unmanaged, Enum
        {
            // IgnoreDataMemberAttributeつきフィールドは除外
            var enumValues = typeof(TEnum).GetFields()
                .Where(static f => !f.IsDefined(typeof(IgnoreDataMemberAttribute)))
                .Select(static f => (TEnum)f.GetValue(null))
                .ToArray();

            if (typeof(TEnum).IsDefined(typeof(FlagsAttribute)) && language.ConstructFlags())
            {
                foreach (var e in enumValues)
                {
                    if (language.HasBitFlags(e))
                    {
                        yield return e;
                    }
                }
                yield break;
            }

            yield return language;
        }

        // 非ローカライズな読み込み処理生成.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Task<(Type, IList<object>)> CreateNonLocalizedLoadTask(MetaTable table, string path, CancellationTokenSource cancellationTokenSource)
        {
            return new Task<(Type, IList<object>)>(static args =>
            {
                var (table, path, cts) = (StateTuple<MetaTable, string, CancellationTokenSource>)args;
                try
                {
                    var list = new List<object>();

                    using var stream = File.OpenRead(path);
                    using var reader = new CsvReader(stream);

                    while (reader.ReadLine())
                    {
#if NET8_0_OR_GREATER
                        var data = RuntimeHelpers.GetUninitializedObject(table.DataType);
#else
                        var data = FormatterServices.GetUninitializedObject(table.DataType);
#endif
                        // bind value
                        foreach (var prop in table.Properties)
                        {
                            var value = reader.ReadObject(prop.PropertyInfo);
                            prop.PropertyInfo.SetValue(data, value);
                        }

                        list.Add(data);
                    }

                    return (table.DataType, list);
                }
                catch (OperationCanceledException e) when (e.CancellationToken == cts.Token)
                {
                    // 別スレッドのキャンセルによる例外は無視.
                    return default;
                }
                catch (Exception) when (!cts.IsCancellationRequested)
                {
                    cts.Cancel();
                    throw;
                }

            }, StateTuple.Create(table, path, cancellationTokenSource), cancellationTokenSource.Token);
        }

        private Task<(Type, IList<object>)> CreateLocalizedLoadTask(MetaTable table, int languageNo, string path, CancellationTokenSource cancellationTokenSource)
        {

        }
    }
}