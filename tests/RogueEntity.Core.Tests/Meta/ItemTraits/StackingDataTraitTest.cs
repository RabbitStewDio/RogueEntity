using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Meta.ItemTraits;
using RogueEntity.Core.Tests.Meta.Items;

namespace RogueEntity.Core.Tests.Meta.ItemTraits
{
    public class StackingDataTraitTest : ItemComponentTraitTestBase<BasicItemContext, ItemReference, StackCount, StackingBulkTrait<BasicItemContext, ItemReference>>
    {
        public StackingDataTraitTest() : base(new ItemReferenceMetaData())
        {
        }

        protected override BasicItemContext CreateContext()
        {
            return new BasicItemContext();
        }

        protected override StackingBulkTrait<BasicItemContext, ItemReference> CreateTrait()
        {
            return new StackingBulkTrait<BasicItemContext, ItemReference>(1, 100);
        }

        protected override IItemComponentTestDataFactory<StackCount> ProduceTestData(EntityRelations<ItemReference> relations)
        {
            return new ItemComponentTestDataFactory<StackCount>(StackCount.Of(1, 100), 
                                                                StackCount.Of(20, 100), 
                                                                StackCount.Of(100, 100))
                .WithRemoveProhibited();
        }
    }
}