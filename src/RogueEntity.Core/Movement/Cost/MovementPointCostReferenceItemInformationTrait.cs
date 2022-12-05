using EnTTSharp;
using System.Collections.Generic;
using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Movement.MovementModes;
using RogueEntity.Core.Positioning.Algorithms;

namespace RogueEntity.Core.Movement.Cost
{
    public class MovementPointCostReferenceItemInformationTrait< TActorId, TMovementMode> : SimpleReferenceItemComponentTraitBase< TActorId, MovementPointCost<TMovementMode>>,
                                                                                 IMovementCostTrait<TActorId>,
                                                                                 IMovementStyleInformationTrait
        where TActorId : struct, IEntityKey
        where TMovementMode : IMovementMode
    {
        readonly IMovementMode movementModeBoxed;
        readonly DistanceCalculation movementStyle;
        readonly float standardMovementCost;
        readonly int movementModePreference;

        public MovementPointCostReferenceItemInformationTrait(TMovementMode movementMode,
                                                   DistanceCalculation movementStyle,
                                                   float standardMovementCost,
                                                   int movementModePreference) :
            base("Core.Traits.Movement.MovementPointCosts+" + typeof(TMovementMode).Name, 100)
        {
            this.movementModeBoxed = movementMode;
            this.movementStyle = movementStyle;
            this.standardMovementCost = standardMovementCost;
            this.movementModePreference = movementModePreference;
        }

        public bool TryQuery(out (IMovementMode, DistanceCalculation) t)
        {
            t = (movementModeBoxed, movementStyle);
            return true;
        }

        protected override Optional<MovementPointCost<TMovementMode>> CreateInitialValue( TActorId reference)
        {
            return new MovementPointCost<TMovementMode>(standardMovementCost);
        }

        public bool TryQuery(IEntityViewControl<TActorId> v, TActorId k, out MovementCost t)
        {
            if (TryQuery(v,  k, out MovementPointCost<TMovementMode> pointCost) &&
                pointCost.Cost > 0)
            {
                t = new MovementCost(movementModeBoxed, movementStyle, pointCost.Cost, movementModePreference);
                return true;
            }

            t = default;
            return false;
        }

        protected override bool ValidateData(IEntityViewControl<TActorId> entityViewControl,  in TActorId itemReference, in MovementPointCost<TMovementMode> data)
        {
            return data.Cost > 0;
        }

        public override IEnumerable<EntityRoleInstance> GetEntityRoles()
        {
            yield return MovementModules.GetMovableActorWithPointsRole<TMovementMode>().Instantiate<TActorId>();
        }
    }
}