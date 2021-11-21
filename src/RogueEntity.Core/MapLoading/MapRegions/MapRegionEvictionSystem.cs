using JetBrains.Annotations;
using RogueEntity.Api.Utils;
using RogueEntity.Core.MapLoading.FlatLevelMaps;
using Serilog;
using System;
using System.Diagnostics;

namespace RogueEntity.Core.MapLoading.MapRegions
{
    /// <summary>
    ///    This system reacts to unload requests by marking regions as ready for eviction.
    /// </summary>
    public class MapRegionEvictionSystem<TRegionKey> : IMapRegionEvictionSystem
    {
        readonly ILogger logger = SLog.ForContext<MapRegionLoaderSystem<TRegionKey>>();
        readonly TimeSpan maximumProcessingTime;
        readonly IMapRegionTrackerService<TRegionKey> mapTrackerService;
        readonly IMapRegionEvictionStrategy<TRegionKey> mapLoadingStrategy;
        readonly Stopwatch processingTimeStopWatch;
        readonly BufferList<IMapRegionProcessingRequestHandle<TRegionKey>> buffer;

        public MapRegionEvictionSystem([NotNull] IMapRegionTrackerService<TRegionKey> mapTrackerService,
                                       [NotNull] IMapRegionEvictionStrategy<TRegionKey> mapLoadingStrategy,
                                       TimeSpan maximumProcessingTime = default)
        {
            this.mapTrackerService = mapTrackerService ?? throw new ArgumentNullException(nameof(mapTrackerService));
            this.mapLoadingStrategy = mapLoadingStrategy ?? throw new ArgumentNullException(nameof(mapLoadingStrategy));
            this.maximumProcessingTime = NormalizeMaximumProcessingTime(maximumProcessingTime);
            this.processingTimeStopWatch = new Stopwatch();
            this.buffer = new BufferList<IMapRegionProcessingRequestHandle<TRegionKey>>();
        }

        static TimeSpan NormalizeMaximumProcessingTime(TimeSpan maximumProcessingTime)
        {
            return maximumProcessingTime <= TimeSpan.Zero ? TimeSpan.FromMilliseconds(5) : maximumProcessingTime;
        }

        /// <summary>
        ///    A basic driver function that loads the next requested chunk.
        /// </summary>
        public void EvictChunks()
        {
            processingTimeStopWatch.Restart();
            try
            {
                mapTrackerService.QueryActiveRequests(MapRegionStatus.UnloadingRequested, buffer);
                foreach (var job in buffer)
                {
                    if (PerformEviction(job))
                    {
                        // time out reached, abort any further loading.
                        return;
                    }
                }
            }
            finally
            {
                processingTimeStopWatch.Stop();
            }
        }

        protected IMapRegionTrackerService<TRegionKey> MapTrackerService => mapTrackerService;

        bool PerformEviction(IMapRegionProcessingRequestHandle<TRegionKey> job)
        {
            for (int attemptCounter = 0; attemptCounter < 5; attemptCounter += 1)
            {
                var result = mapLoadingStrategy.PerformUnloadChunk(job.RegionKey);
                logger.Debug("Attempted to evict map chunk {ChunkId} ({Status}) was {Result}", job.RegionKey, job.Status, result);

                switch (result)
                {
                    case MapRegionProcessingResult.Pending:
                    {
                        if (IsTimeOutReached())
                        {
                            return true;
                        }

                        break;
                    }
                    case MapRegionProcessingResult.Scheduled:
                    {
                        // not finished yet, will try again during next frame.
                        // in theory: On Pending try again.
                        return IsTimeOutReached();
                    }
                    case MapRegionProcessingResult.Success:
                    {
                        // job done.
                        job.MarkUnloaded();
                        return IsTimeOutReached();
                    }
                    case MapRegionProcessingResult.Error:
                    {
                        job.MarkFailed();
                        return IsTimeOutReached();
                    }
                    case MapRegionProcessingResult.Invalid:
                    {
                        job.MarkInvalid();
                        return IsTimeOutReached();
                    }
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return IsTimeOutReached();
        }

        bool IsTimeOutReached()
        {
            return (processingTimeStopWatch.Elapsed >= maximumProcessingTime);
        }
    }
}
