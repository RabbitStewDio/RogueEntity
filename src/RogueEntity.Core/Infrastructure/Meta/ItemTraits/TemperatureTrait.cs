using RogueEntity.Core.Infrastructure.Meta.Items;

namespace RogueEntity.Core.Infrastructure.Meta.ItemTraits
{
    public class TemperatureTrait<TContext, TItemId>: StatelessItemComponentTraitBase<TContext, TItemId, Temperature> 
        where TItemId : IBulkDataStorageKey<TItemId>
    {
        readonly Temperature temperature;

        public TemperatureTrait(Temperature temperature) : base("Core.Item.Temperature", 100)
        {
            this.temperature = temperature;
        }

        protected override Temperature GetData(TContext context)
        {
            return temperature;
        }
    }
}