using FluentAssertions;
using NUnit.Framework;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Equipment;
using RogueEntity.Core.Meta.Base;
using RogueEntity.Core.Meta.EntityKeys;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Meta.ItemTraits;
using RogueEntity.Core.Tests.Fixtures;

namespace RogueEntity.Core.Tests.Equipment
{
    [TestFixture]
    public partial class SlottedEquipmentTest: WhenFixtureSupport, IItemFixture
    {
        static readonly ItemDeclarationId ActorDeclaration = "actor";
        static readonly ItemDeclarationId ContainerDeclaration = "container";
        static readonly ItemDeclarationId ContentDeclaration = "content";
        static readonly ItemDeclarationId NonPickupContentDeclaration = "non-pickup-content";
        static readonly ItemDeclarationId HeavyContentDeclaration = "heavy-content";
        static readonly ItemDeclarationId BulkContentDeclaration = "bulk-content";
        static readonly ItemDeclarationId BulkCombinedItemDeclaration = "bulk-combined-item";

        readonly EquipmentSlotRegistry registry;
        readonly EquipmentSlot slotHead;
        readonly EquipmentSlot slotLeftHand;
        readonly EquipmentSlot slotRightHand;

        internal ISlottedEquipment<ItemReference> Equipment;
        EquipmentTestContext Context { get; set; }
        ActorReference Owner { get; set; }

        [Test]
        public void Validate_Equip_StackedBulkItem()
        {
            // equipping within weight limits is ok

            // equipping beyond weight limits does not partially equip items
            // todo: Maybe change that? Equipping a bundle of arrows would not be easy otherwise. 
        }

        [Test]
        public void Validate_DefaultState()
        {
            Equipment.TotalWeight.Should().Be(Weight.OfKiloGram(0f));
            Equipment.MaximumCarryWeight.Should().Be(Weight.OfKiloGram(100f));
            Equipment.AvailableSlots.Should().ContainInOrder(slotHead, slotLeftHand, slotRightHand);
            Equipment.QueryItems().Should().BeEmpty();
        }

        [Test]
        public void Validate_Equip_ReferenceItem()
        {
            var item = Context.ItemResolver.Instantiate(HeavyContentDeclaration);
            Equipment.TryEquipItem(item, out var modItem, Optional.Empty(), out var actualSlot).Should().BeTrue();
            actualSlot.Should().Be(slotLeftHand);
            modItem.Should().Be(ItemReference.Empty);
            Equipment.QueryItems().Should().ContainInOrder(new EquippedItem<ItemReference>(item, actualSlot));
            Equipment.TotalWeight.Should().Be(Weight.OfKiloGram(97.5f));
            Equipment.MaximumCarryWeight.Should().Be(Weight.OfKiloGram(100f));
        }


        [Test]
        public void Validate_Equip_ReferenceItem_Fails_If_ContainedElsewhere()
        {
            var item = Context.ItemResolver.Instantiate(HeavyContentDeclaration);
            Equipment.TryEquipItem(item, out _, Optional.Empty(), out _).Should().BeTrue();

            // Attempting to add an item that is heavier than the total allowed weight should fail.

            var actor2 = Context.ActorResolver.Instantiate(ActorDeclaration);
            Context.ActorResolver.TryQueryData(actor2, out ISlottedEquipment<ItemReference> _).Should().BeTrue();
            Equipment.TryEquipItem(item, out var modItemOther, Optional.Empty(), out var actualSlotOther).Should().BeFalse();
            actualSlotOther.Should().Be(default(EquipmentSlot));
            modItemOther.Should().Be(ItemReference.Empty);
        }

        [Test]
        public void Validate_Equip_ReferenceItem_Fails_If_ContainedElsewhere2()
        {
            var item = Context.ItemResolver.Instantiate(HeavyContentDeclaration);
            Context.ItemResolver.TryUpdateData(item, new ContainerEntityMarker<ActorReference>(), out _).Should().BeTrue();

            Equipment.TryEquipItem(item, out var modItem, Optional.Empty(), out var actualSlot).Should().BeFalse();
            actualSlot.Should().Be(default(EquipmentSlot));
            modItem.Should().Be(ItemReference.Empty);
        }

        [Test]
        public void Validate_Equip_AlternateSlot()
        {
            var item = Context.ItemResolver.Instantiate(ContentDeclaration);
            Equipment.TryEquipItem(item, out _, Optional.Empty(), out var actualSlot).Should().BeTrue();
            actualSlot.Should().Be(slotLeftHand);

            // Requesting to be placed on an already occupied spot should fail.
            var bulkItem = Context.ItemResolver.Instantiate(BulkContentDeclaration);
            Equipment.TryEquipItem(bulkItem, out _, Optional.ValueOf(slotLeftHand), out _).Should().BeFalse();

            Equipment.TryEquipItem(bulkItem, out var modItemBulk, Optional.Empty(), out var actualSlotBulk).Should().BeTrue();
            modItemBulk.Should().Be(ItemReference.Empty);
            actualSlotBulk.Should().Be(slotRightHand);
        }

