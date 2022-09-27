using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Inputs.Commands;
using RogueEntity.Core.MapLoading.MapRegions;
using RogueEntity.Core.Positioning;
using Serilog;

namespace RogueEntity.Core.MapLoading.FlatLevelMaps
{
    public class ChangeLevelCommandSystem<TActor>
        where TActor : struct, IEntityKey
    {
        readonly ILogger logger = SLog.ForContext<ChangeLevelCommandSystem<TActor>>();
        
        readonly IMapRegionMetaDataService<int> mapMetaDataService; 
        readonly IMapRegionTrackerService<int> mapTracker;
        readonly IItemResolver<TActor> itemResolver;
        readonly IItemPlacementService<TActor> itemPlacementService;
        readonly FlatLevelMapConfiguration config;

        public ChangeLevelCommandSystem(IMapRegionMetaDataService<int> mapMetaDataService,
                                        IMapRegionTrackerService<int> mapTracker, 
                                        IItemResolver<TActor> itemResolver,
                                        IItemPlacementService<TActor> itemPlacementService,
                                        FlatLevelMapConfiguration config)
        {
            this.mapMetaDataService = mapMetaDataService;
            this.itemResolver = itemResolver;
            this.itemPlacementService = itemPlacementService;
            this.config = config;
            this.mapTracker = mapTracker;
        }

        public void ProcessCommand(IEntityViewControl<TActor> v, TActor k, in ChangeLevelCommand cmd, ref CommandInProgress cip)
        {
            if (!v.GetComponent(k, out ChangeLevelCommandState state))
            {
                state = ChangeLevelCommandState.Start;
            }

            logger.Debug("Processing ChangeLevel command for entity {Entity} at state {State}", k, state);

            switch (state)
            {
                // A new request. Validate the incoming data (to make sure the level actually exists,
                case ChangeLevelCommandState.Start:
                case ChangeLevelCommandState.PlayerRemoved:
                    state = StartCommandProcessing(v, k, cmd);
                    break;
                case ChangeLevelCommandState.WaitingForReset:
                    state = WaitForLevelReset(v, k, cmd);
                    break;
                case ChangeLevelCommandState.WaitingForLoad:
                    state = WaitForLevelLoading(v, k, cmd);
                    break;
                case ChangeLevelCommandState.PlayerPlaced:
                    break;
            }

            if (state is ChangeLevelCommandState.PlayerPlaced or ChangeLevelCommandState.Aborted)
            {
                v.RemoveComponent<ChangeLevelCommandState>(k);
                v.RemoveComponent<ChangeLevelCommand>(k);
                cip = cip.MarkHandled();
            }
            else
            {
                v.AssignOrReplace(k, state);
            }
        }

        ChangeLevelCommandState WaitForLevelLoading(IEntityViewControl<TActor> v, TActor k, ChangeLevelCommand cmd)
        {
            if (itemResolver.TryQueryData(k, out Position pos) && !pos.IsInvalid)
            {
                if (pos.GridZ == cmd.Level)
                {
                    logger.Information("Player {Player} has transferred to requested level {Level} at position {Position}", k, cmd.Level, pos);
                    // already spawned.
                    v.RemoveComponent<ChangeLevelRequest>(k);
                    return ChangeLevelCommandState.PlayerPlaced;
                }
            }

            return ChangeLevelCommandState.WaitingForLoad;
        }
        
        ChangeLevelCommandState WaitForLevelReset(IEntityViewControl<TActor> v, TActor k, ChangeLevelCommand cmd)
        {
            // Whilst waiting for resets of level data, the player will have a EvictLevelRequest 
            // object assigned. We will repeatedly request to evict the given region until the
            // region has been evicted at least once.
            // 
            // Eviction can be prevented or cancelled by removing the EvictLevelRequest object 
            // from the player. We then assume that some other process performed the reset.
            if (!v.GetComponent(k, out EvictLevelRequest _))
            {
                logger.Debug("Player {Player} proceeds to transfer to {Level} after eviction of level data was cancelled - filing change-level-request", k, cmd.Level);
                v.AssignOrReplace(k, new ChangeLevelRequest(cmd.Level));
                return ChangeLevelCommandState.WaitingForLoad;
            }

            if (mapTracker.IsRegionEvicted(cmd.Level))
            {
                logger.Debug("Player {Player} wants to transfer to {Level} after eviction of level data - filing change-level-request", k, cmd.Level);
                v.AssignOrReplace(k, new ChangeLevelRequest(cmd.Level));
                v.RemoveComponent<EvictLevelRequest>(k);
                return ChangeLevelCommandState.WaitingForLoad;
            }

            return ChangeLevelCommandState.WaitingForReset;
        }

        ChangeLevelCommandState StartCommandProcessing(IEntityViewControl<TActor> v, TActor k, ChangeLevelCommand cmd)
        {
            if (!mapMetaDataService.TryGetRegionBounds(cmd.Level, out _))
            {
                logger.Error("Player {Player} cannot change to invalid level {Level}; Aborting command processing", k, cmd.Level);
                return ChangeLevelCommandState.Aborted;
            }

            if (itemResolver.TryQueryData(k, out Position playerPos))
            {
                if (!playerPos.IsInvalid)
                {
                    if (!itemPlacementService.TryRemoveItem(k, playerPos))
                    {
                        logger.Warning("Unable to remove entity {Player} from its current position", k);
                        return ChangeLevelCommandState.Aborted;
                    }
                    
                    logger.Debug("Removed {Player} from its current position in preparation for level change to {Level}", k, cmd.Level);
                }
            }

            if (config.ChangingLevelResetsMapData)
            {
                logger.Debug("Player {Player} wants to reset level {Level} before entering level", k, cmd.Level);
                v.AssignOrReplace(k, new EvictLevelRequest(cmd.Level));
                return ChangeLevelCommandState.WaitingForReset;
            }

            logger.Debug("Player {Player} wants to transfer to {Level} - filing change-level-request", k, cmd.Level);
            v.AssignOrReplace(k, new ChangeLevelRequest(cmd.Level));
            return ChangeLevelCommandState.WaitingForLoad;
        }
    }
}
