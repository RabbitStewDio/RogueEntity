using FluentAssertions;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.Meta.EntityKeys;
using RogueEntity.Core.Meta.ItemTraits;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Utils;

namespace RogueEntity.Core.Tests.Fixtures
{
    public class PlacementAssertions<TEntity, TItemFixture>
        where TItemFixture : IEntityFixture<TEntity>
        where TEntity : struct, IBulkDataStorageKey<TEntity>
    {
        readonly TItemFixture fixture;
        readonly Position position;

        public PlacementAssertions(TItemFixture fixture, Position position)
        {
            this.fixture = fixture;
            this.position = position;
        }

        public PlacementAssertions<TEntity, TItemFixture> ContainEntity(TEntity r)
        {
            fixture.ItemMapContext.TryGetGridDataFor(position.LayerId, out var data).Should().BeTrue();
            data.TryGetView(position.GridZ, out var view).Should().BeTrue();
            Assert.NotNull(view);
            view.TryGet(position.GridX, position.GridY, out var content).Should().Be(true);
            content.Should().Be(r);

            if (r.IsReference)
            {
                fixture.ItemResolver.TryQueryData(r, out EntityGridPosition entityPositionFromComponent).Should().BeTrue();
                entityPositionFromComponent.Should().BeEquivalentTo(EntityGridPosition.From(position));
            }

            return this;
        }

        public PlacementAssertions<TEntity, TItemFixture> BeEmpty()
        {
            return ContainEntity(default);
        }

        public PlacementAssertions<TEntity, TItemFixture> ContainEntityOfType(ItemDeclarationId id)
        {
            fixture.ItemMapContext.TryGetGridDataFor(position.LayerId, out var data).Should().BeTrue();
            data.TryGetView(position.GridZ, out var view).Should().BeTrue();
            Assert.NotNull(view);
            view.TryGet(position.GridX, position.GridY, out var entity).Should().Be(true);
            fixture.ItemResolver.TryResolve(entity, out var itemDeclaration).Should().BeTrue();
            itemDeclaration.Id.Should().Be(id);
            return this;
        }

        public PlacementAssertions<TEntity, TItemFixture> WithStackSize(int expectedStackCount)
        {
            fixture.ItemMapContext.TryGetGridDataFor(position.LayerId, out var data).Should().BeTrue();
            data.TryGetView(position.GridZ, out var view).Should().BeTrue();
            Assert.NotNull(view);
            view.TryGet(position.GridX, position.GridY, out var entity).Should().Be(true);
            var stack = fixture.ItemResolver.QueryStackSize(entity);
            stack.Count.Should().Be(expectedStackCount);
            return this;
        }

        public PlacementAssertions<TEntity, TItemFixture> Should()
        {
            return this;
        }


        public TEntity Fetch()
        {
            fixture.ItemMapContext.TryGetGridDataFor(position.LayerId, out var data).Should().BeTrue();
            data.TryGetView(position.GridZ, out var view).Should().BeTrue();
            Assert.NotNull(view);
            view.TryGet(position.GridX, position.GridY, out var result).Should().BeTrue();
            return result;
        }

        public ItemAssertions<TItemFixture, TEntity> AndThatItem()
        {
            var item = Fetch();
            return new ItemAssertions<TItemFixture, TEntity>(fixture, item);
        }
    }

    public class ItemAssertions<TItemFixture, TEntityId>
        where TItemFixture : IEntityFixture<TEntityId>
        where TEntityId : struct, IBulkDataStorageKey<TEntityId>
    {
        readonly TItemFixture fixture;
        readonly TEntityId item;

        public ItemAssertions(TItemFixture fixture, TEntityId item)
        {
            this.fixture = fixture;
            this.item = item;
        }

        public ItemAssertions<TItemFixture, TEntityId> Should()
        {
            return this;
        }

        public ItemAssertions<TItemFixture, TEntityId> BeDestroyed()
        {
            if (fixture.ItemResolver.EntityMetaData.IsReferenceEntity(item))
            {
                fixture.ItemResolver.IsDestroyed(item).Should().BeTrue();
            }

            return this;
        }

        public ItemAssertions<TItemFixture, TEntityId> BeActive()
        {
            item.IsEmpty.Should().BeFalse();

            if (fixture.ItemResolver.EntityMetaData.IsReferenceEntity(item))
            {
                fixture.ItemResolver.IsDestroyed(item).Should().BeFalse();
            }

            return this;
        }
    }
}