using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Inputs.Commands;
using RogueEntity.Core.MapLoading.MapRegions;
using RogueEntity.Core.Positioning;
using RogueEntity.Generator;
using Serilog;

namespace RogueEntity.Core.MapLoading.FlatLevelMaps
{
    public class ChangeLevelCommandSystem<TActor>
        where TActor : IEntityKey
    {
        readonly ILogger logger = SLog.ForContext<ChangeLevelCommandSystem<TActor>>();
        
        readonly IMapRegionMetaDataService<int> mapMetaDataService; 
        readonly IItemResolver<TActor> itemResolver;
        readonly IItemPlacementService<TActor> itemPlacementService;

        public ChangeLevelCommandSystem(IMapRegionMetaDataService<int> mapMetaDataService, 
                                        IItemResolver<TActor> itemResolver,
                                        IItemPlacementService<TActor> itemPlacementService)
        {
            this.mapMetaDataService = mapMetaDataService;
            this.itemResolver = itemResolver;
            this.itemPlacementService = itemPlacementService;
        }

        public void ProcessCommand(IEntityViewControl<TActor> v, TActor k, in ChangeLevelCommand cmd, ref CommandInProgress cip)
        {
            if (!mapMetaDataService.TryGetRegionBounds(cmd.Level, out _))
            {
                logger.Error("Player {Player} cannot change to invalid level {Level}; Aborting command processing", k, cmd.Level);
                // error:
                cip = cip.MarkHandled();
                return;
            }

            if (itemResolver.TryQueryData(k, out Position pos))
            {
                if (pos.GridZ == cmd.Level)
                {
                    logger.Information("Player {Player} has transferred to requested level {Level}", k, cmd.Level);
                    // already spawned.
                    cip = cip.MarkHandled();
                    return;
                }
            }

            if (!v.GetComponent(k, out ChangeLevelRequest r))
            {
                if (itemResolver.TryQueryData(k, out Position playerPos))
                {
                    if (!itemPlacementService.TryRemoveItem(k, playerPos))
                    {
                        logger.Warning("Unable to remove entity {Player} from its current position", k);
                        cip = cip.MarkHandled();
                        return;
                    }
                }
                
                logger.Debug("Player {Player} wants to transfer to {Level} - filing change-level-request", k, cmd.Level);
                v.AssignOrReplace(k, new ChangeLevelRequest(cmd.Level));
            }
        }
    }
}
