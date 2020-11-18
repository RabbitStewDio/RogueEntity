using MessagePack;
using MessagePack.Formatters;

namespace RogueEntity.Api.Utils
{
    public class OptionalMessagePackFormatter<TValue>: IMessagePackFormatter<Optional<TValue>>
    {
        public void Serialize(ref MessagePackWriter writer, Optional<TValue> value, MessagePackSerializerOptions options)
        {
            if (value.TryGetValue(out var containedValue))
            {
                MessagePackSerializer.Serialize(ref writer, containedValue, options);
            }
            else
            {
                writer.WriteNil();
            }
        }

        public Optional<TValue> Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            if (reader.TryReadNil())
            {
                return Optional.Empty();
            }

            var v = MessagePackSerializer.Deserialize<TValue>(ref reader, options);
            return Optional.ValueOf(v);
        }
    }
}