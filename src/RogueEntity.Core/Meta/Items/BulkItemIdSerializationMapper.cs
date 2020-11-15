using System;
using System.Collections.Generic;
using EnTTSharp.Entities;
using JetBrains.Annotations;
using RogueEntity.Core.Infrastructure.ItemTraits;

namespace RogueEntity.Core.Meta.Items
{
    public delegate bool BulkItemSerializationMapperDelegate<TItemId>(in TItemId remoteId, out TItemId localId)
        where TItemId : IEntityKey;
    
    public class BulkItemIdSerializationMapper<TItemId> where TItemId : IEntityKey
    {
        readonly IBulkDataStorageMetaData<TItemId> metaData;
        readonly IBulkItemIdMapping localMapper;
        readonly Dictionary<int, int> remoteMapping;

        public BulkItemIdSerializationMapper([NotNull] IBulkDataStorageMetaData<TItemId> metaData,
                                             [NotNull] IBulkItemIdMapping localMapper,
                                             [NotNull] IBulkItemIdMapping remoteMapper)
        {
            if (remoteMapper == null)
            {
                throw new ArgumentNullException(nameof(remoteMapper));
            }

            this.metaData = metaData ?? throw new ArgumentNullException(nameof(metaData));
            this.localMapper = localMapper ?? throw new ArgumentNullException(nameof(localMapper));
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

        public bool TryMap(in TItemId remoteId, out TItemId localId)
        {
            if (metaData.TryDeconstructBulkKey(in remoteId, out var bulkItemId, out var payload) &&
                remoteMapping.TryGetValue(bulkItemId, out var localItemId) &&
                metaData.TryCreateBulkKey(localItemId, payload, out localId))
            {
                return true;
            }

            localId = default;
            return false;
        }
    }
}