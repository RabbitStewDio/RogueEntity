using EnTTSharp.Entities;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Meta.ItemTraits;

namespace RogueEntity.Core.Tests.Meta.Items
{
    public class StackingDataTraitTest : ItemComponentTraitTestBase<BasicItemContext, ItemReference, StackCount, StackingTrait<BasicItemContext, ItemReference>>
    {
        protected override EntityRegistry<ItemReference> EntityRegistry => Context.EntityRegistry;
        protected override ItemRegistry<BasicItemContext, ItemReference> ItemRegistry => Context.ItemRegistry;

        protected override BasicItemContext CreateContext()
        {
            return new BasicItemContext();
        }

        protected override StackingTrait<BasicItemContext, ItemReference> CreateTrait()
        {
            return new StackingTrait<BasicItemContext, ItemReference>(1, 100);
        }

        public override IItemComponentTestDataFactory<StackCount> ProduceTestData(EntityRelations<ItemReference> relations)
        {
            return new ItemComponentTestDataFactory<StackCount>(StackCount.Of(1, 100), 
                                                                StackCount.Of(20, 100), 
                                                                StackCount.Of(100, 100))
                .WithRemoveProhibited();
        }
    }
}