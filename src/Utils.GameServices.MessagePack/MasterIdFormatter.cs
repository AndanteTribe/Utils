using MessagePack;
using MessagePack.Formatters;

namespace AndanteTribe.Utils.GameServices.MessagePack;

/// <summary>
/// <see cref="MasterId{TGroup}"/>の<see cref="IMessagePackFormatter{T}"/>.
/// </summary>
/// <typeparam name="TGroup"></typeparam>
public sealed class MasterIdFormatter<TGroup> : IMessagePackFormatter<MasterId<TGroup>> where TGroup : unmanaged, Enum
{
    /// <inheritdoc />
    public void Serialize(ref MessagePackWriter writer, MasterId<TGroup> value, MessagePackSerializerOptions options)
    {
        writer.WriteArrayHeader(2);
        options.Resolver.GetFormatterWithVerify<TGroup>().Serialize(ref writer, value.Group, options);
        writer.Write(value.Id);
    }

    /// <inheritdoc />
    public MasterId<TGroup> Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        if (reader.TryReadNil())
        {
            throw new InvalidOperationException("typecode is null, struct not supported.");
        }

        options.Security.DepthStep(ref reader);
        _ = reader.ReadArrayHeader();

        var group = options.Resolver.GetFormatterWithVerify<TGroup>().Deserialize(ref reader, options);
        var id = reader.ReadUInt32();
        reader.Depth--;

        return new MasterId<TGroup>(group, id);
    }
}