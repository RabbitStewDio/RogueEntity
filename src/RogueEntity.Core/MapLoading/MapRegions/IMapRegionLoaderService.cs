namespace RogueEntity.Core.MapLoading.MapRegions
{
    public interface IMapRegionLoaderService
    {

        /// <summary>
        ///    Attempts to load the next chunk. Returns false if there are no more chunks to load.
        /// </summary>
        bool PerformLoadNextChunk();

        /// <summary>
        ///   Returns true if there is any pending immediate load request.
        /// </summary>
        bool IsBlocked();

        /// <summary>
        ///   Returns true if there was a map region that cannot be loaded due to an unrecoverable error.
        ///   Gracefully ending the game with a blue screen would be a good next step.
        /// </summary>
        bool IsError();

        bool EvictAllRegions();
        
        void Initialize();
    }
    
    public interface IMapRegionLoaderService<TRegionKey>: IMapRegionLoaderService
    {
        IMapRegionLoadRequestStatus<TRegionKey> RequestLazyLoading(TRegionKey region);
        IMapRegionLoadRequestStatus<TRegionKey> RequestImmediateLoading(TRegionKey region);

        /// <summary>
        ///   Returns true if the given map region has been loaded and is active. This will never
        ///   return true if there had been an error.
        /// </summary>
        bool IsRegionLoaded(TRegionKey region);

        /// <summary>
        ///    Marks the given region as unloaded.
        /// </summary>
        /// <returns></returns>
        bool EvictRegion(TRegionKey region);
    }
}
