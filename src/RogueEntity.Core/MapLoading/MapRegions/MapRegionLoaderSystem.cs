using RogueEntity.Api.Utils;
using RogueEntity.Core.MapLoading.FlatLevelMaps;
using Serilog;
using System;
using System.Diagnostics;

namespace RogueEntity.Core.MapLoading.MapRegions
{
    /// <summary>
    ///    A basic map loading handler that connects incoming change requests into
    ///    map tracker updates. Any pending tracker regions marked for loading
    ///    will then be forwarded to a map region loading strategy. 
    /// </summary>
    public class MapRegionLoaderSystem<TRegionKey> : IFlatLevelRegionLoaderSystem
    {
        readonly ILogger logger = SLog.ForContext<MapRegionLoaderSystem<TRegionKey>>();
        readonly TimeSpan maximumProcessingTime;
        readonly IMapRegionTrackerService<TRegionKey> mapTrackerService;
        readonly IMapRegionLoadingStrategy<TRegionKey> mapLoadingStrategy;
        readonly Stopwatch processingTimeStopWatch;
        readonly BufferList<IMapRegionProcessingRequestHandle<TRegionKey>> buffer;

        public MapRegionLoaderSystem(IMapRegionTrackerService<TRegionKey> mapLoaderService,
                                                IMapRegionLoadingStrategy<TRegionKey> mapLoadingStrategy,
                                                TimeSpan maximumProcessingTime = default)
        {
            this.mapTrackerService = mapLoaderService ?? throw new ArgumentNullException(nameof(mapLoaderService));
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
        public void LoadChunks()
        {
            processingTimeStopWatch.Restart();
            try
            {
                mapTrackerService.QueryActiveRequests(MapRegionStatus.ImmediateLoadRequested, buffer);
                foreach (var job in buffer)
                {
                    if (PerformLoad(job))
                    {
                        // time out reached, abort any further loading.
                        return;
                    }
                }

                mapTrackerService.QueryActiveRequests(MapRegionStatus.LazyLoadRequested, buffer);
                foreach (var job in buffer)
                {
                    if (PerformLoad(job))
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

        bool PerformLoad(IMapRegionProcessingRequestHandle<TRegionKey> job)
        {
            for (int attemptCounter = 0; attemptCounter < 5; attemptCounter += 1)
            {
                var result = mapLoadingStrategy.PerformLoadChunk(job.RegionKey);
                logger.Debug("Attempted to load map chunk {ChunkId} ({Status}) was {Result}", job.RegionKey, job.Status, result);

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
                        job.MarkLoaded();
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
