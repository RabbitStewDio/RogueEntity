using FluentAssertions;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.Meta.EntityKeys;
using RogueEntity.Core.Meta.ItemTraits;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Grid;

namespace RogueEntity.Core.Tests.Fixtures
{
    public class PlacementAssertions<TItemFixture>
        where TItemFixture: IItemFixture
    {
        readonly TItemFixture fixture;
        readonly Position position;

        public PlacementAssertions(TItemFixture fixture, Position position)
        {
            this.fixture = fixture;
            this.position = position;
        }

        public PlacementAssertions<TItemFixture> ContainEntity(ItemReference r)
        {
            fixture.ItemMapContext.TryGetGridDataFor(position.LayerId, out var data).Should().BeTrue();
            data.TryGetView(position.GridZ, out var view).Should().BeTrue();
            view[position.GridX, position.GridY].Should().Be(r);

            if (r.IsReference)
            {
                fixture.ItemResolver.TryQueryData(r, out EntityGridPosition entityPositionFromComponent).Should().BeTrue();
                entityPositionFromComponent.Should().BeEquivalentTo(EntityGridPosition.From(position));
            }
            return this;
        }

        public PlacementAssertions<TItemFixture> BeEmpty()
        {
            return ContainEntity(default);
        }

        public PlacementAssertions<TItemFixture> ContainEntityOfType(ItemDeclarationId id)
        {
            fixture.ItemMapContext.TryGetGridDataFor(position.LayerId, out var data).Should().BeTrue();
            data.TryGetView(position.GridZ, out var view).Should().BeTrue();
            var entity = view[position.GridX, position.GridY];
            fixture.ItemResolver.TryResolve(entity, out var itemDeclaration).Should().BeTrue();
            itemDeclaration.Id.Should().Be(id);
            return this;
        }

        public PlacementAssertions<TItemFixture> WithStackSize(int expectedStackCount)
        {
            fixture.ItemMapContext.TryGetGridDataFor(position.LayerId, out var data).Should().BeTrue();
            data.TryGetView(position.GridZ, out var view).Should().BeTrue();
            var entity = view[position.GridX, position.GridY];
            var stack = fixture.ItemResolver.QueryStackSize(entity);
            stack.Count.Should().Be(expectedStackCount);
            return this;
        }

        public PlacementAssertions<TItemFixture> Should()
        {
            return this;
        }
    }
}
