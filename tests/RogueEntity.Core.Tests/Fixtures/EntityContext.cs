using FluentAssertions;
using FluentAssertions.Execution;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Meta.ItemTraits;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Utils;
using System;

namespace RogueEntity.Core.Tests.Fixtures
{
    public readonly struct EntityContext<TItemFixture, TEntity> 
        where TItemFixture : IEntityFixture<TEntity> 
        where TEntity : struct, IBulkDataStorageKey<TEntity>
    {
        public readonly TItemFixture Context;
        public readonly TEntity Item;

        public EntityContext(TItemFixture context, ItemDeclarationId item)
        {
            this.Context = context ?? throw new ArgumentNullException();
            this.Item = this.Context.ItemResolver.Instantiate(item);
        }

        public EntityContext(TItemFixture context)
        {
            this.Context = context;
            this.Item = default;
        }

        public EntityContext(TItemFixture context, TEntity r = default)
        {
            this.Context = context;
            this.Item = r;
        }

        public EntityContext<TItemFixture, TEntity> WithStackSize(int stackSize)
        {
            var stack = Context.ItemResolver.QueryStackSize(Item);
            if (Context.ItemResolver.TryUpdateData(Item, stack.WithCount(stackSize), out var changedKey))
            {
                return new EntityContext<TItemFixture, TEntity>(Context, changedKey);
            }
                
            throw new AssertionFailedException($"Item {Item} is not stackable or stack size does not fit requested number of items");
        }

        public TEntity InstantiatedWithoutPosition()
        {
            return Item;
        }
            
        public (TEntity, Position) IsPlacedAt(Position p)
        {
            Context.ItemPlacementContext.TryGetItemPlacementService(p.LayerId, out var data).Should().BeTrue();
            Assert.NotNull(data);
            data.TryPlaceItem(Item, p).Should().BeTrue();
            return (Item, p);
        }
    }
}
