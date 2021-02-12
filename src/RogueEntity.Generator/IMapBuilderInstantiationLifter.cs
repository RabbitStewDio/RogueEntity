using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Positioning;

namespace RogueEntity.Generator
{
    public interface IMapBuilderInstantiationLifter
    {
        public Optional<TEntity> ClearPreProcess<TEntity>(ItemDeclarationId item, 
                                                          Position pos, 
                                                          IItemResolver<TEntity> itemResolver, 
                                                          TEntity entityKey)
            where TEntity : IEntityKey;

        public Optional<TEntity> InstantiatePostProcess<TEntity>(ItemDeclarationId item, 
                                                                 Position pos, 
                                                                 IItemResolver<TEntity> itemResolver, 
                                                                 TEntity entityKey)
            where TEntity : IEntityKey;
    }

}
