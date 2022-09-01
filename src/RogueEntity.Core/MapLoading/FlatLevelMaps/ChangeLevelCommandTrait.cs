using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.Inputs.Commands;
using RogueEntity.Core.MapLoading.MapRegions;
using RogueEntity.Core.Meta.Items;
using System;

namespace RogueEntity.Core.MapLoading.FlatLevelMaps
{
    public class ChangeLevelCommandTrait<TActorId> : CommandTraitBase<TActorId, ChangeLevelCommand>,
                                                     IItemComponentTrait<TActorId, ChangeLevelCommandState>
        where TActorId : IEntityKey
    {
        readonly Lazy<IMapRegionMetaDataService<int>> mapData;

        public ChangeLevelCommandTrait(Lazy<IMapRegionMetaDataService<int>> mapData)
        {
            this.mapData = mapData;
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

        public bool TryQuery(IEntityViewControl<TActorId> v, TActorId k, out ChangeLevelCommandState t)
        {
            if (!v.IsValid(k))
            {
                t = default;
                return false;
            }
            
            return v.GetComponent(k, out t);
        }

        public override bool TryRemoveCompletedCommandData(IItemResolver<TActorId> r, TActorId k)
        {
            r.TryRemoveData<ChangeLevelCommandState>(k, out _);
            return base.TryRemoveCompletedCommandData(r, k);
        }

        public bool TryUpdate(IEntityViewControl<TActorId> v, TActorId k, in ChangeLevelCommandState t, out TActorId changedK)
        {
            changedK = k; 
            v.AssignOrReplace(k, t);
            return true;
        }

        bool IItemComponentTrait<TActorId, ChangeLevelCommandState>.TryRemove(IEntityViewControl<TActorId> v, TActorId k, out TActorId changedK)
        {
            v.RemoveComponent<ChangeLevelCommandState>(k);
            changedK = k;
            return true;
        }
    }
}
