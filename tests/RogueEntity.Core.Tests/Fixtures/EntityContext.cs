using FluentAssertions;
using FluentAssertions.Execution;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.Meta.EntityKeys;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Meta.ItemTraits;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Utils.DataViews;

namespace RogueEntity.Core.Tests.Fixtures
{
    public readonly struct EntityContext<TItemFixture> where TItemFixture: IItemFixture
    {
        public readonly TItemFixture Context;
        public readonly ItemReference Item;

        public EntityContext(TItemFixture context, ItemDeclarationId item)
        {
            this.Context = context;
            this.Item = this.Context.ItemResolver.Instantiate(item);
        }

        public EntityContext(TItemFixture context, ItemReference r = default)
        {
            this.Context = context;
            this.Item = r;
        }

        public EntityContext<TItemFixture> WithStackSize(int stackSize)
        {
            var stack = Context.ItemResolver.QueryStackSize(Item);
            if (Context.ItemResolver.TryUpdateData(Item, stack.WithCount(stackSize), out var changedKey))
            {
                return new EntityContext<TItemFixture>(Context, changedKey);
            }
                
            throw new AssertionFailedException($"Item {Item} is not stackable or stack size does not fit requested number of items");
        }

        public ItemReference InstantiatedWithoutPosition()
        {
            return Item;
        }
            
        public (ItemReference, Position) IsPlacedAt(Position p)
        {
            Context.ItemMapContext.TryGetGridDataFor(p.LayerId, out var data).Should().BeTrue();
            if (Item.IsReference)
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
