namespace RogueEntity.Core.MapLoading.MapRegions
{
    public enum MapRegionLoadingStatus
    {
        Unloaded = 0,
        LazyLoadRequested = 1,
        ImmediateLoadRequested = 2,
        Loaded = 3,
        UnloadingRequested = 4,
        Error = -1,
        Invalid = -2,
    }
}
