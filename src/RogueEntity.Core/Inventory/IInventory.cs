using System.Collections.Generic;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.Meta.Base;
using RogueEntity.Core.Meta.ItemTraits;

namespace RogueEntity.Core.Inventory
{
    public interface IInventory<TGameContext, TItemId>: IContainerView<TItemId>
    {
        Weight TotalWeight { get; }
        Weight AvailableCarryWeight { get; }

        bool TryReAddItemStack(TGameContext context, TItemId r, int slot);
        bool TryAddItem(TGameContext context, TItemId item, out TItemId o, bool ignoreWeight = false);
        bool TryRemoveItem(TGameContext context, ItemDeclarationId itemByType, out TItemId removedItem);

        bool TryRemoveItemStack(TGameContext context, TItemId itemByType, int itemPosition);

        List<TItemId> TryRemoveItemsInBulk(TGameContext context,
                                      ItemDeclarationId itemByType,
                                      int count,
                                      List<TItemId> removedItems = null);
    }
}