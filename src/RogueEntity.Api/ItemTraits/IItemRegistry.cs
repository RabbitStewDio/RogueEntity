using RogueEntity.Api.Utils;

namespace RogueEntity.Api.ItemTraits
{
    public interface IItemRegistry
    {
        bool TryGetItemById(ItemDeclarationId id, out IItemDeclaration item);
        IItemDeclaration ReferenceItemById(ItemDeclarationId id);
        ReadOnlyListWrapper<IItemDeclaration> Items { get; }
        
        IBulkItemIdMapping BulkItemMapping { get; }
    }
}