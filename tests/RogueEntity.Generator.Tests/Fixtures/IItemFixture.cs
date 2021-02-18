using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.Positioning.Grid;

namespace RogueEntity.Generator.Tests.Fixtures
{
    public interface IItemFixture<TEntityId>
        where TEntityId : IEntityKey
    {
        IItemResolver<TEntityId> ItemResolver{ get; }
        IGridMapContext<TEntityId> ItemMapContext { get; }
    }

    public readonly struct ItemFixture<TEntityId> : IItemFixture<TEntityId>
        where TEntityId : IEntityKey
    {
        public IItemResolver<TEntityId> ItemResolver { get; }
        public IGridMapContext<TEntityId> ItemMapContext { get; }

        public ItemFixture(IItemResolver<TEntityId> itemResolver, IGridMapContext<TEntityId> itemMapContext)
        {
            ItemResolver = itemResolver;
            ItemMapContext = itemMapContext;
        }
    }
}