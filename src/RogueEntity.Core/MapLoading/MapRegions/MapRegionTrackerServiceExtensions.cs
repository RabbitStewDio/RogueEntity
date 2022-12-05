namespace RogueEntity.Core.MapLoading.MapRegions;

public static class MapRegionTrackerServiceExtensions
{
    /// <summary>
    ///   Returns true if the given map region has been loaded and is active. This will never
    ///   return true if there had been an error.
    /// </summary>
    public static bool IsRegionLoaded<TRegionKey>(this IMapRegionTrackerService<TRegionKey> x, TRegionKey region)
    {
        return x.QueryRegionStatus(region) == MapRegionStatus.Loaded;
    }


    /// <summary>
    ///    Returns true if the given map region has been evicted and is fully unloaded.
    ///    This will never return true for already loaded regions or regions currently
    ///    waiting to be unloaded.
    /// </summary>
    public static bool IsRegionEvicted<TRegionKey>(this IMapRegionTrackerService<TRegionKey> x, TRegionKey region)
    {
        return x.QueryRegionStatus(region) == MapRegionStatus.Unloaded;
    }
}