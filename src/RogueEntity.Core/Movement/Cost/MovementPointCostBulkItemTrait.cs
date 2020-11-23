using System.Collections.Generic;
using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Movement.MovementModes;
using RogueEntity.Core.Utils.Algorithms;

namespace RogueEntity.Core.Movement.Cost
{
    public class MovementPointCostBulkItemTrait<TGameContext, TActorId, TMovementMode> : SimpleBulkItemComponentTraitBase<TGameContext, TActorId, MovementPointCost<TMovementMode>>,
                                                                                         IItemComponentInformationTrait<TGameContext, TActorId, MovementCost>
        where TActorId : IBulkDataStorageKey<TActorId>
        where TMovementMode : IMovementMode
    {
        readonly TMovementMode movementMode;
        readonly DistanceCalculation movementStyle;
        readonly float standardMovementCost;
        readonly int movementModePreference;

        public MovementPointCostBulkItemTrait(TMovementMode movementMode,
                                              DistanceCalculation movementStyle,
                                              float standardMovementCost,
                                              int movementModePreference) :
            base("Core.Traits.Movement.MovementPointCosts+" + typeof(TMovementMode).Name, 100)
        {
            this.movementMode = movementMode;
            this.movementStyle = movementStyle;
            this.standardMovementCost = standardMovementCost;
            this.movementModePreference = movementModePreference;
        }

        protected override MovementPointCost<TMovementMode> CreateInitialValue(TGameContext c, TActorId reference)
        {
            return new MovementPointCost<TMovementMode>(standardMovementCost);
        }

        public bool TryQuery(IEntityViewControl<TActorId> v, TGameContext context, TActorId k, out MovementCost t)
        {
            if (TryQuery(v, context, k, out MovementPointCost<TMovementMode> pointCost) &&
                pointCost.Cost > 0)
            {
                t = new MovementCost(movementMode, movementStyle, pointCost.Cost, movementModePreference);
                return true;
            }

            t = default;
            return false;
        }

        protected override bool ValidateData(IEntityViewControl<TActorId> entityViewControl, TGameContext context, in TActorId itemReference, in MovementPointCost<TMovementMode> data)
        {
            return data.Cost > 0;
        }

        public override IEnumerable<EntityRoleInstance> GetEntityRoles()
        {
            yield return MovementModules.GetMovableActorRole<TMovementMode>().Instantiate<TActorId>();
        }
    }
}