using System;
using System.Collections.Generic;
using RogueEntity.Core.Infrastructure.ItemTraits;

namespace RogueEntity.Core.Meta.Items
{
    public delegate bool BulkItemSerializationMapperDelegate<TItemId>(TItemId remoteId, out TItemId localId) where TItemId : IBulkDataStorageKey<TItemId>;
    
    public class BulkItemIdSerializationMapper<TItemId> where TItemId : IBulkDataStorageKey<TItemId>
    {
        readonly Func<int, TItemId> bulkKeyFactory;
        readonly IBulkItemIdMapping localMapper;
        readonly Dictionary<int, int> remoteMapping;

        public BulkItemIdSerializationMapper(Func<int, TItemId> bulkKeyFactory,
                                             IBulkItemIdMapping localMapper,
                                             IBulkItemIdMapping remoteMapper)
        {
            this.bulkKeyFactory = bulkKeyFactory;
            this.localMapper = localMapper;
            this.remoteMapping = BuildRemoteMapping(remoteMapper);
        }

        Dictionary<int, int> BuildRemoteMapping(IBulkItemIdMapping remoteMapper)
        {
            var buildRemoteMapping = new Dictionary<int, int>();
            var reverseMapping = BuildReverseMapping(localMapper);
            foreach (var remoteId in remoteMapper)
            {
                if (!remoteMapper.TryResolveBulkItem(remoteId, out var remoteType) ||
                    !reverseMapping.TryGetValue(remoteType, out var localId))
                {
                    throw new Exception($"Incomplete mapping for remote id {remoteId}");
                }

                buildRemoteMapping.Add(remoteId, localId);
            }

            return buildRemoteMapping;
        }

        static Dictionary<ItemDeclarationId, int> BuildReverseMapping(IBulkItemIdMapping m)
        {
            var retval = new Dictionary<ItemDeclarationId, int>();
            foreach (var id in m)
            {
                if (m.TryResolveBulkItem(id, out var v))
                {
                    retval.Add(v, id);
                }
            }

            return retval;
        }

        public bool TryMap(TItemId remoteId, out TItemId localId)
        {
            if (remoteMapping.TryGetValue(remoteId.BulkItemId, out var localItemId))
            {
                localId = bulkKeyFactory(localItemId).WithData(remoteId.Data);
                return true;
            }

            localId = default;
            return false;
        }
    }
}