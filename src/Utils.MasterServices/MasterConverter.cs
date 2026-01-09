using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using AndanteTribe.Utils.GameServices;
using MasterMemory;
using MasterMemory.Meta;
using MessagePack;
using MessagePack.Formatters;

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
    /// <param name="cryptoTransform">暗号化用トランスフォーム. 指定しない場合は暗号化しません.</param>
    public static void Build(MasterSettings settings, string outputPath, ICryptoTransform? cryptoTransform = null)
    {
        using var fileStream = File.Create(outputPath);

        if (cryptoTransform != null)
        {
            using var cryptoStream = new CryptoStream(fileStream, cryptoTransform, CryptoStreamMode.Write);
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

    /// <summary>
    /// マスターデータ内に含まれる全ての文字を取得します.
    /// </summary>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static char[] GetAllCharacters(MasterSettings settings)
    {
        var option = MessagePackSerializer.DefaultOptions;
        var hashset = new HashSet<char>();
        var resolver = new CollectAllCharactersResolver(MessagePackSerializerOptions.Standard.Resolver, hashset);
        MessagePackSerializer.DefaultOptions = option.WithResolver(resolver);

        try
        {
            LoadCore(settings);
        }
        finally
        {
            MessagePackSerializer.DefaultOptions = option;
        }

        return hashset.ToArray();
    }

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

#pragma warning disable MsgPack013 // Inaccessible formatter
    internal class CollectAllCharactersResolver(IFormatterResolver innerResolver, HashSet<char> hashset) : IFormatterResolver, IMessagePackFormatter<string?>, IMessagePackFormatter<LocalizeFormat?>
    {
        private readonly object _lock = new();
        private readonly IMessagePackFormatter<LocalizeFormat?> _localizeFormatter = GameServiceResolver.Instance.GetFormatter<LocalizeFormat?>()!;

        public IMessagePackFormatter<T>? GetFormatter<T>()
        {
            if (typeof(T) == typeof(string) || typeof(T) == typeof(LocalizeFormat))
            {
                return (IMessagePackFormatter<T>)this;
            }

            return innerResolver.GetFormatter<T>();
        }

        string IMessagePackFormatter<string?>.Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        LocalizeFormat? IMessagePackFormatter<LocalizeFormat?>.Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            return _localizeFormatter.Deserialize(ref reader, options);
        }

        void IMessagePackFormatter<string?>.Serialize(ref MessagePackWriter writer, string? value, MessagePackSerializerOptions options)
        {
            writer.Write(value);
            if (value == null)
            {
                return;
            }

            lock (_lock)
            {
                foreach (var c in value.AsSpan())
                {
                    if (!char.IsWhiteSpace(c))
                    {
                        hashset.Add(c);
                    }
                }
            }
        }

        void IMessagePackFormatter<LocalizeFormat?>.Serialize(ref MessagePackWriter writer, LocalizeFormat? value, MessagePackSerializerOptions options)
        {
            _localizeFormatter.Serialize(ref writer, value, options);
            if (value == null)
            {
                return;
            }

            lock (_lock)
            {
                foreach (var literal in value.Literal)
                {
                    foreach (var c in literal.AsSpan())
                    {
                        if (!char.IsWhiteSpace(c))
                        {
                            hashset.Add(c);
                        }
                    }
                }
            }
        }
    }
}