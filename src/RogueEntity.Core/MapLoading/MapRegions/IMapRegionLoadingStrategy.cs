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
        ///    Attempts to load the next chunk. Returns true if the given chunk was
        ///    loaded or at minimum scheduled to be loaded. 
        /// </summary>
        MapRegionLoadingStrategyResult PerformLoadChunk(TRegionKey key);

        MapRegionLoadingStrategyResult PerformUnloadChunk(TRegionKey key);
    }
}
