using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.Inputs.Commands;
using RogueEntity.Core.MapLoading.MapRegions;
using RogueEntity.Core.Positioning;

namespace RogueEntity.Generator.Commands
{
    public class ChangeLevelCommandSystem<TActor>
        where TActor : IEntityKey
    {
        readonly IMapLevelMetaDataService mapMetaDataService; 
        readonly IItemResolver<TActor> itemResolver;

        public ChangeLevelCommandSystem(IMapLevelMetaDataService mapMetaDataService, 
                                        IItemResolver<TActor> itemResolver)
        {
            this.mapMetaDataService = mapMetaDataService;
            this.itemResolver = itemResolver;
        }

        public void ProcessCommand(IEntityViewControl<TActor> v, TActor k, in ChangeLevelCommand cmd, ref CommandInProgress cip)
        {
            if (!mapMetaDataService.TryGetMetaData(cmd.Level, out _))
            {
                // error:
                cip = cip.MarkHandled();
                return;
            }

            if (itemResolver.TryQueryData(k, out Position pos))
            {
                if (pos.GridZ == cmd.Level)
                {
                    // already spawned.
                    cip = cip.MarkHandled();
                    return;
                }
            }
            
            v.AssignOrReplace(k, new ChangeLevelRequest(cmd.Level));
        }
    }
}
