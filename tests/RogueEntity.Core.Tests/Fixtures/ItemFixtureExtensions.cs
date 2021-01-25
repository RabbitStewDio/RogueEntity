using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Tests.Positioning.Grid.GridItemPlacementServiceTest;

namespace RogueEntity.Core.Tests.Fixtures
{
    public static class ItemFixtureExtensions
    {
        public static EntityContext<TItemFixture> GivenAnEntity<TItemFixture>(this TItemFixture f, ItemDeclarationId item)
            where TItemFixture : IItemFixture
            => new EntityContext<TItemFixture>(f, item);
        
        public static EntityContext<TItemFixture> GivenAnEmptySpace<TItemFixture>(this TItemFixture f)
            where TItemFixture : IItemFixture
            => new EntityContext<TItemFixture>(f);
        
        public static PlacementAssertions<TItemFixture> Then_Position<TItemFixture>(this TItemFixture f, Position position)
            where TItemFixture : IItemFixture
        {
            return new PlacementAssertions<TItemFixture>(f, position);
        }

    }
}
