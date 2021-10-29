namespace RogueEntity.Core.MapLoading.MapRegions
{
    public interface IMapRegionLoadRequestStatus<out TChunkKey>
    {
        TChunkKey RegionKey { get; }
        MapRegionLoadingStatus Status { get; }
    }

    public interface IMapRegionLoadRequestProcess<out TChunkKey>: IMapRegionLoadRequestStatus<TChunkKey>
    {
        void MarkFailed();
        void MarkLoaded();
        void MarkUnloaded();
        
        /// <summary>
        ///   Marks a region as permanently unavailable. This status signals the region is not defined,
        ///   and does not mean there was a fatal error.
        /// </summary>
        void MarkInvalid();
    }
}
