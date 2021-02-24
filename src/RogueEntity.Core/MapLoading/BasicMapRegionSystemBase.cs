using EnTTSharp.Entities;
using JetBrains.Annotations;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Players;
using RogueEntity.Core.Positioning;
using System;
using System.Diagnostics;

namespace RogueEntity.Core.MapLoading
{
    public abstract class BasicMapRegionSystemBase: IMapRegionSystem
    {
        readonly TimeSpan maximumProcessingTime;
        readonly IMapRegionLoaderService<int> mapLoaderService;
        readonly Stopwatch processingTimeStopWatch;

        protected BasicMapRegionSystemBase([NotNull] IMapRegionLoaderService<int> mapLoaderService, TimeSpan maximumProcessingTime = default)
        {
            this.mapLoaderService = mapLoaderService ?? throw new ArgumentNullException(nameof(mapLoaderService));
            this.maximumProcessingTime = NormalizeMaximumLoadingTime(maximumProcessingTime);
            this.processingTimeStopWatch = new Stopwatch();
        }

        static TimeSpan NormalizeMaximumLoadingTime(TimeSpan maximumProcessingTime)
        {
            return maximumProcessingTime <= TimeSpan.Zero ? TimeSpan.FromMilliseconds(5) : maximumProcessingTime;
        }

        /// <summary>
        ///   Invoked when a new player has spawned. This uses some built-in default
        ///   to place the player in the first level. 
        /// </summary>
        public void RequestLoadLevelFromNewPlayer<TItemId>(IEntityViewControl<TItemId> v,
                                                           TItemId k,
                                                           in PlayerObserverTag player,
                                                           in NewPlayerTag newPlayerTag)
            where TItemId : IEntityKey
        {
            if (!CreateInitialLevelRequest(v, k, player).TryGetValue(out var cmd))
            {
                return;
            }

            v.AssignComponent(k, cmd);
            v.RemoveComponent<NewPlayerTag>(k);
        }

        protected abstract Optional<ChangeLevelCommand> CreateInitialLevelRequest<TItemId>(IEntityViewControl<TItemId> v,
                                                                                           TItemId k,
                                                                                           in PlayerObserverTag player)
            where TItemId : IEntityKey;

        /// <summary>
        ///   Invoked when a existing player requests to be moved to a different level.
        ///   May not be appropriate for all game types. Also used when a player is moving
        ///   into a new level by entering a stair case or portal, where the player
        ///   has no control over where the end point of the portal lies. 
        /// </summary>
        public void RequestLoadLevelFromChangeLevelCommand<TItemId>(IEntityViewControl<TItemId> v,
                                                                    TItemId k,
                                                                    in PlayerObserverTag player,
                                                                    in ChangeLevelCommand cmd)
            where TItemId : IEntityKey
        {
            var level = cmd.Level;
            var status = mapLoaderService.RequestImmediateLoading(level);
            v.AssignOrReplace(k, status);
        }

        /// <summary>
        ///   Invoked when a player is moving into a new level by falling or by knowing where
        ///   the end point of a given portal is placed. Useful for stairs that should line
        ///   up across levels or for jumping down a hole in the ground. 
        /// </summary>
        public void RequestLoadLevelFromChangePositionCommand<TItemId>(IEntityViewControl<TItemId> v,
                                                                       TItemId k,
                                                                       in PlayerObserverTag player,
                                                                       in ChangeLevelPositionCommand cmd)
            where TItemId : IEntityKey
        {
            if (cmd.Position.IsInvalid)
            {
                return;
            }

            var level = cmd.Position.GridZ;
            var status = mapLoaderService.RequestImmediateLoading(level);
            v.AssignOrReplace(k, status);
        }
        
        /// <summary>
        ///    A basic driver function that loads the next requested chunk.
        /// </summary>
        public void LoadChunks()
        {
            processingTimeStopWatch.Restart();
            try
            {
                while (mapLoaderService.PerformLoadNextChunk())
                {
                    if (processingTimeStopWatch.Elapsed >= maximumProcessingTime)
                    {
                        return;
                    }
                }
            }
            finally
            {
                processingTimeStopWatch.Stop();
            }
        }
    }
}
