using RogueEntity.Core.Infrastructure.Meta.Items;
using RogueEntity.Core.Utils;

namespace RogueEntity.Core.Infrastructure.Meta.ItemTraits
{
    public class SensoryResistanceTrait<TContext, TItemId>: StatelessItemComponentTraitBase<TContext, TItemId, SensoryResistance> 
        where TItemId : IBulkDataStorageKey<TItemId>
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