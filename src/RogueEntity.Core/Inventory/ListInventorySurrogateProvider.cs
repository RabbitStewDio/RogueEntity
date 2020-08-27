using System;
using EnTTSharp.Entities;
using EnTTSharp.Serialization.Xml;
using RogueEntity.Core.Meta.Items;

namespace RogueEntity.Core.Inventory
{
    public class ListInventorySurrogateProvider<TGameContext, TOwnerId, TItemId> :
        SerializationSurrogateProviderBase<ListInventory<TGameContext, TOwnerId, TItemId>, ListInventoryData<TOwnerId, TItemId>>
        where TItemId : IBulkDataStorageKey<TItemId>
        where TOwnerId : IEntityKey
    {
        readonly IItemResolver<TGameContext, TOwnerId> ownerResolver;
        readonly IItemResolver<TGameContext, TItemId> itemResolver;

        public ListInventorySurrogateProvider(IItemResolver<TGameContext, TOwnerId> ownerResolver,
                                              IItemResolver<TGameContext, TItemId> itemResolver)
        {
            this.ownerResolver = ownerResolver ?? throw new ArgumentNullException(nameof(ownerResolver));
            this.itemResolver = itemResolver ?? throw new ArgumentNullException(nameof(itemResolver));
        }

        public override ListInventory<TGameContext, TOwnerId, TItemId> GetDeserializedObject(ListInventoryData<TOwnerId, TItemId> surrogate)
        {
            return new ListInventory<TGameContext, TOwnerId, TItemId>(ownerResolver, itemResolver, surrogate);
        }

        public override ListInventoryData<TOwnerId, TItemId> GetObjectToSerialize(ListInventory<TGameContext, TOwnerId, TItemId> obj)
        {
            return obj.Data;
        }
    }
}