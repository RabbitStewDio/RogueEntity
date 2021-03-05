namespace RogueEntity.Core.Positioning
{
    /// <summary>
    ///   Encapsulates all primitive operations that place (bulk and reference) entities 
    ///   into a map.
    /// </summary>
    /// <typeparam name="TItemId"></typeparam>
    public interface IItemPlacementService<TItemId>
    {
        bool TryQueryItem<TPosition>(in TPosition pos, out TItemId item)
            where TPosition: IPosition<TPosition>;
        
        /// <summary>
        ///   Tries to remove the target item from the map. The item will not be destroyed
        ///   in the process. Use this to place items into containers or generally leave
        ///   them outside of the physical realm.
        /// </summary>
        /// <param name="targetItem"></param>
        /// <param name="placementPos"></param>
        /// <returns></returns>
        bool TryRemoveItem<TPosition>(in TItemId targetItem,
                                      in TPosition placementPos)
            where TPosition: IPosition<TPosition>;

        /// <summary>
        ///    Places an item at the given placement position. It is assumed and strongly
        ///    recommended that the item has not been placed elsewhere (and is validated for
        ///    reference items).
        /// </summary>
        /// <param name="targetItem"></param>
        /// <param name="placementPos"></param>
        /// <returns></returns>
        bool TryPlaceItem<TPosition>(in TItemId targetItem,
                                     in TPosition placementPos)
            where TPosition: IPosition<TPosition>;

        /// <summary>
        ///    Moves an item at the given current position to the placement position. This
        ///    method will fail if the target position is not empty or stackable and receptive
        ///    of the given item.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="currentPos"></param>
        /// <param name="placementPos"></param>
        /// <returns></returns>
        bool TryMoveItem<TPosition>(in TItemId item,
                                    in TPosition currentPos,
                                    in TPosition placementPos)
            where TPosition: IPosition<TPosition>;

        /// <summary>
        ///    Swaps the source entity with the target entity (which is expected to be located at the given position).
        /// </summary>
        bool TrySwapItem<TPosition>(in TItemId sourceItem,
                                    in TPosition sourcePosition,
                                    in TItemId targetItem,
                                    in TPosition targetPosition)
            where TPosition: IPosition<TPosition>;
    }
}
