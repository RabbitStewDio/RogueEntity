using RogueEntity.Api.Utils;
using Serilog;
using System.Collections.Generic;

namespace RogueEntity.Core.MapLoading.MapRegions
{
    public class BasicMapRegionLoaderService<TRegionKey> : IMapRegionLoaderService<TRegionKey>
    {
        static readonly ILogger Logger = SLog.ForContext<BasicMapRegionLoaderService<TRegionKey>>();

        readonly Dictionary<TRegionKey, MapRegionLoadRequestStatus<TRegionKey>> chunks;

        public BasicMapRegionLoaderService()
        {
            chunks = new Dictionary<TRegionKey, MapRegionLoadRequestStatus<TRegionKey>>();
        }

        /// <summary>
        ///   (Re)Initializes the map region loader. This marks all map regions as unloaded.
        ///   We do assume that this is part of a global reset and not just something you
        ///   called because you felt like it. Make sure all underlying maps and data stores
        ///   are reset as well or chaos will strike. 
        /// </summary>
        public virtual void Initialize()
        {
            chunks.Clear();
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

        public BufferList<IMapRegionLoadRequestProcess<TRegionKey>> QueryPendingRequests(MapRegionLoadingStatus status,
                                                                                         BufferList<IMapRegionLoadRequestProcess<TRegionKey>> k = null)
        {
            k = BufferList.PrepareBuffer(k);

            foreach (var c in chunks.Values)
            {
                if (c.Status == status)
                {
                    k.Add(c);
                }
            }

            return k;
        }

        public virtual IMapRegionLoadRequestStatus<TRegionKey> EvictRegion(TRegionKey region)
        {
            if (chunks.TryGetValue(region, out var chunk))
            {
                chunk.RequestUnloading();
            }
            else
            {
                chunk = new MapRegionLoadRequestStatus<TRegionKey>(region);
                chunks[region] = chunk;
            }

            return chunk;
        }
    }
}
