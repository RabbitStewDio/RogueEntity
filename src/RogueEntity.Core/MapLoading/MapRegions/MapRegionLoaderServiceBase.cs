using RogueEntity.Api.Utils;
using Serilog;
using System.Collections.Generic;
using System.Linq;

namespace RogueEntity.Core.MapLoading.MapRegions
{
    public abstract class MapRegionLoaderServiceBase<TRegionKey> : IMapRegionLoaderService<TRegionKey>
    {
        static readonly ILogger Logger = SLog.ForContext<MapRegionLoaderServiceBase<TRegionKey>>();
        
        readonly Dictionary<TRegionKey, MapRegionLoadRequestStatus<TRegionKey>> chunks;

        public MapRegionLoaderServiceBase()
        {
            chunks = new Dictionary<TRegionKey, MapRegionLoadRequestStatus<TRegionKey>>();
        }

        public abstract void Initialize();

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

        public bool IsRegionLoaded(TRegionKey region)
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
                if (c.Status != MapRegionLoadingStatus.ImmediateLoadRequested)
                {
                    continue;
                }

                Logger.Debug("Loading map chunk {RegionKey} as immediate chunk", c.RegionKey);
                var s = PerformLoadNextChunk(c.RegionKey);
                switch (s)
                {
                    case MapRegionLoadingStatus.Error:
                        Logger.Debug("Loading map chunk {RegionKey} as immediate chunk failed", c.RegionKey);
                        c.MarkFailed();
                        return false;
                    case MapRegionLoadingStatus.Loaded:
                        Logger.Debug("Loading map chunk {RegionKey} as immediate chunk success", c.RegionKey);
                        c.MarkLoaded();
                        return true;
                    default:
                        return true;
                }
            }
            
            foreach (var c in chunks.Values)
            {
                if (c.Status != MapRegionLoadingStatus.LazyLoadRequested)
                {
                    continue;
                }

                var s = PerformLoadNextChunk(c.RegionKey);
                switch (s)
                {
                    case MapRegionLoadingStatus.Error:
                        Logger.Debug("Loading map chunk {RegionKey} as lazy chunk failed", c.RegionKey);
                        c.MarkFailed();
                        return false;
                    case MapRegionLoadingStatus.Loaded:
                        Logger.Debug("Loading map chunk {RegionKey} as lazy chunk success", c.RegionKey);
                        c.MarkLoaded();
                        return true;
                    case MapRegionLoadingStatus.ImmediateLoadRequested:
                        Logger.Debug("Loading map chunk {RegionKey} as lazy chunk upgraded to immediate loading", c.RegionKey);
                        c.RequestImmediateLoading();
                        return true;
                    default:
                        return true;
                }

            }

            return false;
        }

        public virtual bool EvictRegion(TRegionKey region)
        {
            chunks.Remove(region);
            return true;
        }

        public bool EvictAllRegions()
        {
            var keys = chunks.Keys.ToArray();
            bool result = true;
            foreach (var k in keys)
            {
                result &= EvictRegion(k);
            }

            return result;
        }

        protected abstract MapRegionLoadingStatus PerformLoadNextChunk(TRegionKey region);
    }
}