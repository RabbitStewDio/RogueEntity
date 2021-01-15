using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.Meta.EntityKeys;
using RogueEntity.Core.Meta.Items;

namespace RogueEntity.Core.Tests.Equipment
{
    public class EquipmentTestContext : IItemContext<ItemReference>,
                                        IItemContext<ActorReference>
    {
        readonly ItemContextBackend<ItemReference> items;
        readonly ItemContextBackend<ActorReference> actors;

        public EquipmentTestContext()
        {
            items = new ItemContextBackend<ItemReference>(new ItemReferenceMetaData());
            actors = new ItemContextBackend<ActorReference>(new ActorReferenceMetaData());
        }

        public EntityRegistry<ItemReference> ItemEntities => items.EntityRegistry;
        public IItemResolver<ItemReference> ItemResolver => items.ItemResolver;
        public IItemRegistryBackend<ItemReference> ItemRegistry => items.ItemRegistry;

        public EntityRegistry<ActorReference> ActorEntities => actors.EntityRegistry;
        public IItemResolver<ActorReference> ActorResolver => actors.ItemResolver;
        public IItemRegistryBackend<ActorReference> ActorRegistry => actors.ItemRegistry;

        IItemResolver<ItemReference> IItemContext<ItemReference>.ItemResolver => ItemResolver;
        IItemResolver<ActorReference> IItemContext<ActorReference>.ItemResolver => ActorResolver;
    }
}