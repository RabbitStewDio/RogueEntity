using EnTTSharp.Entities;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Meta.ItemTraits;

namespace RogueEntity.Core.Tests.Meta.Items
{
    public class TemperatureDataTraitTest : ItemComponentTraitTestBase<BasicItemContext, ItemReference, Temperature, TemperatureTrait<BasicItemContext, ItemReference>>
    {
        protected override EntityRegistry<ItemReference> EntityRegistry => Context.EntityRegistry;
        protected override ItemRegistry<BasicItemContext, ItemReference> ItemRegistry => Context.ItemRegistry;

        public TemperatureDataTraitTest()
        {
            EnableSerializationTest = false; // Temperature is currently implemented as purely static data that is not serialized as it is never changing.
        }

        protected override BasicItemContext CreateContext()
        {
            return new BasicItemContext();
        }

        protected override TemperatureTrait<BasicItemContext, ItemReference> CreateTrait()
        {
            return new TemperatureTrait<BasicItemContext, ItemReference>(Temperature.FromCelsius(30));
        }

        public override IItemComponentTestDataFactory<Temperature> ProduceTestData(EntityRelations<ItemReference> relations)
        {
            return new ItemComponentTestDataFactory<Temperature>(Temperature.FromCelsius(30),
                                                                 Temperature.FromCelsius(40),
                                                                 Temperature.FromCelsius(400))
                .WithRemoveProhibited()
                .WithUpdateProhibited()
                .WithApplyRestoresDefaultValue();
        }
    }
}