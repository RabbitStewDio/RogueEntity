using RogueEntity.Core.Meta.ItemTraits;
using RogueEntity.Core.Tests.Meta.Items;
using RogueEntity.Core.Meta.EntityKeys;

namespace RogueEntity.Core.Tests.Meta.ItemTraits
{
    public class ItemChargeDataTraitTest: ItemComponentTraitTestBase<ItemReference, ItemCharge, ItemChargeTrait<ItemReference>>
    {
        public ItemChargeDataTraitTest() : base(new ItemReferenceMetaData())
        {
        }

        protected override ItemChargeTrait<ItemReference> CreateTrait()
        {
            return new ItemChargeTrait<ItemReference>(1, 100);
        }

        protected override IItemComponentTestDataFactory<ItemCharge> ProduceTestData(EntityRelations<ItemReference> relations)
        {
            return new ItemComponentTestDataFactory<ItemCharge>(new ItemCharge(1, 100), 
                                                   new ItemCharge(10, 100), 
                                                   new ItemCharge(100, 100))
                .WithInvalidResult(new ItemCharge(250,250))
                .WithRemovedResult(new ItemCharge(0, 100));
        }
    }
}