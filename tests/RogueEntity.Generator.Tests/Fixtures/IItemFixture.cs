using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Tests.Fixtures;

namespace RogueEntity.Generator.Tests.Fixtures
{
    public class ItemFixture<TEntityId> : ItemTestFixtureBase<ItemFixture<TEntityId>, TEntityId>
        where TEntityId : struct, IBulkDataStorageKey<TEntityId>
    {
        public override IItemResolver<TEntityId> ItemResolver { get; }
        public override IGridMapContext<TEntityId> ItemMapContext { get; }

        public ItemFixture(IItemResolver<TEntityId> itemResolver, IGridMapContext<TEntityId> itemMapContext)
        {
            ItemResolver = itemResolver;
            ItemMapContext = itemMapContext;
        }
    }
}