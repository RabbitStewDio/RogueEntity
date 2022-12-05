using FluentAssertions;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.Tests.Fixtures;

namespace RogueEntity.Generator.Tests.Fixtures
{
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
