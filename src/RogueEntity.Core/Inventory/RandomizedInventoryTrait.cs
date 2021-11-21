using System.Collections.Generic;
using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Infrastructure.Randomness;
using RogueEntity.Core.Meta.ItemBuilder;

namespace RogueEntity.Core.Inventory
{
    public sealed class RandomizedInventoryTrait< TOwnerId, TItemId> : IReferenceItemTrait< TOwnerId>
        where TOwnerId : IEntityKey, IRandomSeedSource
        where TItemId : IEntityKey
    {
        readonly IEntityRandomGeneratorSource randomGenerator;
        readonly IItemResolver< TOwnerId> ownerResolver;
        readonly IItemResolver< TItemId> itemResolver;
        readonly ReadOnlyListWrapper<InventoryLootEntry> itemPool;

        public RandomizedInventoryTrait(IItemResolver< TOwnerId> ownerResolver,
                                        IItemResolver< TItemId> itemResolver,
                                        IEntityRandomGeneratorSource randomGenerator,
                                        params InventoryLootEntry[] items)
        {
            this.ownerResolver = ownerResolver;
            this.itemResolver = itemResolver;
            this.randomGenerator = randomGenerator;
            this.itemPool = new List<InventoryLootEntry>(items);
        }

        public ItemTraitId Id => "Core.Inventory.RandomContent";
        public int Priority => 10000;

        public IReferenceItemTrait< TOwnerId> CreateInstance()
        {
            return this;
        }

        public void Initialize(IEntityViewControl<TOwnerId> v, TOwnerId k, IItemDeclaration actor)
        {
            if (!ownerResolver.TryQueryData(k, out IInventory< TItemId> inventory))
            {
                return;
            }

            var rng = randomGenerator.RandomGenerator(k, 0);
            foreach (var i in itemPool)
            {
                if (rng.Next() >= i.Probability)
                {
                    continue;
                }

                var item = itemResolver.Build(i.Item)
                                       .WithRandomizedProperties(rng)
                                       .ToItemReference;

                if (!inventory.TryAddItem(item, out var remains) ||
                    !remains.IsEmpty)
                {
                    itemResolver.DiscardUnusedItem(remains);
                }
            }

            ownerResolver.TryUpdateData(k, in inventory, out _);
        }

        public void Apply(IEntityViewControl<TOwnerId> v, TOwnerId k, IItemDeclaration item)
        {
        }

        public IEnumerable<EntityRoleInstance> GetEntityRoles()
        {
            yield return InventoryModule.ContainerRole.Instantiate<TOwnerId>();
            yield return InventoryModule.ContainedItemRole.Instantiate<TItemId>();
        }

        public IEnumerable<EntityRelationInstance> GetEntityRelations()
        {
            yield return InventoryModule.ContainsRelation.Instantiate<TOwnerId, TItemId>();
        }
    }
}