using EnTTSharp.Entities;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Meta.ItemTraits;
using RogueEntity.Core.Tests.Meta.Items;

namespace RogueEntity.Core.Tests.Meta.ItemTraits
{
    public class ItemChargeDataTraitTest: ItemComponentTraitTestBase<BasicItemContext, ItemReference, ItemCharge, ItemChargeTrait<BasicItemContext, ItemReference>>
    {
        protected override IBulkDataStorageMetaData<ItemReference> ItemIdMetaData => new ItemReferenceMetaData();
        protected override EntityRegistry<ItemReference> EntityRegistry => Context.EntityRegistry;
        protected override ItemRegistry<BasicItemContext, ItemReference> ItemRegistry => Context.ItemRegistry;

        protected override BasicItemContext CreateContext()
        {
            return new BasicItemContext();
        }

        protected override ItemChargeTrait<BasicItemContext, ItemReference> CreateTrait()
        {
            return new ItemChargeTrait<BasicItemContext, ItemReference>(1, 100);
        }

        public override IItemComponentTestDataFactory<ItemCharge> ProduceTestData(EntityRelations<ItemReference> relations)
        {
            return new ItemComponentTestDataFactory<ItemCharge>(new ItemCharge(1, 100), 
                                                   new ItemCharge(10, 100), 
                                                   new ItemCharge(100, 100))
                .WithInvalidResult(new ItemCharge(250,250))
                .WithRemovedResult(new ItemCharge(0, 100));
        }
    }
}