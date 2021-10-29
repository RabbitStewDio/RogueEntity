using EnTTSharp.Entities;
using RogueEntity.Core.Inputs.Commands;
using RogueEntity.Core.MapLoading.MapRegions;
using RogueEntity.Core.Players;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Grid;

namespace RogueEntity.Samples.BoxPusher.Core.Commands
{
    /// <summary>
    ///   Reacts to a ResetLevelCommand. This will remove the player from the map,
    ///   and then fire a level loading request for the current level. It also
    ///   installs a handler that resets the current map so that level loading
    ///   recreates an empty slate. 
    /// </summary>
    public class ResetLevelSystem<TActorId>
        where TActorId : IEntityKey
    {
        readonly IMapStateController mapStateController;
        readonly IItemPlacementService<TActorId> placementService;

        public ResetLevelSystem(IMapStateController mapStateController, 
                                IItemPlacementService<TActorId> placementService)
        {
            this.mapStateController = mapStateController;
            this.placementService = placementService;
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
                                             in ResetLevelCommand command,
                                             ref CommandInProgress progress)
        {
            if (v.GetComponent(k, out EntityGridPosition pos) && !pos.IsInvalid)
            {
                v.AssignComponent(k, new ChangeLevelRequest(pos.GridZ));
            }
            
            progress = progress.MarkHandled();
        }
        
        public void ResetMapDataBeforeSpawningPlayer(IEntityViewControl<TActorId> v,
                                                     TActorId k,
                                                     in PlayerTag player,
                                                     in ChangeLevelRequest cmd)
        {
            if (v.GetComponent(k, out EntityGridPosition pos) && !pos.IsInvalid)
            {
                placementService.TryRemoveItem(k, pos);
            }
            
            mapStateController.ResetLevel(cmd.Level);
        }

    }
}
