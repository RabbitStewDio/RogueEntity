using EnTTSharp.Entities;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Utils;

namespace RogueEntity.Core.Movement.Resistance
{
    public class MovementResistanceTrait<TContext, TItemId, TSense>: StatelessItemComponentTraitBase<TContext, TItemId, MovementResistance<TSense>>
        where TItemId : IEntityKey
    {
        readonly MovementResistance<TSense> sensoryResistance;

        public MovementResistanceTrait(Percentage blocksSense) : base("Core.Item.SensoryResistance+" + typeof(TSense).Name, 100)
        {
            this.sensoryResistance = new MovementResistance<TSense>(blocksSense);
        }

        protected override MovementResistance<TSense> GetData(TContext context, TItemId k)
        {
            return sensoryResistance;
        }
    }
}