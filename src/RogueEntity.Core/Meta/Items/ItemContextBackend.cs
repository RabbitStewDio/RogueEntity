using EnTTSharp.Entities;

namespace RogueEntity.Core.Meta.Items
{
    public class ItemContextBackend<TGameContext, TItemId>: IItemContext<TGameContext, TItemId>
        where TItemId : IBulkDataStorageKey<TItemId>
    {
        public ItemContextBackend(IBulkDataStorageMetaData<TItemId> meta)
        {
            EntityRegistry = new EntityRegistry<TItemId>(meta.MaxAge, meta.EntityKeyFactory);
            ItemRegistry = new ItemRegistry<TGameContext, TItemId>(meta.BulkDataFactory);
            ItemResolver = new ItemResolver<TGameContext, TItemId>(ItemRegistry, EntityRegistry);
        }

        public ItemRegistry<TGameContext, TItemId> ItemRegistry { get; }
        public EntityRegistry<TItemId> EntityRegistry { get; }
        public IItemResolver<TGameContext, TItemId> ItemResolver { get; }

    }
}