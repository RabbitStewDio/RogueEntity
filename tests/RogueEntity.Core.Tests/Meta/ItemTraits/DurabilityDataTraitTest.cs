using RogueEntity.Core.Meta.ItemTraits;
using RogueEntity.Core.Tests.Meta.Items;
using RogueEntity.Core.Meta.EntityKeys;

namespace RogueEntity.Core.Tests.Meta.ItemTraits
{
    public class DurabilityDataTraitTest : ItemComponentTraitTestBase<ItemReference, Durability, DurabilityTrait<ItemReference>>
    {
        public DurabilityDataTraitTest() : base(new ItemReferenceMetaData())
        {
        }

        protected override DurabilityTrait<ItemReference> CreateTrait()
        {
            return new DurabilityTrait<ItemReference>(1, 100);
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