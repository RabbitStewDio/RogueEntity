using FluentAssertions;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.Meta.ItemTraits;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Grid;
using System.Collections.Generic;

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
            var cmp = EqualityComparer<TEntity>.Default;
            fixture.ItemMapContext.TryGetMapDataFor(position.LayerId, out var data).Should().BeTrue();
            var found = false;
            foreach (var content in data.QueryItem(position))
            {
                found |= cmp.Equals(content, r);
            }
            found.Should().BeTrue();

            if (r.IsReference)
            {
                fixture.ItemResolver.TryQueryData<EntityGridPosition>(r, out var entityPositionFromComponent).Should().BeTrue();
                entityPositionFromComponent.Should().BeEquivalentTo(EntityGridPosition.From(position));
            }

            return this;
        }

        public PlacementAssertions<TEntity, TItemFixture> BeEmpty()
        {
            fixture.ItemMapContext.TryGetMapDataFor(position.LayerId, out var data).Should().BeTrue();
            data.QueryItem(position).Count.Should().Be(0);
            return this;
        }

        public PlacementAssertions<TEntity, TItemFixture> ContainEntityOfType(ItemDeclarationId id)
        {
            fixture.ItemMapContext.TryGetMapDataFor(position.LayerId, out var data).Should().BeTrue();
            foreach (var content in data.QueryItem(position))
            {
                if (fixture.ItemResolver.TryResolve(content, out var itemDeclaration) &&
                    itemDeclaration.Id == id)
                {
                    return this;
                }
            }
            
            NUnit.Framework.Assert.Fail();
            return this;
        }

        public PlacementAssertions<TEntity, TItemFixture> WithStackSize(int expectedStackCount)
        {
            fixture.ItemMapContext.TryGetMapDataFor(position.LayerId, out var data).Should().BeTrue();
            foreach (var content in data.QueryItem(position))
            {
                var stack = fixture.ItemResolver.QueryStackSize(content);
                if (stack.Count == expectedStackCount)
                {
                    return this;
                }
            }
            
            NUnit.Framework.Assert.Fail();
            return this;
        }

        public PlacementAssertions<TEntity, TItemFixture> Should()
        {
            return this;
        }


        public TEntity Fetch()
        {
            fixture.ItemMapContext.TryGetMapDataFor(position.LayerId, out var data).Should().BeTrue();
            foreach (var content in data.QueryItem(position))
            {
                return content;
            }
            
            return default;
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