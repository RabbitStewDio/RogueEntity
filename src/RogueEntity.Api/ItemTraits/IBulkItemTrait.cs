namespace RogueEntity.Api.ItemTraits
{
    public interface IBulkItemTrait<TItemId> : IItemTrait
    {
        TItemId Initialize(IItemDeclaration item, TItemId reference);
        
        IBulkItemTrait<TItemId> CreateInstance();
    }
}