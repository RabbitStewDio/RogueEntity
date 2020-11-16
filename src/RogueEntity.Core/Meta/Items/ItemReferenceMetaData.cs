using RogueEntity.Api.ItemTraits;

namespace RogueEntity.Core.Meta.Items
{
    public class ItemReferenceMetaData : IBulkDataStorageMetaData<ItemReference>
    {
        public int MaxAge => ItemReference.MaxAge;
        public bool IsSameBulkType(ItemReference a, ItemReference b) => EntityKeyMetaData.IsSameBulkType(a, b);
        public bool IsReferenceEntity(in ItemReference targetItem) => targetItem.IsReference;
        
        public ItemReference CreateReferenceKey(byte age, int entityId) => ItemReference.FromReferencedItem(age, entityId);

        public bool TryCreateBulkKey(int id, int data, out ItemReference key)
        {
            key = ItemReference.BulkItemFactoryMethod(id).WithData(data);
            return true;
        }

        public bool TryDeconstructBulkKey(in ItemReference id, out int entityId, out int payload)
        {
            if (id.IsReference)
            {
                entityId = default;
                payload = default;
                return false;
            }
            
            entityId = id.BulkItemId;
            payload = id.Data;
            return true;
        }

    }

    public static class EntityKeyMetaData
    {
        public static bool IsSameBulkType<TItemId>(TItemId a, TItemId b)
            where TItemId: IBulkDataStorageKey<TItemId>
        {
            return !a.IsEmpty && !a.IsReference && !b.IsReference && a.BulkItemId == b.BulkItemId;
        }
    }
}