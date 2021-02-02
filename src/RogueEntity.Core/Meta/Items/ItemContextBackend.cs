using System;
using EnTTSharp.Entities;
using JetBrains.Annotations;
using RogueEntity.Api.ItemTraits;

namespace RogueEntity.Core.Meta.Items
{
    public class ItemContextBackend<TItemId>: IItemContextBackend<TItemId>
        where TItemId : IEntityKey
    {
        public ItemContextBackend([NotNull] IBulkDataStorageMetaData<TItemId> meta)
        {
            EntityMetaData = meta ?? throw new ArgumentNullException(nameof(meta));
            ItemRegistry = new ItemRegistry<TItemId>(meta);
            EntityRegistry = new EntityRegistry<TItemId>(meta.MaxAge, meta.CreateReferenceKey);
            ItemResolver = new ItemResolver<TItemId>(ItemRegistry, EntityRegistry);
        }

        public IBulkDataStorageMetaData<TItemId> EntityMetaData { get; }
        public ItemRegistry<TItemId> ItemRegistry { get; }
        IItemRegistryBackend<TItemId> IItemContextBackend<TItemId>.ItemRegistry => ItemRegistry;
        public EntityRegistry<TItemId> EntityRegistry { get; }
        public IItemResolver<TItemId> ItemResolver { get; }
    }
}