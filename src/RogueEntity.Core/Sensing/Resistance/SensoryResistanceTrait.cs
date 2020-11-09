using EnTTSharp.Entities;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Utils;

namespace RogueEntity.Core.Sensing.Resistance
{
    public class SensoryResistanceTrait<TContext, TItemId, TSense>: StatelessItemComponentTraitBase<TContext, TItemId, SensoryResistance<TSense>>
        where TItemId : IEntityKey
    {
        readonly SensoryResistance<TSense> sensoryResistance;

        public SensoryResistanceTrait(SensoryResistance<TSense> sensoryResistance) : base("Core.Item.SensoryResistance+" + typeof(TSense).Name, 100)
        {
            this.sensoryResistance = sensoryResistance;
        }

        public SensoryResistanceTrait(Percentage blocksSense) : this(new SensoryResistance<TSense>(blocksSense))
        {
        }

        protected override SensoryResistance<TSense> GetData(TContext context, TItemId k)
        {
            return sensoryResistance;
        }
    }
}