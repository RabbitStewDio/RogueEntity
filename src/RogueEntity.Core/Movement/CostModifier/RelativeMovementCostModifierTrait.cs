using System.Collections.Generic;
using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Movement.MovementModes;

namespace RogueEntity.Core.Movement.CostModifier
{
    public class RelativeMovementCostModifierTrait<TContext, TItemId, TMovementMode>: StatelessItemComponentTraitBase<TContext, TItemId, RelativeMovementCostModifier<TMovementMode>>
        where TItemId : IEntityKey
    {
        readonly RelativeMovementCostModifier<TMovementMode> sensoryResistance;

        public RelativeMovementCostModifierTrait(RelativeMovementCostModifier<TMovementMode> movementCostModifier) : base("Core.Item.RelativeMovementCostModifier+" + typeof(TMovementMode).Name, 100)
        {
            this.sensoryResistance = movementCostModifier;
        }

        protected override RelativeMovementCostModifier<TMovementMode> GetData(TContext context, TItemId k)
        {
            return sensoryResistance;
        }

        /// <summary>
        ///   Defining movement cost modifiers does not imply that a given movement mode is actually used.
        ///   Only actors with a given movement mode can enable the relevant systems. 
        /// </summary>
        /// <returns></returns>
        public override IEnumerable<EntityRoleInstance> GetEntityRoles()
        {
            yield return MovementModules.GetCostModifierSourceRole<TMovementMode>().Instantiate<TItemId>();
        }
    }
}