using RogueEntity.Api.ItemTraits;

namespace RogueEntity.Core.Meta.Items
{
    public interface IBulkItemTrait<TItemId> : IItemTrait
    {
        TItemId Initialize(IItemDeclaration item, TItemId reference);
        
        IBulkItemTrait<TItemId> CreateInstance();
    }
}