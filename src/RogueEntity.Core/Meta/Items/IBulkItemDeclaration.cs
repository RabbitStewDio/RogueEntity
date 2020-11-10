using EnTTSharp.Entities;
using RogueEntity.Core.Infrastructure.ItemTraits;

namespace RogueEntity.Core.Meta.Items
{
    public interface IBulkItemDeclaration<TContext, TItemId> : IItemDeclaration
        where TItemId: IEntityKey
    {
        TItemId Initialize(TContext context, TItemId itemReference);
    }
}