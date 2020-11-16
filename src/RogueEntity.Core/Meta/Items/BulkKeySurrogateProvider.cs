﻿using EnTTSharp.Entities;
using EnTTSharp.Serialization;
using EnTTSharp.Serialization.Xml;
using RogueEntity.Api.ItemTraits;

namespace RogueEntity.Core.Meta.Items
{
    public class BulkKeySurrogateProvider<TItemId> : SerializationSurrogateProviderBase<TItemId, BulkKeyData>
        where TItemId : IEntityKey
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
            return metaData.CreateReferenceKey(data.Age, data.Key);
        }

        public override TItemId GetDeserializedObject(BulkKeyData surrogate)
        {
            if (surrogate.IsReference)
            {
                return mapper(new EntityKeyData(surrogate.Age, surrogate.ItemId));
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