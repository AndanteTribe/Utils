using MessagePack;
using MessagePack.Formatters;

namespace AndanteTribe.Utils.GameServices.MessagePack;

/// <summary>
/// <see cref="LocalizeFormat"/>の<see cref="IMessagePackFormatter{T}"/>.
/// </summary>
public sealed class LocalizeFormatFormatter : IMessagePackFormatter<LocalizeFormat?>
{
    /// <inheritdoc />
    public void Serialize(ref MessagePackWriter writer, LocalizeFormat? value, MessagePackSerializerOptions options)
    {
        if (value is null)
        {
            writer.WriteNil();
            return;
        }

        writer.WriteArrayHeader(3);
        options.Resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value._literal, options);
        options.Resolver.GetFormatterWithVerify<(int index, string format)[]>().Serialize(ref writer, value._embed, options);
        writer.Write(value._literalLength);
    }

    /// <inheritdoc />
    public LocalizeFormat? Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        if (reader.TryReadNil())
        {
            return null;
        }

        options.Security.DepthStep(ref reader);
        _ = reader.ReadArrayHeader();

        var literal = options.Resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
        var embed = options.Resolver.GetFormatterWithVerify<(int index, string format)[]>().Deserialize(ref reader, options);
        var literalLength = reader.ReadInt32();
        reader.Depth--;

        return new LocalizeFormat(literal, embed, literalLength);
    }
}