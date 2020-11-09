namespace RogueEntity.Core.Meta.Items
{
    public interface IBulkItemTrait<TGameContext, TItemId> : IItemTrait
    {
        TItemId Initialize(TGameContext context, IItemDeclaration item, TItemId reference);
        
        IBulkItemTrait<TGameContext, TItemId> CreateInstance();
    }
}