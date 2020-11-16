using System;
using EnTTSharp.Entities;
using JetBrains.Annotations;
using RogueEntity.Api.ItemTraits;

namespace RogueEntity.Core.Meta.Items
{
    public class ItemContextBackend<TGameContext, TItemId>: IItemContextBackend<TGameContext, TItemId>
        where TItemId : IBulkDataStorageKey<TItemId>
    {
        public ItemContextBackend([NotNull] IBulkDataStorageMetaData<TItemId> meta)
        {
            EntityMetaData = meta ?? throw new ArgumentNullException(nameof(meta));
            ItemRegistry = new ItemRegistry<TGameContext, TItemId>(meta);
            EntityRegistry = new EntityRegistry<TItemId>(meta.MaxAge, meta.CreateReferenceKey);
            ItemResolver = new ItemResolver<TGameContext, TItemId>(ItemRegistry, EntityRegistry);
        }

        public IBulkDataStorageMetaData<TItemId> EntityMetaData { get; }
        public ItemRegistry<TGameContext, TItemId> ItemRegistry { get; }
        IItemRegistryBackend<TGameContext, TItemId> IItemContextBackend<TGameContext, TItemId>.ItemRegistry => ItemRegistry;
        public EntityRegistry<TItemId> EntityRegistry { get; }
        public IItemResolver<TGameContext, TItemId> ItemResolver { get; }

    }
}