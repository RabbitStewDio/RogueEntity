using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.Inputs.Commands;
using RogueEntity.Generator;
using System;
using System.Collections.Generic;

namespace RogueEntity.Core.MapLoading.FlatLevelMaps
{
    public class ChangeLevelCommandTrait<TActorId> : CommandTraitBase<TActorId, ChangeLevelCommand>
        where TActorId : IEntityKey
    {
        readonly Lazy<IMapRegionMetaDataService<int>> mapData;

        public ChangeLevelCommandTrait(Lazy<IMapRegionMetaDataService<int>> mapData)
        {
            this.mapData = mapData;
        }

        public override IEnumerable<EntityRoleInstance> GetEntityRoles()
        {
            yield return CreateDefaultEntityRoleInstance();
            yield return CommandRoles.CreateRoleFor(CommandType.Of<ChangeLevelCommand>()).Instantiate<TActorId>();
        }

        public override bool IsCommandValidForState(TActorId actor, ChangeLevelCommand cmd)
        {
            if (mapData.Value.TryGetRegionBounds(cmd.Level, out _))
            {
                // level is at least theoretically available.
                return true;
            }
            return false;
        }
    }
}
