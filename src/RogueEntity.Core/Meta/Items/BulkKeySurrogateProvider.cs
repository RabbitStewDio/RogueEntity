using EnTTSharp.Entities;
using EnTTSharp.Serialization;
using EnTTSharp.Serialization.Xml;
using RogueEntity.Api.ItemTraits;
using System;

namespace RogueEntity.Core.Meta.Items
{
    public class BulkKeySurrogateProvider<TItemId> : SerializationSurrogateProviderBase<TItemId, BulkKeyData>
        where TItemId : struct, IEntityKey
    {
        readonly IEntityKeyMapper mapper;
        readonly IBulkDataStorageMetaData<TItemId> metaData;
        readonly BulkItemSerializationMapperDelegate<TItemId> bulkMapper;

        public BulkKeySurrogateProvider(IBulkDataStorageMetaData<TItemId> metaData,
                                        IEntityKeyMapper? mapper,
                                        BulkItemSerializationMapperDelegate<TItemId> bulkMapper)
        {
            this.metaData = metaData ?? throw new ArgumentNullException(nameof(metaData));
            this.bulkMapper = bulkMapper ?? throw new ArgumentNullException(nameof(bulkMapper));
            this.mapper = mapper ?? new DefaultEntityKeyMapper().Register(Map);
        }

        TItemId Map(EntityKeyData data)
        {
            return metaData.CreateReferenceKey(data.Age, data.Key);
        }

        public override TItemId GetDeserializedObject(BulkKeyData surrogate)
        {
            if (surrogate.IsReference)
            {
                return mapper.EntityKeyMapper<TItemId>(new EntityKeyData(surrogate.Age, surrogate.ItemId));
            }

            if (metaData.TryCreateBulkKey(surrogate.ItemId, surrogate.Data, out var tmp) && 
                bulkMapper(tmp, out var result))
            {
                return result;
            }

            throw new SurrogateResolverException();
        }

        public override BulkKeyData GetObjectToSerialize(TItemId obj)
        {
            if (!metaData.TryDeconstructBulkKey(in obj, out var bulkId, out var bulkData))
            {
                return new BulkKeyData(true, obj.Key, obj.Age, 0);
            }

            return new BulkKeyData(false, bulkId, 0, bulkData);
        }
    }
}