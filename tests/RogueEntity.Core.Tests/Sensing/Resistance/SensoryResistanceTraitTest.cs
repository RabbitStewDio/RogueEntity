using NUnit.Framework;
using RogueEntity.Core.Sensing;
using RogueEntity.Core.Sensing.Resistance;
using RogueEntity.Core.Tests.Meta.Items;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Meta.EntityKeys;

namespace RogueEntity.Core.Tests.Sensing.Resistance
{
    [TestFixture]
    public class SensoryResistanceTraitTest : ItemComponentTraitTestBase<ItemReference, SensoryResistance<VisionSense>, SensoryResistanceTrait<ItemReference, VisionSense>>
    {
        public SensoryResistanceTraitTest(): base(new ItemReferenceMetaData())
        {
            EnableSerializationTest = false; // Temperature is currently implemented as purely static data that is not serialized as it is never changing.
        }

        protected override SensoryResistanceTrait<ItemReference, VisionSense> CreateTrait()
        {
            return new SensoryResistanceTrait<ItemReference, VisionSense>(Percentage.Of(0.5f));
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