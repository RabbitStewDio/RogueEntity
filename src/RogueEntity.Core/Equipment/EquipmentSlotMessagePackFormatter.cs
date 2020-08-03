using System;
using MessagePack;
using MessagePack.Formatters;

namespace RogueEntity.Core.Equipment
{
    public class EquipmentSlotMessagePackFormatter : IMessagePackFormatter<EquipmentSlot>
    {
        readonly EquipmentSlotRegistryLookup registry;

        public EquipmentSlotMessagePackFormatter(EquipmentSlotRegistry registry)
        {
            if (registry == null)
            {
                throw new ArgumentNullException(nameof(registry));
            }

            this.registry = registry.TryGet;
        }

        public EquipmentSlotMessagePackFormatter(EquipmentSlotRegistryLookup registry)
        {
            this.registry = registry;
        }

        public void Serialize(ref MessagePackWriter writer, EquipmentSlot value, MessagePackSerializerOptions options)
        {
            writer.Write(value.Id);
        }

        public EquipmentSlot Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            var id = reader.ReadString();
            if (registry(id, out var slot))
            {
                return slot;
            }
            throw new MessagePackSerializationException($"Unable to resolve surrogate value {id} for type EquipmentSlot");
        }
    }
}