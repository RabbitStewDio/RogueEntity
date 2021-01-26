using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.Meta.EntityKeys;
using RogueEntity.Core.Positioning;

namespace RogueEntity.Generator.Tests.Fixtures
{
    public static class ItemFixtureExtensions
    {
        public static EntityContext<TItemFixture, TEntityId> GivenAnEntity<TItemFixture, TEntityId>(this TItemFixture f, ItemDeclarationId item)
            where TItemFixture : IItemFixture<TEntityId>
            where TEntityId : IEntityKey
            => new EntityContext<TItemFixture, TEntityId>(f, item);
        
        public static EntityContext<TItemFixture, TEntityId> GivenAnEmptySpace<TItemFixture, TEntityId>(this TItemFixture f)
            where TItemFixture : IItemFixture<TEntityId>
            where TEntityId : IEntityKey
            => new EntityContext<TItemFixture, TEntityId>(f);
        
        public static PlacementAssertions<TItemFixture, TEntityId> Then_Position<TItemFixture, TEntityId>(this TItemFixture f, Position position)
            where TItemFixture : IItemFixture<TEntityId>
            where TEntityId : IEntityKey
            => new PlacementAssertions<TItemFixture, TEntityId>(f, position);

        public static PlacementAssertions<ItemFixture<TEntityId>, TEntityId> Then_Position<TEntityId>(this ItemFixture<TEntityId> f, Position position)
            where TEntityId : IEntityKey
            => new PlacementAssertions<ItemFixture<TEntityId>, TEntityId>(f, position);

        public static ItemAssertions<ItemFixture<TEntityId>, TEntityId> Then_Item<TEntityId>(this ItemFixture<TEntityId> f, in TEntityId item)
            where TEntityId : IEntityKey
            => new ItemAssertions<ItemFixture<TEntityId>, TEntityId>(f, item);
        
        public static ItemAssertions<TItemFixture, TEntityId> Then_Item<TEntityId, TItemFixture>(this TItemFixture f, in TEntityId item)
            where TEntityId : IEntityKey
            where TItemFixture : IItemFixture<TEntityId>
            => new ItemAssertions<TItemFixture, TEntityId>(f, item);
    }
}
