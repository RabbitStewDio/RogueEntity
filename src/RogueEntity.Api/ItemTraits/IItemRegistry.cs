using RogueEntity.Api.Utils;
using System.Diagnostics.CodeAnalysis;

namespace RogueEntity.Api.ItemTraits
{
    public interface IItemRegistry
    {
        bool TryGetItemById(ItemDeclarationId id, [MaybeNullWhen(false)] out IItemDeclaration item);
        IItemDeclaration ReferenceItemById(ItemDeclarationId id);
        ReadOnlyListWrapper<IItemDeclaration> Items { get; }
        
        IBulkItemIdMapping BulkItemMapping { get; }
    }
}