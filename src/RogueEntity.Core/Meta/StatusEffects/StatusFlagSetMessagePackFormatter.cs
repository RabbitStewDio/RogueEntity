using System;
using MessagePack;
using MessagePack.Formatters;

namespace RogueEntity.Core.Meta.StatusEffects
{
    public class StatusFlagSetMessagePackFormatter: IMessagePackFormatter<StatusFlagSet>
    {
        readonly StatusFlagRegistry registry;

        public StatusFlagSetMessagePackFormatter(StatusFlagRegistry registry)
        {
            this.registry = registry ?? throw new ArgumentNullException(nameof(registry));
        }

        public void Serialize(ref MessagePackWriter writer, StatusFlagSet value, MessagePackSerializerOptions options)
        {
            writer.Write(value.StatusEffects);
        }

        public StatusFlagSet Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            var flagSet = reader.ReadInt64();
            return new StatusFlagSet(registry, flagSet);
        }
    }
}