using EnTTSharp.Serialization;
using EnTTSharp.Serialization.Xml;

namespace RogueEntity.Core.Meta.Items
{
    public class BulkKeySurrogateProvider<TItemId> : SerializationSurrogateProviderBase<TItemId, BulkKeyData>
        where TItemId : IBulkDataStorageKey<TItemId>
    {
        readonly EntityKeyMapper<TItemId> mapper;
        readonly IBulkDataStorageMetaData<TItemId> metaData;
        readonly BulkItemSerializationMapperDelegate<TItemId> bulkMapper;

        public BulkKeySurrogateProvider(IBulkDataStorageMetaData<TItemId> metaData,
                                        EntityKeyMapper<TItemId> mapper,
                                        BulkItemSerializationMapperDelegate<TItemId> bulkMapper)
        {
            this.metaData = metaData;
            this.bulkMapper = bulkMapper;
            this.mapper = mapper ?? Map;
        }

        TItemId Map(EntityKeyData data)
        {
            return metaData.EntityKeyFactory(data.Age, data.Key);
        }

        public override TItemId GetDeserializedObject(BulkKeyData surrogate)
        {
            if (surrogate.IsReference)
            {
                return mapper(new EntityKeyData(surrogate.Age, surrogate.ItemId));
            }

            var tmp = metaData.BulkDataFactory(surrogate.ItemId).WithData(surrogate.Data);
            if (bulkMapper(tmp, out var result))
            {
                return result;
            }

            throw new SurrogateResolverException();
        }

        public override BulkKeyData GetObjectToSerialize(TItemId obj)
        {
            if (obj.IsReference)
            {
                return new BulkKeyData(true, obj.Key, obj.Age, 0);
            }

            return new BulkKeyData(false, obj.BulkItemId, 0, obj.Data);
        }
    }
}