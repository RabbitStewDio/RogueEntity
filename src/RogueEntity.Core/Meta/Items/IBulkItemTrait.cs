namespace RogueEntity.Core.Meta.Items
{
    public interface IBulkItemTrait<TContext, TItemId> : IItemTrait
    {
        TItemId Initialize(TContext context, IItemDeclaration item, TItemId reference);
    }
}