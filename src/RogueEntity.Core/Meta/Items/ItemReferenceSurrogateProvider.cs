using EnTTSharp.Serialization;
using EnTTSharp.Serialization.Xml;

namespace RogueEntity.Core.Meta.Items
{
    public class ItemReferenceSurrogateProvider: SerializationSurrogateProviderBase<ItemReference, BulkKeyData>
    {
        readonly EntityKeyMapper<ItemReference> mapper;

        public ItemReferenceSurrogateProvider(EntityKeyMapper<ItemReference> mapper)
        {
            this.mapper = mapper ?? Map;
        }

        ItemReference Map(EntityKeyData data)
        {
            return ItemReference.FromReferencedItem(data.Age, data.Key);
        }

        public override ItemReference GetDeserializedObject(BulkKeyData surrogate)
        {
            if (surrogate.IsReference)
            {
                return mapper(new EntityKeyData(surrogate.Age, surrogate.ItemId));
            }

            return ItemReference.FromBulkItem((short) surrogate.ItemId, (ushort) surrogate.Data);
        }

        public override BulkKeyData GetObjectToSerialize(ItemReference obj)
        {
            if (obj.IsReference)
            {
                return new BulkKeyData(true, obj.Key, obj.Age, 0);
            }

            return new BulkKeyData(false, obj.BulkItemId, 0, obj.Data);
        }
    }
}