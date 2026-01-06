using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using MasterMemory;
using MasterMemory.Meta;

namespace AndanteTribe.Utils.MasterServices;

/// <summary>
/// マスターデータコンバーター.
/// </summary>
public static class MasterConverter
{
    /// <summary>
    /// マスターデータをコンバートして、指定されたパスにバイナリを出力します.
    /// </summary>
    /// <param name="settings">マスター設定.</param>
    /// <param name="outputPath">出力パス.</param>
    /// <param name="aes">暗号化用AES. 指定しない場合は暗号化しません.</param>
    public static void Build(MasterSettings settings, string outputPath, Aes? aes = null)
    {
        using var fileStream = File.Create(outputPath);

        if (aes != null)
        {
            using var cryptoStream = new CryptoStream(fileStream, aes.CreateEncryptor(), CryptoStreamMode.Write);
            LoadCore(settings).WriteToStream(cryptoStream);
            cryptoStream.FlushFinalBlock();
            return;
        }

        LoadCore(settings).WriteToStream(fileStream);
    }

    /// <summary>
    /// マスターデータをコンバートして、バイナリを生成します.
    /// </summary>
    /// <param name="settings">マスター設定.</param>
    /// <returns>バイナリ.</returns>
    /// <exception cref="InvalidOperationException"></exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte[] Load(MasterSettings settings) => LoadCore(settings).Build();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static DatabaseBuilderBase LoadCore(MasterSettings settings)
    {
        var builder = settings.BuilderFactory();
        var actions = new List<Action>();
        var container = new ConcurrentBag<(Type, IList<object>)>();

        foreach (var tableInfo in settings.Metadata.GetTableInfos())
        {
            var fileName = tableInfo.DataType.GetCustomAttribute<FileNameAttribute>()?.Name;
            if (fileName == null)
            {
                throw new InvalidOperationException($"{nameof(FileNameAttribute)} not found on {tableInfo.DataType.Name}.");
            }

            if (!fileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            {
                fileName += ".csv";
            }

            var fullPath = Path.Combine(settings.InputDirectoryPath, fileName);

            actions.Add(() => LoadFileCore(tableInfo, fullPath, settings.LanguageIndex, settings.MaxLanguageCount, container));
        }

        // foreach (var action in actions)
        // {
        //     action.Invoke();
        // }

        Parallel.Invoke(new ParallelOptions
        {
            MaxDegreeOfParallelism = Environment.ProcessorCount,
        }, actions.ToArray());

        foreach (var (type, data) in container)
        {
            builder.AppendDynamic(type, data);
        }

        return builder;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void LoadFileCore(MetaTable table, string path, uint languageIndex, uint maxLanguageCount, ConcurrentBag<(Type, IList<object>)> container)
    {
        using var stream = File.OpenRead(path);
        using var reader = new LocalizeCsvReader(stream) { MaxLanguageCount = maxLanguageCount };
        var list = new List<object>();

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
                var value = reader.ReadObject(prop.PropertyInfo, languageIndex);
                prop.PropertyInfo.SetValue(data, value);
            }

            list.Add(data);
        }

        container.Add((table.DataType, list));
    }
}