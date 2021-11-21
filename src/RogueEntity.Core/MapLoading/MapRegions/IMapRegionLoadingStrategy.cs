namespace RogueEntity.Core.MapLoading.MapRegions
{
    /// <summary>
    ///   Encapsulates the service interface for actually loading a given map chunk.
    ///   (This is intentionally kept as a synchronous interface. Feel free to use
    ///   async tasks in an implementation, but then also stay away from other shared
    ///   state whilst you do so)
    /// </summary>
    /// <typeparam name="TRegionKey"></typeparam>
    public interface IMapRegionLoadingStrategy<in TRegionKey> 
    {

        /// <summary>
        ///    Attempts to load the next chunk.  
        /// </summary>
        MapRegionProcessingResult PerformLoadChunk(TRegionKey key);
    }

    public interface IMapRegionEvictionStrategy<in TRegionKey>
    {
        /// <summary>
        ///    Attempts to unload the next chunk. 
        /// </summary>
        MapRegionProcessingResult PerformUnloadChunk(TRegionKey key);
    }
}
