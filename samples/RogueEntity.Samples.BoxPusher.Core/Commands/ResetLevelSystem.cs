using EnTTSharp.Entities;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Inputs.Commands;
using RogueEntity.Core.MapLoading.FlatLevelMaps;
using RogueEntity.Core.MapLoading.MapRegions;
using RogueEntity.Core.Players;
using RogueEntity.Core.Positioning.Grid;
using Serilog;

namespace RogueEntity.Samples.BoxPusher.Core.Commands
{
    /// <summary>
    ///   Reacts to a ResetLevelCommand. This will first issue a unload/evict request
    ///   followed by a change-level request.
    /// </summary>
    public class ResetLevelSystem<TActorId>
        where TActorId : IEntityKey
    {
        readonly ILogger logger = SLog.ForContext<ResetLevelSystem<TActorId>>();
        readonly IMapRegionTrackerService<int> mapTracker;

        public ResetLevelSystem(IMapRegionTrackerService<int> mapTracker)
        {
            this.mapTracker = mapTracker;
        }

        /// <summary>
        ///   Translate a ResetLevelCommand from the UI into a internal ChangeLevelRequest
        ///   that triggers a respawn of the player. 
        /// </summary>
        /// <param name="v"></param>
        /// <param name="k"></param>
        /// <param name="command"></param>
        /// <param name="progress"></param>
        public void ProcessResetLevelCommand(IEntityViewControl<TActorId> v,
                                             TActorId k,
                                             in PlayerTag player,
                                             in ResetLevelCommand command,
                                             ref CommandInProgress progress)
        {
            if (v.HasComponent<ChangeLevelRequest>(k) ||
                v.HasComponent<EvictLevelRequest>(k))
            {
                logger.Debug("Skipping reset-level command - process still running");
                return;
            }
            
            if (v.GetComponent(k, out EntityGridPosition pos) && !pos.IsInvalid)
            {
                // if the player is in an existing level, remove that level first.
                logger.Debug("Received reset level command: Begin eviction of level {z}", pos.GridZ);
                v.AssignComponent(k, new EvictLevelRequest(pos.GridZ));
            }
            else
            {
                // ignore the command, we have no current level.
                logger.Debug("Received reset level command without placed player");
                progress = progress.MarkHandled();
            }
        }


        public void EvictOldLevelOnChangeLevelCommand(IEntityViewControl<TActorId> v,
                                                      TActorId k,
                                                      in PlayerTag player,
                                                      in ChangeLevelCommand command,
                                                      in CommandInProgress progress)
        {
            if (v.GetComponent(k, out EntityGridPosition pos) && !pos.IsInvalid)
            {
                if (pos.GridZ != command.Level)
                {
                    // player is in an existing level. We can clear that out.
                    v.AssignComponent(k, new EvictLevelRequest(pos.GridZ));
                }
            }
        }

        public void ResetMapDataBeforeSpawningPlayer(IEntityViewControl<TActorId> v,
                                                     TActorId k,
                                                     in PlayerTag player,
                                                     in EvictLevelRequest cmd)
        {

            if (mapTracker.IsRegionEvicted(cmd.Level))
            {
                logger.Debug("After evicting level {Level}, proceed to reload level");
                v.RemoveComponent<EvictLevelRequest>(k);
                v.AssignComponent(k, new ChangeLevelRequest(cmd.Level));
            }
            else
            {
                mapTracker.EvictRegion(cmd.Level);
            }
        }

        public void FinalizeResetLevelCommand(IEntityViewControl<TActorId> v,
                                              TActorId k,
                                              in PlayerTag player,
                                              in ChangeLevelRequest levelChange,
                                              in ResetLevelCommand cmd,
                                              ref CommandInProgress progress)
        {
            if (mapTracker.IsRegionLoaded(levelChange.Level))
            {
                progress = progress.MarkHandled();
            }
        }
    }
}
