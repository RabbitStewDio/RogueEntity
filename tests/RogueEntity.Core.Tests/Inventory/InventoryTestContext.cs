using EnTTSharp.Entities;
using RogueEntity.Core.Meta.Items;

namespace RogueEntity.Core.Tests.Inventory
{
    public class InventoryTestContext : IItemContext<InventoryTestContext, ItemReference>,
                                        IItemContext<InventoryTestContext, ActorReference>
    {
        readonly ItemContextBackend<InventoryTestContext, ItemReference> items;
        readonly ItemContextBackend<InventoryTestContext, ActorReference> actors;

        public InventoryTestContext()
        {
            items = new ItemContextBackend<InventoryTestContext, ItemReference>(new ItemReferenceMetaData());
            actors = new ItemContextBackend<InventoryTestContext, ActorReference>(new ActorReferenceMetaData());
        }

        public EntityRegistry<ItemReference> ItemEntities => items.EntityRegistry;
        public IItemResolver<InventoryTestContext, ItemReference> ItemResolver => items.ItemResolver;
        public IItemRegistryBackend<InventoryTestContext, ItemReference> ItemRegistry => items.ItemRegistry;

        public EntityRegistry<ActorReference> ActorEntities => actors.EntityRegistry;
        public IItemResolver<InventoryTestContext, ActorReference> ActorResolver => actors.ItemResolver;
        public IItemRegistryBackend<InventoryTestContext, ActorReference> ActorRegistry => actors.ItemRegistry;

        IItemResolver<InventoryTestContext, ItemReference> IItemContext<InventoryTestContext, ItemReference>.ItemResolver => ItemResolver;
        IItemResolver<InventoryTestContext, ActorReference> IItemContext<InventoryTestContext, ActorReference>.ItemResolver => ActorResolver;
    }
}