using System;

namespace RogueEntity.Api.ItemTraits
{
    public interface IBulkDataStorageMetaData<TItemId>
    {
        public int MaxAge { get; }
        public int MaxBulkKeyTypes { get; }
        TItemId CreateReferenceKey(byte age, int entityId);
        bool TryCreateBulkKey(int id, int data, out TItemId key);
        bool IsReferenceEntity(in TItemId targetItem);
        bool TryDeconstructBulkKey(in TItemId id, out int entityId, out int payload);
    }

    public static class BulkDataStorageEntityKeyExtensions
    {
        [Obsolete]
        public static bool IsSameBulkType<TItemId>(this IBulkDataStorageMetaData<TItemId> meta,
                                                   TItemId a,
                                                   TItemId b)
            where TItemId: struct, IBulkDataStorageKey<TItemId>
        {
            if (a.IsEmpty || b.IsEmpty) return false;
            if (a.IsReference || b.IsReference) return false;
            return a.BulkItemId == b.BulkItemId;
        }

    }
}