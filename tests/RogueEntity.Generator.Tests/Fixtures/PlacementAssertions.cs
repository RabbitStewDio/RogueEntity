using EnTTSharp.Entities;
using FluentAssertions;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.Meta.ItemTraits;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Grid;

namespace RogueEntity.Generator.Tests.Fixtures
{
    public class PlacementAssertions<TItemFixture, TEntityId>
        where TItemFixture : IItemFixture<TEntityId>
        where TEntityId : IEntityKey
    {
        readonly TItemFixture fixture;
        readonly Position position;

        public PlacementAssertions(TItemFixture fixture, Position position)
        {
            this.fixture = fixture;
            this.position = position;
        }

        public PlacementAssertions<TItemFixture, TEntityId> ContainEntity(TEntityId r)
        {
            fixture.ItemMapContext.TryGetGridDataFor(position.LayerId, out var data).Should().BeTrue();
            data.TryGetView(position.GridZ, out var view).Should().BeTrue();
            view[position.GridX, position.GridY].Should().Be(r);

            if (fixture.ItemResolver.EntityMetaData.IsReferenceEntity(r))
            {
                fixture.ItemResolver.TryQueryData(r, out EntityGridPosition entityPositionFromComponent).Should().BeTrue();
                entityPositionFromComponent.Should().BeEquivalentTo(EntityGridPosition.From(position));
            }

            return this;
        }

        public PlacementAssertions<TItemFixture, TEntityId> BeEmpty()
        {
            return ContainEntity(default);
        }

        public PlacementAssertions<TItemFixture, TEntityId> ContainEntityOfType(ItemDeclarationId id)
        {
            fixture.ItemMapContext.TryGetGridDataFor(position.LayerId, out var data).Should().BeTrue();
            data.TryGetView(position.GridZ, out var view).Should().BeTrue();
            var entity = view[position.GridX, position.GridY];
            fixture.ItemResolver.TryResolve(entity, out var itemDeclaration).Should().BeTrue($"for entity {entity}");
            itemDeclaration.Id.Should().Be(id);
            return this;
        }

        public PlacementAssertions<TItemFixture, TEntityId> WithStackSize(int expectedStackCount)
        {
            fixture.ItemMapContext.TryGetGridDataFor(position.LayerId, out var data).Should().BeTrue();
            data.TryGetView(position.GridZ, out var view).Should().BeTrue();
            var entity = view[position.GridX, position.GridY];
            var stack = fixture.ItemResolver.QueryStackSize(entity);
            stack.Count.Should().Be(expectedStackCount);
            return this;
        }

        public TEntityId Fetch()
        {
            fixture.ItemMapContext.TryGetGridDataFor(position.LayerId, out var data).Should().BeTrue();
            data.TryGetView(position.GridZ, out var view).Should().BeTrue();
            return view[position.GridX, position.GridY];
        }

        public ItemAssertions<TItemFixture, TEntityId> AndThatItem()
        {
            var item = Fetch();
            return new ItemAssertions<TItemFixture, TEntityId>(fixture, item);
        }
        
        public PlacementAssertions<TItemFixture, TEntityId> Should()
        {
            return this;
        }
    }
}
