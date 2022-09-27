using EnTTSharp;
using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.Positioning;

namespace RogueEntity.Core.MapLoading.Builder
{
    public interface IMapBuilderInstantiationLifter
    {
        public Optional<TEntity> ClearPreProcess<TEntity>(ItemDeclarationId item, 
                                                          Position pos, 
                                                          IItemResolver<TEntity> itemResolver, 
                                                          TEntity entityKey)
            where TEntity : struct, IEntityKey;

        public Optional<TEntity> InstantiatePostProcess<TEntity>(ItemDeclarationId item, 
                                                                 Position pos, 
                                                                 IItemResolver<TEntity> itemResolver, 
                                                                 TEntity entityKey)
            where TEntity : struct, IEntityKey;
    }

}
