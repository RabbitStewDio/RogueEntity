using EnTTSharp.Entities;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Meta.ItemTraits;
using RogueEntity.Core.Tests.Meta.Items;
using RogueEntity.Core.Utils;

namespace RogueEntity.Core.Tests.Meta.ItemTraits
{
    public class SensoryResistanceTraitTest : ItemComponentTraitTestBase<BasicItemContext, ItemReference, SensoryResistance, SensoryResistanceTrait<BasicItemContext, ItemReference>>
    {
        protected override EntityRegistry<ItemReference> EntityRegistry => Context.EntityRegistry;
        protected override ItemRegistry<BasicItemContext, ItemReference> ItemRegistry => Context.ItemRegistry;

        public SensoryResistanceTraitTest()
        {
            EnableSerializationTest = false; // Temperature is currently implemented as purely static data that is not serialized as it is never changing.
        }

        protected override BasicItemContext CreateContext()
        {
            return new BasicItemContext();
        }

        protected override SensoryResistanceTrait<BasicItemContext, ItemReference> CreateTrait()
        {
            return new SensoryResistanceTrait<BasicItemContext, ItemReference>(Percentage.Of(0.5f), Percentage.Of(0.6f), Percentage.Of(0.7f), Percentage.Of(0.8f));
        }

        public override IItemComponentTestDataFactory<SensoryResistance> ProduceTestData(EntityRelations<ItemReference> relations)
        {
            return new ItemComponentTestDataFactory<SensoryResistance>(new SensoryResistance(Percentage.Of(0.5f), Percentage.Of(0.6f), Percentage.Of(0.7f), Percentage.Of(0.8f)),
                                                                       new SensoryResistance(Percentage.Of(0.1f), Percentage.Of(0.2f), Percentage.Of(0.3f), Percentage.Of(0.4f)),
                                                                       new SensoryResistance(Percentage.Of(0.3f), Percentage.Of(0.4f), Percentage.Of(0.5f), Percentage.Of(0.6f)))
                   .WithRemoveProhibited()
                   .WithUpdateProhibited()
                   .WithApplyRestoresDefaultValue();
        }
    }
}