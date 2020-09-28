using System;
using EnTTSharp.Serialization;
using MessagePack;
using MessagePack.Formatters;

namespace RogueEntity.Core.Meta.Items
{
    public class BulkKeyMessagePackFormatter<TItemId> : IMessagePackFormatter<TItemId> 
        where TItemId: IBulkDataStorageKey<TItemId>
    {
        readonly IBulkDataStorageMetaData<TItemId> metaData;
        readonly EntityKeyMapper<TItemId> entityKeyMapper;
        readonly BulkItemSerializationMapperDelegate<TItemId> bulkIdMapper;
        
        public BulkKeyMessagePackFormatter(IBulkDataStorageMetaData<TItemId> metaData,
                                           EntityKeyMapper<TItemId> entityKeyMapper, 
                                           BulkItemSerializationMapperDelegate<TItemId> bulkIdMapper)
        {
            this.metaData = metaData ?? throw new ArgumentNullException(nameof(metaData));
            this.entityKeyMapper = entityKeyMapper ?? throw new ArgumentNullException(nameof(entityKeyMapper));
            this.bulkIdMapper = bulkIdMapper ?? throw new ArgumentNullException(nameof(bulkIdMapper));
        }

        public void Serialize(ref MessagePackWriter writer, TItemId value, MessagePackSerializerOptions options)
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

        public TItemId Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            if (reader.ReadBoolean())
            {
                var age = reader.ReadByte();
                var key = reader.ReadInt32();
                return entityKeyMapper(new EntityKeyData(age, key));
            }

            var itemId = reader.ReadInt32();
            var data = reader.ReadInt32();
            var tmp = metaData.BulkDataFactory(itemId).WithData(data);
            if (bulkIdMapper(tmp, out var result))
            {
                return result;
            }

            Console.WriteLine($"{typeof(TItemId)} - {tmp}");
            throw new MessagePackSerializationException($"Unable to map ItemReference {tmp} to local bulk item id");
        }
    }
}