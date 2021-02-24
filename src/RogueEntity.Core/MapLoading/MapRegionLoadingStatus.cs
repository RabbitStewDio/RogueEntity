namespace RogueEntity.Core.MapLoading
{
    public enum MapRegionLoadingStatus
    {
        Unloaded = 0,
        LazyLoadRequested = 1,
        ImmediateLoadRequested = 2,
        Loaded = 3,
        Error = 4
    }
}
