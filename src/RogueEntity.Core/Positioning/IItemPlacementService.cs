using RogueEntity.Core.Positioning.MapLayers;

namespace RogueEntity.Core.Positioning
{
    public interface IItemPlacementServiceContext<TItemId>
    {
        bool TryGetItemPlacementService(MapLayer layer, out IItemPlacementService<TItemId> service);
    }

    public interface IItemPlacementService<TItemId>
    {
        bool TryFindAvailableItemSlot(TItemId itemToBePlaced,
                                      in Position origin,
                                      out Position placementPos,
                                      int searchRadius = 10);

        bool TryFindEmptyItemSlot(in Position origin,
                                  out Position placementPos,
                                  int searchRadius = 10);

        bool TryRemoveItem(in TItemId targetItem,
                           in Position placementPos,
                           bool forcePlacement = false);

        bool TryPlaceItem(in TItemId targetItem,
                          in Position placementPos,
                          bool forcePlacement = false);

        bool TryReplaceItem(in TItemId sourceItem,
                            in TItemId targetItem,
                            in Position p,
                            bool forceMove = false);
    }
}
