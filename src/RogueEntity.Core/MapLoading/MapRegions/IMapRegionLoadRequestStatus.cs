namespace RogueEntity.Core.MapLoading.MapRegions
{
    public interface IMapRegionLoadRequestStatus<TChunkKey>
    {
        TChunkKey RegionKey { get; }
        MapRegionLoadingStatus Status { get; }
    }
}
