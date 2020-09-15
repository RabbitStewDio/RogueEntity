using System;
using MessagePack;
using MessagePack.Formatters;

namespace RogueEntity.Core.Meta.StatusEffects
{
    public class StatusFlagMessagePackFormatter: IMessagePackFormatter<StatusFlag>
    {
        readonly StatusFlagRegistry flagRegistry;

        public StatusFlagMessagePackFormatter(StatusFlagRegistry flagRegistry)
        {
            this.flagRegistry = flagRegistry ?? throw new ArgumentNullException(nameof(flagRegistry));
        }

        public void Serialize(ref MessagePackWriter writer, StatusFlag value, MessagePackSerializerOptions options)
        {
            writer.Write(value.LinearIndex);
        }

        public StatusFlag Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            var linIdx = reader.ReadInt32();
            if (flagRegistry.TryGet(linIdx, out var flag))
            {
                return flag;
            }
            throw new MessagePackSerializationException($"Unable to find status flag for linear index {linIdx}");
        }
    }
}