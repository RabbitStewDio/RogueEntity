using EnTTSharp;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Meta.Base;
using RogueEntity.Core.Meta.ItemTraits;
using System.Diagnostics.CodeAnalysis;

namespace RogueEntity.Core.Inventory
{
    public interface IInventory<TItemId> : IContainerView<TItemId>
    {
        Weight TotalWeight { get; }
        Weight AvailableCarryWeight { get; }

        bool TryReAddItemStack(TItemId r, int slot);
        bool TryAddItem(TItemId item, out Optional<TItemId> remainder, bool ignoreWeight = false);
        bool TryRemoveItem(ItemDeclarationId itemByType, [MaybeNullWhen(false)] out TItemId removedItem);

        bool TryRemoveItemStack(TItemId itemByType, int itemPosition);

        BufferList<TItemId> TryRemoveItemsInBulk(ItemDeclarationId itemByType,
                                                 int count,
                                                 BufferList<TItemId>? removedItems = null);
    }
}
