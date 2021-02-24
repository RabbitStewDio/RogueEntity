using RogueEntity.Core.Positioning;
using RogueEntity.Core.Utils;
using System.Collections.Generic;

namespace RogueEntity.Core.MapLoading
{
    public abstract class MapRegionLoaderServiceBase<TRegionKey> : IMapRegionLoaderService<TRegionKey>
    {
        readonly Dictionary<TRegionKey, MapRegionLoadRequestStatus<TRegionKey>> chunks;

        public MapRegionLoaderServiceBase()
        {
            chunks = new Dictionary<TRegionKey, MapRegionLoadRequestStatus<TRegionKey>>();
        }

        public bool IsError()
        {
            foreach (var c in chunks.Values)
            {
                if (c.Status == MapRegionLoadingStatus.Error)
                {
                    return true;
                }
            }

            return false;
        }

        public bool IsBlocked()
        {
            foreach (var c in chunks.Values)
            {
                if (c.Status == MapRegionLoadingStatus.ImmediateLoadRequested)
                {
                    return true;
                }
            }

            return false;
        }

        public bool IsLoaded(TRegionKey region)
        {
            return chunks.TryGetValue(region, out var chunk) && chunk.Status == MapRegionLoadingStatus.Loaded;
        }

        public IMapRegionLoadRequestStatus<TRegionKey> RequestLazyLoading(TRegionKey region)
        {
            if (!chunks.TryGetValue(region, out var chunk))
            {
                chunk = new MapRegionLoadRequestStatus<TRegionKey>(region);
                chunks.Add(region, chunk);
            }
            
            chunk.RequestLazyLoading();
            return chunk;
        }
        
        public IMapRegionLoadRequestStatus<TRegionKey> RequestImmediateLoading(TRegionKey region)
        {
            if (!chunks.TryGetValue(region, out var chunk))
            {
                chunk = new MapRegionLoadRequestStatus<TRegionKey>(region);
                chunks.Add(region, chunk);
            }
            
            chunk.RequestImmediateLoading();
            return chunk;
        }

        public bool PerformLoadNextChunk()
        {
            foreach (var c in chunks.Values)
            {
                if (!c.Status.HasFlags(MapRegionLoadingStatus.ImmediateLoadRequested))
                {
                    continue;
                }

                var s = PerformLoadNextChunk(c.RegionKey);
                switch (s)
                {
                    case MapRegionLoadingStatus.Error:
                        c.MarkFailed();
                        return false;
                    case MapRegionLoadingStatus.Loaded:
                        c.MarkLoaded();
                        return true;
                    default:
                        return true;
                }
            }
            
            foreach (var c in chunks.Values)
            {
                if (!c.Status.HasFlags(MapRegionLoadingStatus.LazyLoadRequested))
                {
                    continue;
                }

                var s = PerformLoadNextChunk(c.RegionKey);
                switch (s)
                {
                    case MapRegionLoadingStatus.Error:
                        c.MarkFailed();
                        return false;
                    case MapRegionLoadingStatus.Loaded:
                        c.MarkLoaded();
                        return true;
                    case MapRegionLoadingStatus.ImmediateLoadRequested:
                        c.RequestImmediateLoading();
                        return true;
                    default:
                        return true;
                }

            }

            return false;
        }
        
        protected abstract MapRegionLoadingStatus PerformLoadNextChunk(TRegionKey region);

        public abstract bool IsLevelPositionAvailable<TPosition>(TPosition p)
            where TPosition: IPosition<TPosition>;
    }
}
