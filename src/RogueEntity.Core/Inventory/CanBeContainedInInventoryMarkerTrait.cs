using System.Runtime.Serialization;
using RogueEntity.Core.Infrastructure.Meta.Items;

namespace RogueEntity.Core.Inventory
{
    /// <summary>
    ///   This trait places a marker on whether an item has been placed in an inventory or container.
    ///   This can be used to look up the container that holds this particular item.
    /// </summary>
    public class CanBeContainedInInventoryMarkerTrait<TGameContext, TOwnerId, TItemId> : 
        SimpleItemComponentTraitBase<TGameContext, TItemId, ContainedInInventoryMarker<TOwnerId, TItemId>>
        where TItemId : IBulkDataStorageKey<TItemId>
    {
        public CanBeContainedInInventoryMarkerTrait() : base("Core.Item.ParentContainerMarker", 100)
        {
        }

        protected override ContainedInInventoryMarker<TOwnerId, TItemId> CreateInitialValue(TGameContext c, TItemId reference)
        {
            return new ContainedInInventoryMarker<TOwnerId, TItemId>();
        }
    }
}