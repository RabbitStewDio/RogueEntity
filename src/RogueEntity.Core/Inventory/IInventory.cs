using System.Collections.Generic;
using RogueEntity.Core.Meta.Items;

namespace RogueEntity.Core.Inventory
{
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