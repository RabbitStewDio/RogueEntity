using EnTTSharp.Entities;

namespace RogueEntity.Core.Meta.Items
{
    public interface IBulkItemDeclaration<TContext, TItemId> : IItemDeclaration
        where TItemId: IEntityKey
    {
        TItemId Initialize(TContext context, TItemId itemReference);
    }
}