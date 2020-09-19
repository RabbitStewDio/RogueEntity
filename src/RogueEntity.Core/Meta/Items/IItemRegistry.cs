using RogueEntity.Core.Utils;

namespace RogueEntity.Core.Meta.Items
{
    public interface IItemRegistry
    {
        bool TryGetItemById(ItemDeclarationId id, out IItemDeclaration item);
        IItemDeclaration ReferenceItemById(ItemDeclarationId id);
        ReadOnlyListWrapper<IItemDeclaration> Items { get; }
        
        IBulkItemIdMapping BulkItemMapping { get; }
    }
}