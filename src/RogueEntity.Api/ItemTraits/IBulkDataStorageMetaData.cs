using EnTTSharp.Entities;

namespace RogueEntity.Api.ItemTraits
{
    public interface IBulkDataStorageMetaData<TItemId> where TItemId : IEntityKey
    {
        public int MaxAge { get; }
        TItemId CreateReferenceKey(byte age, int entityId);
        bool TryCreateBulkKey(int id, int data, out TItemId key);
        bool IsSameBulkType(TItemId a, TItemId b);
        bool IsReferenceEntity(in TItemId targetItem);
        bool TryDeconstructBulkKey(in TItemId id, out int entityId, out int payload);
    }
}