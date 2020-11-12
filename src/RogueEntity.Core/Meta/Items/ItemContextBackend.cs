using EnTTSharp.Entities;

namespace RogueEntity.Core.Meta.Items
{
    public class ItemContextBackend<TGameContext, TItemId>: IItemContextBackend<TGameContext, TItemId>
        where TItemId : IBulkDataStorageKey<TItemId>
    {
        public ItemContextBackend(IBulkDataStorageMetaData<TItemId> meta)
        {
            var itemRegistry = new ItemRegistry<TGameContext, TItemId>(meta.BulkDataFactory);
            ItemRegistry = itemRegistry;
            EntityRegistry = new EntityRegistry<TItemId>(meta.MaxAge, meta.EntityKeyFactory);
            ItemResolver = new ItemResolver<TGameContext, TItemId>(itemRegistry, EntityRegistry);
        }

        public IItemRegistryBackend<TGameContext, TItemId> ItemRegistry { get; }
        public EntityRegistry<TItemId> EntityRegistry { get; }
        public IItemResolver<TGameContext, TItemId> ItemResolver { get; }

    }
}