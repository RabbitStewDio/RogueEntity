using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.Meta.EntityKeys;
using RogueEntity.Core.Positioning.Grid;

namespace RogueEntity.Core.Tests.Fixtures
{
    /// <summary>
    ///    Explicit fixture references are needed as the CSharp compiler is not able
    ///    to resolve type parameters reliably. 
    /// </summary>
    public interface IEntityFixture<TEntity>
      where TEntity: struct, IBulkDataStorageKey<TEntity>
    {
        IItemResolver<TEntity> ItemResolver{ get; }
        IGridMapContext<TEntity> ItemMapContext { get; }
    }
}