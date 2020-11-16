using NUnit.Framework;
using RogueEntity.Core.Equipment;
using RogueEntity.Core.Meta.EntityKeys;
using RogueEntity.Core.Tests.Meta.Items;

namespace RogueEntity.Core.Tests.Equipment
{
    [TestFixture]
    public class EquipmentSlotRequirementsTraitTest : ItemComponentTraitTestBase<BasicItemContext, ItemReference, EquipmentSlotRequirements, EquipmentSlotRequirementsTrait<BasicItemContext, ItemReference>>
    {
        readonly EquipmentSlotRegistry registry;
        readonly EquipmentSlot slotHead;
        readonly EquipmentSlot slotLeftHand;
        readonly EquipmentSlot slotRightHand;

        public EquipmentSlotRequirementsTraitTest(): base(new ItemReferenceMetaData())
        {
            EnableSerializationTest = false;
            
            slotHead = new EquipmentSlot("slot.head", 0, "Head", "HEAD");
            slotLeftHand = new EquipmentSlot("slot.hand.left", 1, "Left Hand", "LHND");
            slotRightHand = new EquipmentSlot("slot.hand.right", 2, "Right Hand", "RHND");

            registry = new EquipmentSlotRegistry();
            registry.Register(slotHead);
            registry.Register(slotRightHand);
            registry.Register(slotLeftHand);
        }

        protected override BasicItemContext CreateContext()
        {
            return new BasicItemContext();
        }

        protected override EquipmentSlotRequirementsTrait<BasicItemContext, ItemReference> CreateTrait()
        {
            return new EquipmentSlotRequirementsTrait<BasicItemContext, ItemReference>(EquipmentSlotRequirements.Create().WithRequiredSlots(slotHead).WithAcceptableSlots(slotLeftHand));
        }

        protected override IItemComponentTestDataFactory<EquipmentSlotRequirements> ProduceTestData(EntityRelations<ItemReference> relations)
        {
            return new ItemComponentTestDataFactory<EquipmentSlotRequirements>(
                       EquipmentSlotRequirements.Create().WithRequiredSlots(slotHead).WithAcceptableSlots(slotLeftHand),
                       EquipmentSlotRequirements.Create().WithRequiredSlots(slotHead).WithAcceptableSlots(slotLeftHand),
                       EquipmentSlotRequirements.Create().WithRequiredSlots(slotHead).WithAcceptableSlots(slotLeftHand)
                   )
                   .WithUpdateProhibited()
                   .WithRemoveProhibited()
                   .WithApplyRestoresDefaultValue();
        }
    }
}