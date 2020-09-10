﻿using EnTTSharp.Entities;
using RogueEntity.Core.Meta.Items;

namespace RogueEntity.Core.Tests.Meta.Items
{
    public class BasicItemContext : IItemContext<BasicItemContext, ItemReference>
    {
        public BasicItemContext()
        {

            EntityRegistry = new EntityRegistry<ItemReference>(ItemReference.MaxAge, ItemReference.FromReferencedItem);
            ItemRegistry = new ItemRegistry<BasicItemContext, ItemReference>(ItemReference.BulkItemFactoryMethod);
            ItemResolver = new ItemResolver<BasicItemContext, ItemReference>(ItemRegistry, EntityRegistry);
        }

        public ItemRegistry<BasicItemContext, ItemReference> ItemRegistry { get; }
        public EntityRegistry<ItemReference> EntityRegistry { get; }
        public IItemResolver<BasicItemContext, ItemReference> ItemResolver { get; }
    }
}