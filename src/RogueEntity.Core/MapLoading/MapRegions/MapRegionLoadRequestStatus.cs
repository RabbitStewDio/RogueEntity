namespace RogueEntity.Core.MapLoading.MapRegions
{
    public class MapRegionProcessingRequestStatus<TChunkKey>: IMapRegionProcessingRequestHandle<TChunkKey>
    {
        public TChunkKey RegionKey { get; }
        public MapRegionStatus Status { get; private set; }

        public MapRegionProcessingRequestStatus(TChunkKey regionKey)
        {
            RegionKey = regionKey;
        }

        public void RequestLazyLoading()
        {
            if (Status == MapRegionStatus.Unloaded)
            {
                Status = MapRegionStatus.LazyLoadRequested;
            }
        }

        public void RequestImmediateLoading()
        {
            if (Status is MapRegionStatus.Unloaded or MapRegionStatus.LazyLoadRequested)
            {
                Status = MapRegionStatus.ImmediateLoadRequested;
            }
        }

        public void MarkFailed()
        {
            Status = MapRegionStatus.Error;
        }

        public void MarkLoaded()
        {
            Status = MapRegionStatus.Loaded;
        }

        public void MarkUnloaded()
        {
            Status = MapRegionStatus.Unloaded;
        }

        public void MarkInvalid()
        {
            Status = MapRegionStatus.Invalid;
        }

        public void RequestUnloading()
        {
            Status = Status switch
            {
                MapRegionStatus.Loaded => MapRegionStatus.UnloadingRequested,
                MapRegionStatus.ImmediateLoadRequested or MapRegionStatus.LazyLoadRequested => MapRegionStatus.Unloaded,
                _ => Status
            };
        }
    }
}
