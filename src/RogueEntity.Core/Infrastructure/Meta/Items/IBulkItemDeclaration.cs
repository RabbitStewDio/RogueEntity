using EnttSharp.Entities;

namespace RogueEntity.Core.Infrastructure.Meta.Items
{
    public interface IBulkItemDeclaration<TContext, TItemId> : IItemDeclaration
        where TItemId: IEntityKey
    {
        TItemId Initialize(TContext context, TItemId itemReference);
    }
}