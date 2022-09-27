using RogueEntity.Api.Utils;
using System.Collections.Generic;

namespace RogueEntity.Core.MapLoading.MapRegions
{
    /// <summary>
    ///   Tracks the current status of all known/loaded map regions. This service
    ///   does not handle loading or eviction of regions.
    /// </summary>
    /// <typeparam name="TRegionKey"></typeparam>
    public class BasicMapRegionTrackerService<TRegionKey> : IMapRegionTrackerService<TRegionKey>
    {
        readonly Dictionary<TRegionKey, MapRegionProcessingRequestStatus<TRegionKey>> chunks;

        public BasicMapRegionTrackerService()
        {
            chunks = new Dictionary<TRegionKey, MapRegionProcessingRequestStatus<TRegionKey>>();
        }

        /// <summary>
        ///   (Re)Initializes the map region loader. This marks all map regions as unloaded.
        ///   We do assume that this is part of a global reset and not just something you
        ///   called because you felt like it. Make sure all underlying maps and data stores
        ///   are reset as well or chaos will strike. 
        /// </summary>
        public void Initialize()
        {
            chunks.Clear();
        }

        public bool IsError()
        {
            foreach (var c in chunks.Values)
            {
                if (c.Status == MapRegionStatus.Error)
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
                if (c.Status == MapRegionStatus.ImmediateLoadRequested)
                {
                    return true;
                }
            }

            return false;
        }

        public MapRegionStatus QueryRegionStatus(TRegionKey region)
        {
            if (chunks.TryGetValue(region, out var chunk))
            {
                return chunk.Status;
            }

            return MapRegionStatus.Unloaded;
        }

        public IMapRegionRequestStatus<TRegionKey> RequestLazyLoading(TRegionKey region)
        {
            if (!chunks.TryGetValue(region, out var chunk))
            {
                chunk = new MapRegionProcessingRequestStatus<TRegionKey>(region);
                chunks.Add(region, chunk);
            }

            chunk.RequestLazyLoading();
            return chunk;
        }

        public IMapRegionRequestStatus<TRegionKey> RequestImmediateLoading(TRegionKey region)
        {
            if (!chunks.TryGetValue(region, out var chunk))
            {
                chunk = new MapRegionProcessingRequestStatus<TRegionKey>(region);
                chunks.Add(region, chunk);
            }

            chunk.RequestImmediateLoading();
            return chunk;
        }

        public BufferList<IMapRegionProcessingRequestHandle<TRegionKey>> QueryActiveRequests(MapRegionStatus status,
                                                                                             BufferList<IMapRegionProcessingRequestHandle<TRegionKey>>? k = null)
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

        public IMapRegionRequestStatus<TRegionKey> EvictRegion(TRegionKey region)
        {
            if (chunks.TryGetValue(region, out var chunk))
            {
                chunk.RequestUnloading();
            }
            else
            {
                chunk = new MapRegionProcessingRequestStatus<TRegionKey>(region);
                chunks[region] = chunk;
            }

            return chunk;
        }
    }
}
