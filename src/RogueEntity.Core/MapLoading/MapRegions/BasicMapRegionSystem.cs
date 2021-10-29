using EnTTSharp.Entities;
using JetBrains.Annotations;
using RogueEntity.Api.Utils;
using Serilog;
using System;
using System.Diagnostics;

namespace RogueEntity.Core.MapLoading.MapRegions
{
    /// <summary>
    ///    A simple map loader system for loading whole levels (identified by the Z-coordinate).
    ///    This service is only activated if there is a <see cref="IMapRegionLoaderService{TRegionKey}"/>
    ///    registered as a service.
    /// </summary>
    public class BasicMapRegionSystem : IMapRegionSystem
    {
        readonly ILogger logger = SLog.ForContext<BasicMapRegionSystem>();
        readonly TimeSpan maximumProcessingTime;
        readonly IMapRegionLoaderService<int> mapLoaderService;
        readonly IMapRegionLoadingStrategy<int> mapLoadingStrategy;
        readonly Stopwatch processingTimeStopWatch;
        readonly BufferList<IMapRegionLoadRequestProcess<int>> buffer;

        public BasicMapRegionSystem([NotNull] IMapRegionLoaderService<int> mapLoaderService,
                                    [NotNull] IMapRegionLoadingStrategy<int> mapLoadingStrategy,
                                    TimeSpan maximumProcessingTime = default)
        {
            this.mapLoaderService = mapLoaderService ?? throw new ArgumentNullException(nameof(mapLoaderService));
            this.mapLoadingStrategy = mapLoadingStrategy ?? throw new ArgumentNullException(nameof(mapLoadingStrategy));
            this.maximumProcessingTime = NormalizeMaximumLoadingTime(maximumProcessingTime);
            this.processingTimeStopWatch = new Stopwatch();
            this.buffer = new BufferList<IMapRegionLoadRequestProcess<int>>();
        }

        static TimeSpan NormalizeMaximumLoadingTime(TimeSpan maximumProcessingTime)
        {
            return maximumProcessingTime <= TimeSpan.Zero ? TimeSpan.FromMilliseconds(5) : maximumProcessingTime;
        }

        /// <summary>
        ///   Invoked when a existing player requests to be moved to a different level.
        ///   May not be appropriate for all game types. Also used when a player is moving
        ///   into a new level by entering a stair case or portal, where the player
        ///   has no control over where the end point of the portal lies. 
        /// </summary>
        public void RequestLoadLevelFromChangeLevelCommand<TItemId>(IEntityViewControl<TItemId> v,
                                                                    TItemId k,
                                                                    in ChangeLevelRequest cmd)
            where TItemId : IEntityKey
        {
            var level = cmd.Level;
            mapLoaderService.RequestImmediateLoading(level);
        }

        /// <summary>
        ///   Invoked when a player is moving into a new level by falling or by knowing where
        ///   the end point of a given portal is placed. Useful for stairs that should line
        ///   up across levels or for jumping down a hole in the ground. 
        /// </summary>
        public void RequestLoadLevelFromChangePositionCommand<TItemId>(IEntityViewControl<TItemId> v,
                                                                       TItemId k,
                                                                       in ChangeLevelPositionRequest cmd)
            where TItemId : IEntityKey
        {
            if (cmd.Position.IsInvalid)
            {
                v.RemoveComponent<ChangeLevelPositionRequest>(k);
                return;
            }

            var level = cmd.Position.GridZ;
            mapLoaderService.RequestImmediateLoading(level);
        }

        /// <summary>
        ///    A basic driver function that loads the next requested chunk.
        /// </summary>
        public void LoadChunks()
        {
            processingTimeStopWatch.Restart();
            try
            {
                mapLoaderService.QueryPendingRequests(MapRegionLoadingStatus.ImmediateLoadRequested, buffer);
                foreach (var job in buffer)
                {
                    if (PerformLoad(job))
                    {
                        // time out reached, abort any further loading.
                        return;
                    }
                }
                
                mapLoaderService.QueryPendingRequests(MapRegionLoadingStatus.LazyLoadRequested, buffer);
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

        protected IMapRegionLoaderService<int> MapLoaderService => mapLoaderService;

        bool PerformLoad(IMapRegionLoadRequestProcess<int> job)
        {
            for (int attemptCounter = 0; attemptCounter < 5; attemptCounter += 1)
            {
                var result = mapLoadingStrategy.PerformLoadChunk(job.RegionKey);
                logger.Debug("Attempted to load map chunk {ChunkId} ({Status}) was {Result}", job.RegionKey, job.Status, result);

                switch (result)
                {
                    case MapRegionLoadingStrategyResult.Pending:
                    {
                        if (IsTimeOutReached())
                        {
                            return true;
                        }
                        break;
                    }
                    case MapRegionLoadingStrategyResult.Scheduled:
                    {
                        // not finished yet, will try again during next frame.
                        // in theory: On Pending try again.
                        return IsTimeOutReached();
                    }
                    case MapRegionLoadingStrategyResult.Success:
                    {
                        // job done.
                        job.MarkLoaded();
                        return IsTimeOutReached();
                    }
                    case MapRegionLoadingStrategyResult.Error:
                    {
                        job.MarkFailed();
                        return IsTimeOutReached();
                    }
                    case MapRegionLoadingStrategyResult.Invalid:
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
