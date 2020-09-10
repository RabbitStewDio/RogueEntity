using EnTTSharp.Serialization;
using MessagePack;
using MessagePack.Formatters;

namespace RogueEntity.Core.Meta.Items
{
    public class ItemReferenceMessagePackFormatter: IMessagePackFormatter<ItemReference>
    {
        readonly EntityKeyMapper<ItemReference> entityKeyMapper;

        public ItemReferenceMessagePackFormatter(EntityKeyMapper<ItemReference> entityKeyMapper)
        {
            this.entityKeyMapper = entityKeyMapper;
        }

        public void Serialize(ref MessagePackWriter writer, ItemReference value, MessagePackSerializerOptions options)
        {
            if (value.IsReference)
            {
                writer.Write(true);
                writer.Write(value.Age);
                writer.Write(value.Key);
            }
            else
            {
                writer.Write(false);
                writer.Write(value.BulkItemId);
                writer.Write(value.Data);
            }
        }

        public ItemReference Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            if (reader.ReadBoolean())
            {
                var age = reader.ReadByte();
                var key = reader.ReadInt32();
                return entityKeyMapper(new EntityKeyData(age, key));
            }

            var itemId = reader.ReadInt32();
            var data = reader.ReadInt32();
            return ItemReference.FromBulkItem((short) itemId, (ushort) data);
        }
    }
}