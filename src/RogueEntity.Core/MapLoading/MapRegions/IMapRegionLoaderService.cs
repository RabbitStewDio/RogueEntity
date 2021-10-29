using RogueEntity.Api.Utils;

namespace RogueEntity.Core.MapLoading.MapRegions
{
    public interface IMapRegionLoaderService
    {
        /// <summary>
        ///   Returns true if there is any pending immediate load request.
        /// </summary>
        bool IsBlocked();

        /// <summary>
        ///   Returns true if there was a map region that cannot be loaded due to an unrecoverable error.
        ///   Gracefully ending the game with a blue screen would be a good next step.
        /// </summary>
        bool IsError();

        void Initialize();
    }
    
    /// <summary>
    ///   Controls the loading and unloading of map chunks. Unloading is an optional
    ///   system - if not defined any unload request is simply silently ignored.
    /// </summary>
    /// <typeparam name="TRegionKey"></typeparam>
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
        IMapRegionLoadRequestStatus<TRegionKey> EvictRegion(TRegionKey region);


        BufferList<IMapRegionLoadRequestProcess<TRegionKey>> QueryPendingRequests(MapRegionLoadingStatus status, 
                                                                                 BufferList<IMapRegionLoadRequestProcess<TRegionKey>> k = null);
    }
}