        [Test]
        public void Validate_Equip_MutuallyExclusiveBulkItems()
        {
            var item = Context.ItemResolver.Instantiate(BulkCombinedItemDeclaration);
            Equipment.TryEquipItem(item, out _, Optional.Empty(), out var actualSlot).Should().BeTrue();
            actualSlot.Should().Be(slotHead);

            // Requesting to be placed on an already occupied spot should fail.
            var bulkItem = Context.ItemResolver.Instantiate(BulkCombinedItemDeclaration);
            Equipment.TryEquipItem(bulkItem, out _, Optional.ValueOf(slotLeftHand), out _).Should().BeFalse();

            // Requesting to be placed on an empty slot when the item shares a common required slot (slotHead!) should fail too.
            Equipment.TryEquipItem(bulkItem, out _, Optional.ValueOf(slotRightHand), out _).Should().BeFalse();
        }

        
        
        [Test]
        public void Validate_Equip_StackingBulkItems_DesiredSlot()
        {
            this.GivenAnEntity(BulkCombinedItemDeclaration).WithStackSize(15).InstantiatedAsEquipment(slotHead);
            var addedItem = this.GivenAnEntity(BulkCombinedItemDeclaration).WithStackSize(15).InstantiatedWithoutPosition();
            var expectedItem = this.GivenAnEntity(BulkCombinedItemDeclaration).WithStackSize(30).InstantiatedWithoutPosition();
            ItemReference remainderItem = ItemReference.Empty;
            
            // Requesting to be placed on an already occupied spot should fail.
            When(() => Equipment.TryEquipItem(addedItem, out remainderItem, Optional.ValueOf(slotLeftHand), out _));
            
            Then_Operation_Should_Succeed();
            Equipment.QueryItems().Should().ContainInOrder(new EquippedItem<ItemReference>(expectedItem, slotHead));
            remainderItem.Should().Be(ItemReference.Empty);
        }

        [Test]
        public void Validate_Equip_StackingBulkItems_AutoSlot_Exceed_StackSize()
        {
            this.GivenAnEntity(BulkCombinedItemDeclaration).WithStackSize(10).InstantiatedAsEquipment(slotHead);
            var addedItem = this.GivenAnEntity(BulkCombinedItemDeclaration).WithStackSize(55).InstantiatedWithoutPosition();
            
            var expectedResultItem = this.GivenAnEntity(BulkCombinedItemDeclaration).WithStackSize(60).InstantiatedWithoutPosition();
            var expectedRemainderItem = this.GivenAnEntity(BulkCombinedItemDeclaration).WithStackSize(5).InstantiatedWithoutPosition();
            
            ItemReference remainderItem = ItemReference.Empty;
            When(_ => Equipment.TryEquipItem(addedItem, out remainderItem, Optional.Empty(), out var _));
            
            Then_Operation_Should_Succeed();
            
            Equipment.QueryItems().Should().ContainInOrder(new EquippedItem<ItemReference>(expectedResultItem, slotHead));
            remainderItem.Should().Be(expectedRemainderItem);
        }

        [Test]
        public void Validate_UnequipItem()
        {
            var item = Context.ItemResolver.Instantiate(ContentDeclaration);
            Equipment.TryEquipItem(item, out _, Optional.Empty(), out var actualSlot).Should().BeTrue();
            actualSlot.Should().Be(slotLeftHand);

            Equipment.TryUnequipItem(item, out var slot).Should().BeTrue();
            slot.Should().Be(slotLeftHand);
            Equipment.QueryItems().Should().BeEmpty();
        }

        [Test]
        public void Validate_Unequip_StackingBulkItem()
        {
            var item = Context.ItemResolver.Instantiate(BulkCombinedItemDeclaration).WithData(10);
            Equipment.TryEquipItem(item, out _, Optional.Empty(), out var actualSlot).Should().BeTrue();
            actualSlot.Should().Be(slotHead);

            Equipment.TryUnequipItem(item, out var slot).Should().BeTrue();
            slot.Should().Be(slotHead);
            Equipment.QueryItems().Should().BeEmpty();
        }

        [Test]
        public void Validate_Unequip_MultipleItems()
        {
            var item = Context.ItemResolver.Instantiate(BulkContentDeclaration);
            Equipment.TryEquipItem(item, out _, Optional.Empty(), out var actualSlot1).Should().BeTrue();
            Equipment.TryEquipItem(item, out _, Optional.Empty(), out var actualSlot2).Should().BeTrue();
            actualSlot1.Should().Be(slotLeftHand);
            actualSlot2.Should().Be(slotRightHand);

            Equipment.QueryItems().Should().ContainInOrder(new EquippedItem<ItemReference>(item, slotLeftHand), new EquippedItem<ItemReference>(item, slotRightHand));

            Equipment.TryUnequipItem(item, out var slot).Should().BeTrue();
            slot.Should().Be(slotLeftHand);
            Equipment.QueryItems().Should().Contain(new EquippedItem<ItemReference>(item, slotRightHand));
            Equipment.TryUnequipItem(item, out var slot2).Should().BeTrue();
            slot2.Should().Be(slotRightHand);
            Equipment.QueryItems().Should().BeEmpty();
        }
    }
}