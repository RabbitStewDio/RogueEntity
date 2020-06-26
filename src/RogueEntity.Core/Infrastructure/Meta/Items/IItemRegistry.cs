using RogueEntity.Core.Utils;

namespace RogueEntity.Core.Infrastructure.Meta.Items
{
    public interface IItemRegistry
    {
        bool TryGetItemById(ItemDeclarationId id, out IItemDeclaration item);
        IItemDeclaration ReferenceItemById(ItemDeclarationId id);
        ReadOnlyListWrapper<IItemDeclaration> Items { get; }
    }
}