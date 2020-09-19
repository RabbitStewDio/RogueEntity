using System;

namespace RogueEntity.Core.Meta.Items
{
    public class ItemReferenceMetaData : IBulkDataStorageMetaData<ItemReference>
    {
        public int MaxAge => ItemReference.MaxAge;
        public Func<byte, int, ItemReference> EntityKeyFactory => ItemReference.FromReferencedItem;
        public Func<int, ItemReference> BulkDataFactory => ItemReference.BulkItemFactoryMethod;
    }
}