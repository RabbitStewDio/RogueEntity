using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Tests.Fixtures;

namespace RogueEntity.Generator.Tests.Fixtures
{
    public class ItemFixture<TEntityId> : ItemTestFixtureBase<ItemFixture<TEntityId>, TEntityId>
        where TEntityId : struct, IBulkDataStorageKey<TEntityId>
    {
        public override IItemResolver<TEntityId> ItemResolver { get; }
        public override IMapContext<TEntityId> ItemMapContext { get; }
        public override IItemPlacementServiceContext<TEntityId> ItemPlacementContext { get; }

        public ItemFixture(IItemResolver<TEntityId> itemResolver, 
                           IMapContext<TEntityId> itemMapContext,
                           IItemPlacementServiceContext<TEntityId> itemPlacementContext)
        {
            ItemResolver = itemResolver;
            ItemMapContext = itemMapContext;
            ItemPlacementContext = itemPlacementContext;
        }
    }
}