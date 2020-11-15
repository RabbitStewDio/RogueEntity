using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Meta.ItemTraits;
using RogueEntity.Core.Tests.Meta.Items;

namespace RogueEntity.Core.Tests.Meta.ItemTraits
{
    public class DurabilityDataTraitTest : ItemComponentTraitTestBase<BasicItemContext, ItemReference, Durability, DurabilityTrait<BasicItemContext, ItemReference>>
    {
        public DurabilityDataTraitTest() : base(new ItemReferenceMetaData())
        {
        }

        protected override BasicItemContext CreateContext()
        {
            return new BasicItemContext();
        }

        protected override DurabilityTrait<BasicItemContext, ItemReference> CreateTrait()
        {
            return new DurabilityTrait<BasicItemContext, ItemReference>(1, 100);
        }

        protected override IItemComponentTestDataFactory<Durability> ProduceTestData(EntityRelations<ItemReference> relations)
        {
            return new ItemComponentTestDataFactory<Durability>(new Durability(1, 100),
                                                                new Durability(10, 100),
                                                                new Durability(100, 100))
                   .WithRemoveProhibited();
        }
    }
}