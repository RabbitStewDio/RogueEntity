using System;
using FluentAssertions;
using NUnit.Framework;
using RogueEntity.Core.Equipment;
using RogueEntity.Core.Meta.Base;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Meta.ItemTraits;
using RogueEntity.Core.Utils;
using Serilog;

namespace RogueEntity.Core.Tests.Equipment
{
    [TestFixture]
    public class SlottedEquipmentTest
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

        ISlottedEquipment<EquipmentTestContext, ItemReference> Equipment;
        EquipmentTestContext Context { get; set; }
        ActorReference Owner { get; set; }

        public SlottedEquipmentTest()
        {
            slotHead = new EquipmentSlot("slot.head", 0, "Head", "HEAD");
            slotLeftHand = new EquipmentSlot("slot.hand.left", 1, "Left Hand", "LHND");
            slotRightHand = new EquipmentSlot("slot.hand.right", 2, "Right Hand", "RHND");

            registry = new EquipmentSlotRegistry();
            registry.Register(slotHead);
            registry.Register(slotRightHand);
            registry.Register(slotLeftHand);
        }

        [SetUp]
        public void SetUp()
        {
            Context = new EquipmentTestContext();
            Context.ActorEntities.Register<DestroyedMarker>();
            Context.ActorEntities.Register<CascadingDestroyedMarker>();
            Context.ActorEntities.RegisterNonConstructable<ItemDeclarationHolder<EquipmentTestContext, ActorReference>>();
            Context.ActorEntities.RegisterNonConstructable<SlottedEquipmentData<ItemReference>>();

            Context.ItemEntities.Register<DestroyedMarker>();
            Context.ItemEntities.Register<CascadingDestroyedMarker>();
            Context.ItemEntities.RegisterNonConstructable<ItemDeclarationHolder<EquipmentTestContext, ItemReference>>();
            Context.ItemEntities.RegisterNonConstructable<SlottedEquipmentData<ItemReference>>();
            Context.ItemEntities.RegisterNonConstructable<ContainerEntityMarker<ActorReference>>();
            Context.ItemEntities.RegisterNonConstructable<ContainerEntityMarker<ItemReference>>();

            Context.ActorRegistry.Register(new ReferenceItemDeclaration<EquipmentTestContext, ActorReference>(ActorDeclaration)
                                           .WithTrait(new WeightViewTrait<EquipmentTestContext, ActorReference>(Context.ActorResolver))
                                           .WithTrait(new SlottedEquipmentTrait<EquipmentTestContext, ActorReference, ItemReference>(Context.ItemResolver,
                                                              Weight.OfKiloGram(100),
                                                              slotHead, slotLeftHand, slotRightHand
                                                      )));

            Context.ItemRegistry.Register(new ReferenceItemDeclaration<EquipmentTestContext, ItemReference>(ContainerDeclaration)
                                          .WithTrait(new ContainerEntityMarkerResolverTrait<EquipmentTestContext, ItemReference>())
                                          .WithTrait(new ContainerEntityMarkerTrait<EquipmentTestContext, ItemReference, ActorReference>())
                                          .WithTrait(new ContainerEntityMarkerTrait<EquipmentTestContext, ItemReference, ItemReference>())
                                          .WithTrait(new WeightViewTrait<EquipmentTestContext, ItemReference>(Context.ItemResolver))
                                          .WithTrait(new WeightTrait<EquipmentTestContext, ItemReference>(Weight.OfKiloGram(5))));

            Context.ItemRegistry.Register(new ReferenceItemDeclaration<EquipmentTestContext, ItemReference>(ContentDeclaration)
                                          .WithTrait(new ContainerEntityMarkerResolverTrait<EquipmentTestContext, ItemReference>())
                                          .WithTrait(new ContainerEntityMarkerTrait<EquipmentTestContext, ItemReference, ActorReference>())
                                          .WithTrait(new ContainerEntityMarkerTrait<EquipmentTestContext, ItemReference, ItemReference>())
                                          .WithTrait(new EquipmentSlotRequirementsTrait<EquipmentTestContext, ItemReference>(EquipmentSlotRequirements.ForRequiredSlots(slotLeftHand)))
                                          .WithTrait(new WeightViewTrait<EquipmentTestContext, ItemReference>(Context.ItemResolver))
                                          .WithTrait(new WeightTrait<EquipmentTestContext, ItemReference>(Weight.OfKiloGram(7.5f))));

            Context.ItemRegistry.Register(new ReferenceItemDeclaration<EquipmentTestContext, ItemReference>(NonPickupContentDeclaration)
                                          .WithTrait(new WeightViewTrait<EquipmentTestContext, ItemReference>(Context.ItemResolver))
                                          .WithTrait(new WeightTrait<EquipmentTestContext, ItemReference>(Weight.OfKiloGram(7.5f))));

            Context.ItemRegistry.Register(new ReferenceItemDeclaration<EquipmentTestContext, ItemReference>(HeavyContentDeclaration)
                                          .WithTrait(new ContainerEntityMarkerResolverTrait<EquipmentTestContext, ItemReference>())
                                          .WithTrait(new ContainerEntityMarkerTrait<EquipmentTestContext, ItemReference, ActorReference>())
                                          .WithTrait(new ContainerEntityMarkerTrait<EquipmentTestContext, ItemReference, ItemReference>())
                                          .WithTrait(new EquipmentSlotRequirementsTrait<EquipmentTestContext, ItemReference>(EquipmentSlotRequirements.ForRequiredSlots(slotLeftHand, slotRightHand)))
                                          .WithTrait(new WeightViewTrait<EquipmentTestContext, ItemReference>(Context.ItemResolver))
                                          .WithTrait(new WeightTrait<EquipmentTestContext, ItemReference>(Weight.OfKiloGram(97.5f))));

            Context.ItemRegistry.Register(new BulkItemDeclaration<EquipmentTestContext, ItemReference>(BulkContentDeclaration)
                                          .WithTrait(new StackingTrait<EquipmentTestContext, ItemReference>(60))
                                          .WithTrait(new EquipmentSlotRequirementsTrait<EquipmentTestContext, ItemReference>(EquipmentSlotRequirements.Create()
                                                                                                                                 .WithAcceptableSlots(slotLeftHand, slotRightHand)))
                                          .WithTrait(new WeightViewTrait<EquipmentTestContext, ItemReference>(Context.ItemResolver))
                                          .WithTrait(new WeightTrait<EquipmentTestContext, ItemReference>(Weight.OfKiloGram(1f))));

            Context.ItemRegistry.Register(new BulkItemDeclaration<EquipmentTestContext, ItemReference>(BulkCombinedItemDeclaration)
                                          .WithTrait(new StackingTrait<EquipmentTestContext, ItemReference>(60))
                                          .WithTrait(new EquipmentSlotRequirementsTrait<EquipmentTestContext, ItemReference>(EquipmentSlotRequirements.Create()
                                                                                                                                 .WithRequiredSlots(slotHead)
                                                                                                                                 .WithAcceptableSlots(slotLeftHand, slotRightHand)))
                                          .WithTrait(new WeightViewTrait<EquipmentTestContext, ItemReference>(Context.ItemResolver))
                                          .WithTrait(new WeightTrait<EquipmentTestContext, ItemReference>(Weight.OfKiloGram(1f))));

            Owner = Context.ActorResolver.Instantiate(Context, ActorDeclaration);
            Context.ActorResolver.TryQueryData(Owner, Context, out ISlottedEquipment<EquipmentTestContext, ItemReference> equipment).Should().BeTrue();
            Equipment = equipment;
        }

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
            var item = Context.ItemResolver.Instantiate(Context, HeavyContentDeclaration);
            Equipment.TryEquipItem(Context, item, out var modItem, Optional.Empty<EquipmentSlot>(), out var actualSlot).Should().BeTrue();
            actualSlot.Should().Be(slotLeftHand);
            modItem.Should().Be(ItemReference.Empty);
            Equipment.QueryItems().Should().ContainInOrder(new EquippedItem<ItemReference>(item, actualSlot));
            Equipment.TotalWeight.Should().Be(Weight.OfKiloGram(97.5f));
            Equipment.MaximumCarryWeight.Should().Be(Weight.OfKiloGram(100f));
        }


        [Test]
        public void Validate_Equip_ReferenceItem_Fails_If_ContainedElsewhere()
        {
            var item = Context.ItemResolver.Instantiate(Context, HeavyContentDeclaration);
            Equipment.TryEquipItem(Context, item, out var modItem, Optional.Empty<EquipmentSlot>(), out var actualSlot).Should().BeTrue();

            // Attempting to add an item that is heavier than the total allowed weight should fail.
            
            var actor2 = Context.ActorResolver.Instantiate(Context, ActorDeclaration);
            Context.ActorResolver.TryQueryData(actor2, Context, out ISlottedEquipment<EquipmentTestContext, ItemReference> otherActorEquipment).Should().BeTrue();
            Equipment.TryEquipItem(Context, item, out var modItemOther, Optional.Empty<EquipmentSlot>(), out var actualSlotOther).Should().BeFalse();
            actualSlotOther.Should().Be(default(EquipmentSlot));
            modItemOther.Should().Be(ItemReference.Empty);
        }

        [Test]
        public void Validate_Equip_ReferenceItem_Fails_If_ContainedElsewhere2()
        {
            var item = Context.ItemResolver.Instantiate(Context, HeavyContentDeclaration);
            Context.ItemResolver.TryUpdateData(item, Context, new ContainerEntityMarker<ActorReference>(), out _).Should().BeTrue();
            
            Equipment.TryEquipItem(Context, item, out var modItem, Optional.Empty<EquipmentSlot>(), out var actualSlot).Should().BeFalse();
            actualSlot.Should().Be(default(EquipmentSlot));
            modItem.Should().Be(ItemReference.Empty);
        }

        [Test]
        public void Validate_Equip_AlternateSlot()
        {
            var item = Context.ItemResolver.Instantiate(Context, ContentDeclaration);
            Equipment.TryEquipItem(Context, item, out var modItem, Optional.Empty<EquipmentSlot>(), out var actualSlot).Should().BeTrue();
            actualSlot.Should().Be(slotLeftHand);

            // Requesting to be placed on an already occupied spot should fail.
            var bulkItem = Context.ItemResolver.Instantiate(Context, BulkContentDeclaration);
            Equipment.TryEquipItem(Context, bulkItem, out _, Optional.ValueOf(slotLeftHand), out _).Should().BeFalse();

            Equipment.TryEquipItem(Context, bulkItem, out var modItemBulk, Optional.Empty<EquipmentSlot>(), out var actualSlotBulk).Should().BeTrue();
            modItemBulk.Should().Be(ItemReference.Empty);
            actualSlotBulk.Should().Be(slotRightHand);
        }

        [Test]
        public void Validate_Equip_MutuallyExclusiveBulkItems()
        {
            var item = Context.ItemResolver.Instantiate(Context, BulkCombinedItemDeclaration);
            Equipment.TryEquipItem(Context, item, out var modItem, Optional.Empty<EquipmentSlot>(), out var actualSlot).Should().BeTrue();
            actualSlot.Should().Be(slotHead);

            // Requesting to be placed on an already occupied spot should fail.
            var bulkItem = Context.ItemResolver.Instantiate(Context, BulkCombinedItemDeclaration);
            Equipment.TryEquipItem(Context, bulkItem, out _, Optional.ValueOf(slotLeftHand), out _).Should().BeFalse();
            
            // Requesting to be placed on an empty slot when the item shares a common required slot (slotHead!) should fail too.
            Equipment.TryEquipItem(Context, bulkItem, out var modItemBulk, Optional.ValueOf(slotRightHand), out var actualSlotBulk).Should().BeFalse();
        }

        [Test]
        public void Validate_Equip_StackingBulkItems_DesiredSlot()
        {
            var item = Context.ItemResolver.Instantiate(Context, BulkCombinedItemDeclaration).WithData(10);
            Equipment.TryEquipItem(Context, item, out var modItem, Optional.Empty<EquipmentSlot>(), out var actualSlot).Should().BeTrue();
            actualSlot.Should().Be(slotHead);

            // Requesting to be placed on an already occupied spot should fail.
            var bulkItem = Context.ItemResolver.Instantiate(Context, BulkCombinedItemDeclaration).WithData(15);
            Equipment.TryEquipItem(Context, bulkItem, out var remainderItem, Optional.ValueOf(slotLeftHand), out _).Should().BeTrue();
            Equipment.QueryItems().Should().ContainInOrder(new EquippedItem<ItemReference>(item.WithData(25), slotHead));
            remainderItem.Should().Be(ItemReference.Empty);
        }

        [Test]
        public void Validate_Equip_StackingBulkItems_AutoSlot()
        {
            var item = Context.ItemResolver.Instantiate(Context, BulkCombinedItemDeclaration).WithData(10);
            Equipment.TryEquipItem(Context, item, out var modItem, Optional.Empty<EquipmentSlot>(), out var actualSlot).Should().BeTrue();
            actualSlot.Should().Be(slotHead);

            // Requesting to be placed on an already occupied spot should fail.
            var bulkItem = Context.ItemResolver.Instantiate(Context, BulkCombinedItemDeclaration).WithData(55);
            Equipment.TryEquipItem(Context, bulkItem, out var remainingItem, Optional.Empty<EquipmentSlot>(), out _).Should().BeTrue();
            Equipment.QueryItems().Should().ContainInOrder(new EquippedItem<ItemReference>(item.WithData(60), slotHead));
            remainingItem.Should().Be(item.WithData(5));
        }

        [Test]
        public void Validate_Equip_Two_Handed()
        {
        }

        [Test]
        public void Validate_UnequipItem()
        {
        }

        [Test]
        public void Validate_Unequip_StackingBulkItem()
        {
        }
    }
}