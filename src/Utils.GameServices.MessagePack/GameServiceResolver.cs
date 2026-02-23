using MessagePack;
using MessagePack.Formatters;

namespace AndanteTribe.Utils.GameServices.MessagePack;

/// <summary>
/// <see cref="AndanteTribe.Utils.GameServices"/>オブジェクトの<see cref="IFormatterResolver"/>.
/// </summary>
public sealed class GameServiceResolver : IFormatterResolver
{
    /// <summary>
    /// <see cref="GameServiceResolver"/>の共有インスタンス.
    /// </summary>
    public static readonly IFormatterResolver Shared = new GameServiceResolver();

    /// <inheritdoc />
    public IMessagePackFormatter<T>? GetFormatter<T>() => FormatterCache<T>.Formatter;

    private static class FormatterCache<T>
    {
        public static readonly IMessagePackFormatter<T>? Formatter;

        static FormatterCache()
        {
            var formatter = GameServiceResolverGetFormatterHelper.GetFormatter(typeof(T));
            if (formatter != null)
            {
                Formatter = (IMessagePackFormatter<T>)formatter;
            }
        }
    }

    private static class GameServiceResolverGetFormatterHelper
    {
        private static readonly RuntimeTypeHandle s_embedArrayTypeHandle = typeof((int, string)[]).TypeHandle;
        private static readonly RuntimeTypeHandle s_embedTypeHandle = typeof((int, string)).TypeHandle;
        private static readonly RuntimeTypeHandle s_localizeFormatTypeHandle = typeof(LocalizeFormat).TypeHandle;
        private static readonly RuntimeTypeHandle s_masterIdTypeHandle = typeof(MasterId<>).TypeHandle;

        public static object? GetFormatter(Type type)
        {
            var handle = type.TypeHandle;
            return handle switch
            {
                _ when handle.Equals(s_embedArrayTypeHandle) => new ArrayFormatter<(int, string)>(),
                _ when handle.Equals(s_embedTypeHandle) => new ValueTupleFormatter<int, string>(),
                _ when handle.Equals(s_localizeFormatTypeHandle) => new LocalizeFormatFormatter(),
                _ when type.IsGenericType && type.GetGenericTypeDefinition().TypeHandle.Equals(s_masterIdTypeHandle)
                    => Activator.CreateInstance(typeof(MasterIdFormatter<>).MakeGenericType(type.GenericTypeArguments)),
                _ => null
            };
        }
    }
}