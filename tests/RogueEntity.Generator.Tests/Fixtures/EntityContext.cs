using EnTTSharp.Entities;
using FluentAssertions;
using FluentAssertions.Execution;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Meta.ItemTraits;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Utils.DataViews;

namespace RogueEntity.Generator.Tests.Fixtures
{
    public readonly struct EntityContext<TItemFixture, TEntityId> where TItemFixture: IItemFixture<TEntityId>
                                                                  where TEntityId : IEntityKey
    {
        public readonly TItemFixture Context;
        public readonly TEntityId Item;

        public EntityContext(TItemFixture context, ItemDeclarationId item)
        {
            this.Context = context;
            this.Item = this.Context.ItemResolver.Instantiate(item);
        }

        public EntityContext(TItemFixture context, TEntityId r = default)
        {
            this.Context = context;
            this.Item = r;
        }

        public EntityContext<TItemFixture, TEntityId> WithStackSize(int stackSize)
        {
            var stack = Context.ItemResolver.QueryStackSize(Item);
            if (Context.ItemResolver.TryUpdateData(Item, stack.WithCount(stackSize), out var changedKey))
            {
                return new EntityContext<TItemFixture, TEntityId>(Context, changedKey);
            }
                
            throw new AssertionFailedException($"Item {Item} is not stackable or stack size does not fit requested number of items");
        }

        public TEntityId InstantiatedWithoutPosition()
        {
            return Item;
        }
            
        public (TEntityId, Position) IsPlacedAt(Position p)
        {
            Context.ItemMapContext.TryGetGridDataFor(p.LayerId, out var data).Should().BeTrue();
            if (Context.ItemResolver.EntityMetaData.IsReferenceEntity(Item))
            {
                // bypass the GridPlacementContext here
                Context.ItemResolver.TryUpdateData(Item, new EntityGridPositionUpdateMessage(EntityGridPosition.From(p)), out _).Should().BeTrue();
            }

            data.TryGetWritableView(p.GridZ, out var view, DataViewCreateMode.CreateMissing).Should().BeTrue();
            view[p.GridX, p.GridY] = Item;
            return (Item, p);
        }
    }
}
