using EnTTSharp.Entities;
using JetBrains.Annotations;
using RogueEntity.Core.MapLoading.MapRegions;
using System;

namespace RogueEntity.Core.MapLoading.FlatLevelMaps
{
    public class BasicFlatLevelRegionRequestHandlerSystem : IFlatLevelRegionRequestHandlerSystem
    {
        readonly IMapRegionTrackerService<int> mapTrackerService;

        public BasicFlatLevelRegionRequestHandlerSystem([NotNull] IMapRegionTrackerService<int> mapTrackerService)
        {
            this.mapTrackerService = mapTrackerService ?? throw new ArgumentNullException(nameof(mapTrackerService));
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
            mapTrackerService.RequestImmediateLoading(level);
        }

        public void RequestEvictLevelFromRequest<TItemId>(IEntityViewControl<TItemId> v,
                                                          TItemId k,
                                                          in EvictLevelRequest cmd)
            where TItemId : IEntityKey
        {
            var level = cmd.Level;
            if (mapTrackerService.IsRegionEvicted(level))
            {
                v.RemoveComponent<EvictLevelRequest>(k);
                return;
            }

            if (mapTrackerService.QueryRegionStatus(level) != MapRegionStatus.UnloadingRequested)
            {
                mapTrackerService.EvictRegion(level);
            }
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
            mapTrackerService.RequestImmediateLoading(level);
        }


    }
}
