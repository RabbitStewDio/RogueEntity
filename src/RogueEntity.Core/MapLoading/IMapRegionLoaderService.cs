using RogueEntity.Core.Positioning;

namespace RogueEntity.Core.MapLoading
{
    public interface IMapRegionLoaderService<TRegionKey>
    {
        IMapRegionLoadRequestStatus<TRegionKey> RequestLazyLoading(TRegionKey region);
        IMapRegionLoadRequestStatus<TRegionKey> RequestImmediateLoading(TRegionKey region);

        /// <summary>
        ///   Returns true if there was a map region that cannot be loaded due to an unrecoverable error.
        ///   Gracefully ending the game with a blue screen would be a good next step.
        /// </summary>
        bool IsError();

        /// <summary>
        ///   Returns true if the given map region has been loaded and is active. This will never
        ///   return true if there had been an error.
        /// </summary>
        bool IsLoaded(TRegionKey region);

        /// <summary>
        ///   Tests whether the given position is in a fully loaded map region. This will never
        ///   return true if there had been an error in loading that region.
        /// </summary>
        bool IsLevelPositionAvailable<TPosition>(TPosition p)            
            where TPosition: IPosition<TPosition>;
        
        /// <summary>
        ///    Attempts to load the next chunk. Returns false if there are no more chunks to load.
        /// </summary>
        bool PerformLoadNextChunk();

        /// <summary>
        ///   Returns true if there is any pending immediate load request.
        /// </summary>
        bool IsBlocked();
    }
}
