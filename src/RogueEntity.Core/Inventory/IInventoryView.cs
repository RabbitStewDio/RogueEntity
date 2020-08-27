using RogueEntity.Core.Meta.ItemTraits;
using RogueEntity.Core.Utils;

namespace RogueEntity.Core.Inventory
{
    public interface IInventoryView<TItemId>
    {
        ReadOnlyListWrapper<TItemId> Items { get; }
        Weight TotalWeight { get; }
        Weight AvailableCarryWeight { get; }
    }
}
