namespace RogueEntity.Core.Positioning
{
    /// <summary>
    ///   Encapsulates all primitive operations that place (bulk and reference) entities 
    ///   into a map.
    /// </summary>
    /// <typeparam name="TItemId"></typeparam>
    public interface IItemPlacementService<TItemId>
    {
        /// <summary>
        ///   Tries to remove the target item from the map. The item will not be destroyed
        ///   in the process. Use this to place items into containers or generally leave
        ///   them outside of the physical realm.
        /// </summary>
        /// <param name="targetItem"></param>
        /// <param name="placementPos"></param>
        /// <returns></returns>
        bool TryRemoveItem(in TItemId targetItem,
                           in Position placementPos);

        /// <summary>
        ///    Places an item at the given placement position. It is assumed and strongly
        ///    recommended that the item has not been placed elsewhere (and is validated for
        ///    reference items).
        /// </summary>
        /// <param name="targetItem"></param>
        /// <param name="placementPos"></param>
        /// <returns></returns>
        bool TryPlaceItem(in TItemId targetItem,
                          in Position placementPos);

        /// <summary>
        ///    Places an item at the given placement position. It is assumed and strongly
        ///    recommended that the item has not been placed elsewhere (and is validated for
        ///    reference items).
        /// </summary>
        /// <param name="item"></param>
        /// <param name="currentPos"></param>
        /// <param name="placementPos"></param>
        /// <returns></returns>
        bool TryMoveItem(in TItemId item,
                         in Position currentPos,
                         in Position placementPos);

        /// <summary>
        ///    Swaps the source entity with the target entity (which is expected to be located at the given position).
        /// </summary>
        bool TrySwapItem(in TItemId sourceItem,
                         in Position sourcePosition,
                         in TItemId targetItem,
                         in Position targetPosition);
    }
}
