using NUnit.Framework;
using RogueEntity.Core.Meta.EntityKeys;
using RogueEntity.Core.Movement.CostModifier;
using RogueEntity.Core.Movement.MovementModes.Walking;
using RogueEntity.Core.Tests.Meta.Items;

namespace RogueEntity.Core.Tests.Movement.CostModifier
{
    [TestFixture]
    public class RelativeMovementCostModifierTraitTest : ItemComponentInformationTraitTestBase<ItemReference,
        RelativeMovementCostModifier<WalkingMovement>,
        RelativeMovementCostModifierTrait<ItemReference, WalkingMovement>>
    {
        public RelativeMovementCostModifierTraitTest() : base(new ItemReferenceMetaData())
        {
        }

        protected override IItemComponentTestDataFactory<RelativeMovementCostModifier<WalkingMovement>> ProduceTestData(EntityRelations<ItemReference> relations)
        {
            return new ItemComponentTestDataFactory<RelativeMovementCostModifier<WalkingMovement>>(0.50f);
        }

        protected override RelativeMovementCostModifierTrait<ItemReference, WalkingMovement> CreateTrait()
        {
            return new RelativeMovementCostModifierTrait<ItemReference, WalkingMovement>(0.50f);
        }
    }
}