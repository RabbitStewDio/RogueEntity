using EnTTSharp.Entities;
using NUnit.Framework;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Sensing;
using RogueEntity.Core.Sensing.Resistance;
using RogueEntity.Core.Tests.Meta.Items;
using RogueEntity.Core.Utils;

namespace RogueEntity.Core.Tests.Sensing.Resistance
{
    [TestFixture]
    public class SensoryResistanceTraitTest : ItemComponentTraitTestBase<BasicItemContext, ItemReference, SensoryResistance<VisionSense>,
        SensoryResistanceTrait<BasicItemContext, ItemReference, VisionSense>>
    {
        protected override IBulkDataStorageMetaData<ItemReference> ItemIdMetaData => new ItemReferenceMetaData();
        protected override EntityRegistry<ItemReference> EntityRegistry => Context.ItemEntities;
        protected override ItemRegistry<BasicItemContext, ItemReference> ItemRegistry => Context.ItemRegistry;

        public SensoryResistanceTraitTest()
        {
            EnableSerializationTest = false; // Temperature is currently implemented as purely static data that is not serialized as it is never changing.
        }

        protected override BasicItemContext CreateContext()
        {
            return new BasicItemContext();
        }

        protected override SensoryResistanceTrait<BasicItemContext, ItemReference, VisionSense> CreateTrait()
        {
            return new SensoryResistanceTrait<BasicItemContext, ItemReference, VisionSense>(Percentage.Of(0.5f));
        }

        protected override IItemComponentTestDataFactory<SensoryResistance<VisionSense>> ProduceTestData(EntityRelations<ItemReference> relations)
        {
            return new ItemComponentTestDataFactory<SensoryResistance<VisionSense>>(
                       new SensoryResistance<VisionSense>(Percentage.Of(0.5f)),
                       new SensoryResistance<VisionSense>(Percentage.Of(0.1f)),
                       new SensoryResistance<VisionSense>(Percentage.Of(0.3f)))
                   .WithRemoveProhibited()
                   .WithUpdateProhibited()
                   .WithApplyRestoresDefaultValue();
        }
    }
}