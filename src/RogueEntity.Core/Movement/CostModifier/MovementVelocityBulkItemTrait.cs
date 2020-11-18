using System.Collections.Generic;
using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Movement.MovementModes;

namespace RogueEntity.Core.Movement.CostModifier
{
    public class MovementVelocityBulkItemTrait<TGameContext, TActorId, TMovementMode> : SimpleBulkItemComponentTraitBase<TGameContext, TActorId, MovementVelocity<TMovementMode>>,
                                                                                        IItemComponentInformationTrait<TGameContext, TActorId, MovementCost>
        where TActorId : IBulkDataStorageKey<TActorId>
        where TMovementMode : IMovementMode
    {
        readonly TMovementMode movementMode;
        readonly float standardMovementCost;
        readonly int movementModePreference;

        public MovementVelocityBulkItemTrait(TMovementMode movementMode,
                                             float standardMovementCost,
                                             int movementModePreference) :
            base("Core.Traits.Movement.MovementVelocity+" + typeof(TMovementMode).Name, 100)
        {
            this.movementMode = movementMode;
            this.standardMovementCost = standardMovementCost;
            this.movementModePreference = movementModePreference;
        }

        protected override MovementVelocity<TMovementMode> CreateInitialValue(TGameContext c, TActorId reference)
        {
            return new MovementVelocity<TMovementMode>(standardMovementCost);
        }

        public bool TryQuery(IEntityViewControl<TActorId> v, TGameContext context, TActorId k, out MovementCost t)
        {
            if (TryQuery(v, context, k, out MovementVelocity<TMovementMode> pointCost) && pointCost.Velocity > 0)
            {
                t = new MovementCost(movementMode, 1 / pointCost.Velocity, movementModePreference);
                return true;
            }

            t = default;
            return false;
        }

        protected override bool ValidateData(IEntityViewControl<TActorId> entityViewControl, TGameContext context, in TActorId itemReference, in MovementVelocity<TMovementMode> data)
        {
            return data.Velocity > 0;
        }

        public override IEnumerable<EntityRoleInstance> GetEntityRoles()
        {
            yield return MovementModules.GetMovableActorRole<TMovementMode>().Instantiate<TActorId>();
        }
    }
}