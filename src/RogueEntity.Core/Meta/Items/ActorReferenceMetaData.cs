using System;

namespace RogueEntity.Core.Meta.Items
{
    public class ActorReferenceMetaData : IBulkDataStorageMetaData<ActorReference>
    {
        public int MaxAge => ActorReference.MaxAge;
        public Func<byte, int, ActorReference> EntityKeyFactory => ActorReference.FromReferencedItem;
        public Func<int, ActorReference> BulkDataFactory => ActorReference.BulkItemFactoryMethod;
    }
}