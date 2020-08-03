using System;
using EnTTSharp.Serialization.Xml;

namespace RogueEntity.Core.Equipment
{
    public class EquipmentSlotSurrogateProvider: SerializationSurrogateProviderBase<EquipmentSlot, SurrogateContainer<string>>
    {
        readonly EquipmentSlotRegistryLookup registry;

        public EquipmentSlotSurrogateProvider(EquipmentSlotRegistry registry)
        {
            if (registry == null)
            {
                throw new ArgumentNullException(nameof(registry));
            }

            this.registry = registry.TryGet;
        }

        public EquipmentSlotSurrogateProvider(EquipmentSlotRegistryLookup registry)
        {
            this.registry = registry;
        }

        public override EquipmentSlot GetDeserializedObject(SurrogateContainer<string> surrogate)
        {
            var id = surrogate.Content;
            if (registry(id, out var slot))
            {
                return slot;
            }
            throw new SurrogateResolverException($"Unable to resolve surrogate value {id} for type EquipmentSlot");
        }

        public override SurrogateContainer<string> GetObjectToSerialize(EquipmentSlot obj)
        {
            return new SurrogateContainer<string>(obj.Id);
        }
    }
}