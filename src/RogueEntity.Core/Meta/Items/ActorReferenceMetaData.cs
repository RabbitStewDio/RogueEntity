namespace RogueEntity.Core.Meta.Items
{
    public class ActorReferenceMetaData : IBulkDataStorageMetaData<ActorReference>
    {
        public int MaxAge => ActorReference.MaxAge;
        public bool IsSameBulkType(ActorReference a, ActorReference b) => EntityKeyMetaData.IsSameBulkType(a, b);
        public bool IsReferenceEntity(in ActorReference targetItem) => targetItem.IsReference;

        public ActorReference CreateReferenceKey(byte age, int entityId) => ActorReference.FromReferencedItem(age, entityId);

        public bool TryCreateBulkKey(int id, int data, out ActorReference key)
        {
            key = ActorReference.BulkItemFactoryMethod(id).WithData(data);
            return true;
        }

        public bool TryDeconstructBulkKey(in ActorReference id, out int entityId, out int payload)
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
}