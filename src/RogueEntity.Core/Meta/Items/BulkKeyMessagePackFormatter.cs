using System;
using EnTTSharp.Entities;
using EnTTSharp.Serialization;
using MessagePack;
using MessagePack.Formatters;

namespace RogueEntity.Core.Meta.Items
{
    public class BulkKeyMessagePackFormatter<TItemId> : IMessagePackFormatter<TItemId> 
        where TItemId: IEntityKey
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
            if (!metaData.TryDeconstructBulkKey(value, out var bulkId, out var bulkData))
            {
                writer.Write(true);
                writer.Write(value.Age);
                writer.Write(value.Key);
            }
            else
            {
                writer.Write(false);
                writer.Write(bulkId);
                writer.Write(bulkData);
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
            if (metaData.TryCreateBulkKey(itemId, data, out var tmp) &&
                bulkIdMapper(tmp, out var result))
            {
                return result;
            }

            throw new MessagePackSerializationException($"Unable to map entity type {typeof(TItemId)} with data {itemId}:{data} to local bulk item id");
        }
    }
}