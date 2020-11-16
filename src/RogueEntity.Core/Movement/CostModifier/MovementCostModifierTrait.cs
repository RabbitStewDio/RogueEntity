using System.Collections.Generic;
using System.Linq;
using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Utils;

namespace RogueEntity.Core.Movement.CostModifier
{
    public class MovementCostModifierTrait<TContext, TItemId, TSense>: StatelessItemComponentTraitBase<TContext, TItemId, MovementCostModifier<TSense>>
        where TItemId : IEntityKey
    {
        readonly MovementCostModifier<TSense> sensoryResistance;

        public MovementCostModifierTrait(Percentage blocksSense) : base("Core.Item.SensoryResistance+" + typeof(TSense).Name, 100)
        {
            this.sensoryResistance = new MovementCostModifier<TSense>(blocksSense);
        }

        protected override MovementCostModifier<TSense> GetData(TContext context, TItemId k)
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
            return Enumerable.Empty<EntityRoleInstance>();
        }
    }
}