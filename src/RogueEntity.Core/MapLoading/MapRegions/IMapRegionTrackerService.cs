using RogueEntity.Api.Utils;

namespace RogueEntity.Core.MapLoading.MapRegions
{
    public interface IMapRegionTrackerService
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
    public interface IMapRegionTrackerService<TRegionKey> : IMapRegionTrackerService
    {
        IMapRegionRequestStatus<TRegionKey> RequestLazyLoading(TRegionKey region);

        IMapRegionRequestStatus<TRegionKey> RequestImmediateLoading(TRegionKey region);

        MapRegionStatus QueryRegionStatus(TRegionKey region);

        /// <summary>
        ///    Marks the given region as unloaded.
        /// </summary>
        /// <returns></returns>
        IMapRegionRequestStatus<TRegionKey> EvictRegion(TRegionKey region);

        BufferList<IMapRegionProcessingRequestHandle<TRegionKey>> QueryActiveRequests(MapRegionStatus status,
                                                                                       BufferList<IMapRegionProcessingRequestHandle<TRegionKey>>? k = null);
    }
}
