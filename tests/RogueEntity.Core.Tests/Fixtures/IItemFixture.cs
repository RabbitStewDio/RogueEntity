using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.Positioning;

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
        IMapContext<TEntity> ItemMapContext { get; }
        IItemPlacementServiceContext<TEntity> ItemPlacementContext { get; }
    }
}