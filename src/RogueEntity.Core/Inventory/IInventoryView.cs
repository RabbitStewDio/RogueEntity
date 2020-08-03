using System.Collections.Generic;
using RogueEntity.Core.Infrastructure.Meta;
using RogueEntity.Core.Infrastructure.Meta.Items;
using RogueEntity.Core.Utils;

namespace RogueEntity.Core.Inventory
{
    public interface IInventoryView<TItemId>
    {
        ReadOnlyListWrapper<TItemId> Items { get; }
        Weight TotalWeight { get; }
        Weight AvailableCarryWeight { get; }
    }

    public interface IInventory<TGameContext, TItemId>: IInventoryView<TItemId>
    {
        bool TryReAddItemStack(TGameContext context, TItemId r, int slot);
        bool TryAddItem(TGameContext context, TItemId item, out TItemId o, bool ignoreWeight = false);
        bool TryRemoveItem(TGameContext context, ItemDeclarationId itemByType, out TItemId removedItem);
        bool TryRemoveItemStack(TGameContext context, TItemId itemByType, int itemPosition);

        List<TItemId> RemoveBulkItems(TGameContext context,
                                      ItemDeclarationId itemByType,
                                      int count,
                                      List<TItemId> removedItems = null);
    }
}