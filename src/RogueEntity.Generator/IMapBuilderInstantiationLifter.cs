using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.Positioning;

namespace RogueEntity.Generator
{
    public interface IMapBuilderInstantiationLifter
    {
        public bool ClearPreProcess<TEntity>(ItemDeclarationId item, Position pos, IItemResolver<TEntity> itemResolver, ref TEntity entityKey) where TEntity: IEntityKey;
        public bool InstantiatePostProcess<TEntity>(ItemDeclarationId item, Position pos, IItemResolver<TEntity> itemResolver, ref TEntity entityKey) where TEntity: IEntityKey;
    }
}
