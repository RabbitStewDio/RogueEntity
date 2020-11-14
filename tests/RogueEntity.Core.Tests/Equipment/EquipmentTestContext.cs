using EnTTSharp.Entities;
using RogueEntity.Core.Infrastructure.ItemTraits;
using RogueEntity.Core.Meta.Items;

namespace RogueEntity.Core.Tests.Equipment
{
    public class EquipmentTestContext : IItemContext<EquipmentTestContext, ItemReference>,
                                        IItemContext<EquipmentTestContext, ActorReference>
    {
        readonly ItemContextBackend<EquipmentTestContext, ItemReference> items;
        readonly ItemContextBackend<EquipmentTestContext, ActorReference> actors;

        public EquipmentTestContext()
        {
            items = new ItemContextBackend<EquipmentTestContext, ItemReference>(new ItemReferenceMetaData());
            actors = new ItemContextBackend<EquipmentTestContext, ActorReference>(new ActorReferenceMetaData());
        }

        public EntityRegistry<ItemReference> ItemEntities => items.EntityRegistry;
        public IItemResolver<EquipmentTestContext, ItemReference> ItemResolver => items.ItemResolver;
        public IItemRegistryBackend<EquipmentTestContext, ItemReference> ItemRegistry => items.ItemRegistry;

        public EntityRegistry<ActorReference> ActorEntities => actors.EntityRegistry;
        public IItemResolver<EquipmentTestContext, ActorReference> ActorResolver => actors.ItemResolver;
        public IItemRegistryBackend<EquipmentTestContext, ActorReference> ActorRegistry => actors.ItemRegistry;

        IItemResolver<EquipmentTestContext, ItemReference> IItemContext<EquipmentTestContext, ItemReference>.ItemResolver => ItemResolver;
        IItemResolver<EquipmentTestContext, ActorReference> IItemContext<EquipmentTestContext, ActorReference>.ItemResolver => ActorResolver;
    }
}