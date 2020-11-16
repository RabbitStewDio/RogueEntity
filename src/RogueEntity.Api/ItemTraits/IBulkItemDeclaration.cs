using EnTTSharp.Entities;

namespace RogueEntity.Api.ItemTraits
{
    public interface IBulkItemDeclaration<TContext, TItemId> : IItemDeclaration
        where TItemId: IEntityKey
    {
        TItemId Initialize(TContext context, TItemId itemReference);
    }
}