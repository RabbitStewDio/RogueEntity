using EnTTSharp.Entities;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Utils;

namespace RogueEntity.Core.Movement.Resistance
{
    public class MovementResistanceTrait<TContext, TItemId, TSense>: StatelessItemComponentTraitBase<TContext, TItemId, MovementCost<TSense>>
        where TItemId : IEntityKey
    {
        readonly MovementCost<TSense> sensoryResistance;

        public MovementResistanceTrait(Percentage blocksSense) : base("Core.Item.SensoryResistance+" + typeof(TSense).Name, 100)
        {
            this.sensoryResistance = new MovementCost<TSense>(blocksSense);
        }

        protected override MovementCost<TSense> GetData(TContext context, TItemId k)
        {
            return sensoryResistance;
        }
    }
}