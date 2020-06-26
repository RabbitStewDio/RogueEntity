namespace RogueEntity.Core.Infrastructure.Meta.Items
{
    public interface IBulkItemTrait<TContext, TItemId> : IItemTrait
    {
        TItemId Initialize(TContext context, IItemDeclaration item, TItemId reference);
    }
}