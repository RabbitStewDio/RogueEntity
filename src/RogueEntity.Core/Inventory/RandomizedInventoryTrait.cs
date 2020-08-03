using System.Collections.Generic;
using EnttSharp.Entities;
using RogueEntity.Core.Infrastructure;
using RogueEntity.Core.Infrastructure.Meta.ItemBuilder;
using RogueEntity.Core.Infrastructure.Meta.Items;

namespace RogueEntity.Core.Inventory
{
    public class RandomizedInventoryTrait<TGameContext, TOwnerId, TItemId> : IReferenceItemTrait<TGameContext, TOwnerId>
        where TGameContext : IRandomContext
        where TOwnerId : IEntityKey
        where TItemId : IBulkDataStorageKey<TItemId>
    {
        readonly IItemResolver<TGameContext, TOwnerId> ownerResolver;
        readonly IItemResolver<TGameContext, TItemId> itemResolver;
        readonly List<ItemEntry> itemPool;

        public RandomizedInventoryTrait(IItemResolver<TGameContext, TOwnerId> ownerResolver,
                                        IItemResolver<TGameContext, TItemId> itemResolver,
                                        params ItemEntry[] items)
        {
            this.ownerResolver = ownerResolver;
            this.itemResolver = itemResolver;
            this.itemPool = new List<ItemEntry>(items);
        }

        public string Id => "Core.Inventory.RandomContent";
        public int Priority => 10000;

        public void Initialize(IEntityViewControl<TOwnerId> v, TGameContext context, TOwnerId k, IItemDeclaration actor)
        {
            if (!ownerResolver.TryQueryData(k, context, out IInventory<TGameContext, TItemId> inventory))
            {
                return;
            }

            var rng = context.RandomGenerator(k, 0);
            foreach (var i in itemPool)
            {
                if (rng() >= i.Probability)
                {
                    continue;
                }

                var item = itemResolver.Build(context, i.Item)
                                       .WithRandomizedProperties(rng)
                                       .ToItemReference;

                if (!inventory.TryAddItem(context, item, out var remains) || 
                    !remains.IsEmpty)
                {
                    itemResolver.DiscardUnusedItem(remains);
                }
            }

            ownerResolver.TryUpdateData(k, context, in inventory, out _);
        }

        public void Apply(IEntityViewControl<TOwnerId> v, TGameContext context, TOwnerId k, IItemDeclaration item)
        {
        }

        public readonly struct ItemEntry
        {
            public readonly ItemDeclarationId Item;
            public readonly float Probability;

            public ItemEntry(ItemDeclarationId item, float probability = 0.1f)
            {
                this.Item = item;
                this.Probability = probability;
            }
        }
    }
}