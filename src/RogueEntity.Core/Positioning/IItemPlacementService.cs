using RogueEntity.Core.Positioning.MapLayers;

namespace RogueEntity.Core.Positioning
{
    public interface IItemPlacementServiceContext<TGameContext, TItemId>
    {
        bool TryGetItemPlacementService(MapLayer layer, out IItemPlacementService<TGameContext, TItemId> service);
    }

    public interface IItemPlacementService<TGameContext, TItemId>
    {
        bool TryFindAvailableItemSlot(TGameContext context,
                                      TItemId itemToBePlaced,
                                      in Position origin,
                                      out Position placementPos,
                                      int searchRadius = 10);

        bool TryFindEmptyItemSlot(TGameContext context,
                                  in Position origin,
                                  out Position placementPos,
                                  int searchRadius = 10);

        bool TryRemoveItem(TGameContext context,
                           in TItemId targetItem,
                           in Position placementPos,
                           bool forcePlacement = false);

        bool TryPlaceItem(TGameContext context,
                          in TItemId targetItem,
                          in Position placementPos,
                          bool forcePlacement = false);

        bool TryReplaceItem(TGameContext context,
                            in TItemId sourceItem,
                            in TItemId targetItem,
                            in Position p,
                            bool forceMove = false);
    }
}