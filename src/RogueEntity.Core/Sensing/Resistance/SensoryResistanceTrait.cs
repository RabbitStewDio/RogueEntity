using EnTTSharp.Entities;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Utils;

namespace RogueEntity.Core.Sensing.Resistance
{
    public class SensoryResistanceTrait<TContext, TItemId>: StatelessItemComponentTraitBase<TContext, TItemId, SensoryResistance>
        where TItemId : IEntityKey
    {
        readonly SensoryResistance sensoryResistance;

        public SensoryResistanceTrait(Percentage blocksLight, 
                                      Percentage blocksSound = default, 
                                      Percentage blocksHeat = default,
                                      Percentage blocksSmell = default) : base("Core.Item.SensoryResistance", 100)
        {
            this.sensoryResistance = new SensoryResistance(blocksLight, blocksSound, blocksHeat, blocksSmell);
        }

        protected override SensoryResistance GetData(TContext context, TItemId k)
        {
            return sensoryResistance;
        }
    }
}