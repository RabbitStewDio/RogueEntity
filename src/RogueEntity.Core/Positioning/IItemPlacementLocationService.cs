namespace RogueEntity.Core.Positioning
{
    /// <summary>
    ///   A service that exclusively deals with finding unoccupied space.
    /// </summary>
    /// <typeparam name="TItemId"></typeparam>
    public interface IItemPlacementLocationService<TItemId>
    {
        /// <summary>
        ///    Tries to find a suitable placement position for the given item. If the
        ///    item is stackable, this will attempt to find the nearest location that
        ///    has the ability to accept more elements. If the item is not stackable,
        ///    this will attempt to locate an unoccupied location near the target point. 
        /// </summary>
        /// <param name="itemToBePlaced"></param>
        /// <param name="origin"></param>
        /// <param name="placementPos"></param>
        /// <param name="searchRadius"></param>
        /// <returns></returns>
        bool TryFindAvailableSpace<TPosition>(in TItemId itemToBePlaced,
                                              in TPosition origin,
                                              out TPosition placementPos,
                                              int searchRadius = 10)
            where TPosition: struct, IPosition<TPosition>;

        /// <summary>
        ///   Tries to find a completely unoccupied location around the origin point.
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="bodySize"></param>
        /// <param name="placementPos"></param>
        /// <param name="searchRadius"></param>
        /// <returns></returns>
        bool TryFindEmptySpace<TPosition>(in TPosition origin,
                                          in BodySize bodySize,
                                          out TPosition placementPos,
                                          int searchRadius = 10)
            where TPosition: struct, IPosition<TPosition>;
    }
}