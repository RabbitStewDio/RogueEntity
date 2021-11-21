namespace RogueEntity.Core.MapLoading.MapRegions
{
    public interface IMapRegionRequestStatus<out TChunkKey>
    {
        TChunkKey RegionKey { get; }
        MapRegionStatus Status { get; }
    }

    public interface IMapRegionProcessingRequestHandle<out TChunkKey>: IMapRegionRequestStatus<TChunkKey>
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
