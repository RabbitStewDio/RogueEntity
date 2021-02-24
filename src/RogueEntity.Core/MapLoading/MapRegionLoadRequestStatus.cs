namespace RogueEntity.Core.MapLoading
{
    public interface IMapRegionLoadRequestStatus<TChunkKey>
    {
        TChunkKey RegionKey { get; }
        MapRegionLoadingStatus Status { get; }
    }
    
    public class MapRegionLoadRequestStatus<TChunkKey>: IMapRegionLoadRequestStatus<TChunkKey>
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
            if (Status != MapRegionLoadingStatus.Loaded &&
                Status != MapRegionLoadingStatus.Error)
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
    }
}
