using RogueEntity.Core.Meta.ItemTraits;
using RogueEntity.Core.Tests.Meta.Items;
using RogueEntity.Core.Meta.EntityKeys;

namespace RogueEntity.Core.Tests.Meta.ItemTraits
{
    public class TemperatureDataTraitTest : ItemComponentTraitTestBase<ItemReference, Temperature, TemperatureTrait<ItemReference>>
    {

        public TemperatureDataTraitTest(): base(new ItemReferenceMetaData())
        {
            EnableSerializationTest = false; // Temperature is currently implemented as purely static data that is not serialized as it is never changing.
        }

        protected override TemperatureTrait<ItemReference> CreateTrait()
        {
            return new TemperatureTrait<ItemReference>(Temperature.FromCelsius(30));
        }

        protected override IItemComponentTestDataFactory<Temperature> ProduceTestData(EntityRelations<ItemReference> relations)
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