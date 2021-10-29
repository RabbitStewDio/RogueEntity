namespace RogueEntity.Core.MapLoading.MapRegions
{
    public class MapRegionLoadRequestStatus<TChunkKey>: IMapRegionLoadRequestProcess<TChunkKey>
    {
        public TChunkKey RegionKey { get; }
        public MapRegionLoadingStatus Status { get; private set; }

        public MapRegionLoadRequestStatus(TChunkKey regionKey)
        {
            RegionKey = regionKey;
        }

        public void RequestLazyLoading()
        {
            if (Status == MapRegionLoadingStatus.Unloaded)
            {
                Status = MapRegionLoadingStatus.LazyLoadRequested;
            }
        }

        public void RequestImmediateLoading()
        {
            if (Status is MapRegionLoadingStatus.Unloaded or MapRegionLoadingStatus.LazyLoadRequested)
            {
                Status = MapRegionLoadingStatus.ImmediateLoadRequested;
            }
        }

        public void MarkFailed()
        {
            Status = MapRegionLoadingStatus.Error;
        }

        public void MarkLoaded()
        {
            Status = MapRegionLoadingStatus.Loaded;
        }

        public void MarkUnloaded()
        {
            Status = MapRegionLoadingStatus.Unloaded;
        }

        public void MarkInvalid()
        {
            Status = MapRegionLoadingStatus.Invalid;
        }

        public void RequestUnloading()
        {
            Status = Status switch
            {
                MapRegionLoadingStatus.Loaded => MapRegionLoadingStatus.UnloadingRequested,
                MapRegionLoadingStatus.ImmediateLoadRequested or MapRegionLoadingStatus.LazyLoadRequested => MapRegionLoadingStatus.Unloaded,
                _ => Status
            };
        }
    }
}
