using FluentAssertions;
using NUnit.Framework;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.Equipment;
using RogueEntity.Core.Meta.Base;
using RogueEntity.Core.Meta.EntityKeys;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Meta.ItemTraits;
using RogueEntity.Core.Positioning.Grid;
using System;

namespace RogueEntity.Core.Tests.Equipment
{
    public partial class SlottedEquipmentTest
    {
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

        public override IItemResolver<ItemReference> ItemResolver => Context.ItemResolver;
        public override IGridMapContext<ItemReference> ItemMapContext => throw new InvalidOperationException();

        [SetUp]
        public void SetUp()
        {
            Context = new EquipmentTestContext();
            Context.ActorEntities.Register<DestroyedMarker>();
            Context.ActorEntities.Register<CascadingDestroyedMarker>();
            Context.ActorEntities.RegisterNonConstructable<ItemDeclarationHolder<ActorReference>>();
            Context.ActorEntities.RegisterNonConstructable<SlottedEquipmentData<ItemReference>>();

            Context.ItemEntities.Register<DestroyedMarker>();
            Context.ItemEntities.Register<CascadingDestroyedMarker>();
            Context.ItemEntities.RegisterNonConstructable<ItemDeclarationHolder<ItemReference>>();
            Context.ItemEntities.RegisterNonConstructable<SlottedEquipmentData<ItemReference>>();
            Context.ItemEntities.RegisterNonConstructable<ContainerEntityMarker<ActorReference>>();
            Context.ItemEntities.RegisterNonConstructable<ContainerEntityMarker<ItemReference>>();

            Context.ActorRegistry.Register(new ReferenceItemDeclaration<ActorReference>(ActorDeclaration)
                                           .WithTrait(new WeightViewTrait<ActorReference>(Context.ActorResolver))
                                           .WithTrait(new SlottedEquipmentTrait<ActorReference, ItemReference>(
                                                          Context.ActorResolver,
                                                          Context.ItemResolver,
                                                          new ItemReferenceMetaData(),
                                                          Weight.OfKiloGram(100),
                                                          slotHead, slotLeftHand, slotRightHand
                                                      )));

            Context.ItemRegistry.Register(new ReferenceItemDeclaration<ItemReference>(ContainerDeclaration)
                                          .WithTrait(new ContainerEntityMarkerResolverTrait<ItemReference>())
                                          .WithTrait(new ContainerEntityMarkerTrait<ItemReference, ActorReference>())
                                          .WithTrait(new ContainerEntityMarkerTrait<ItemReference, ItemReference>())
                                          .WithTrait(new WeightViewTrait<ItemReference>(Context.ItemResolver))
                                          .WithTrait(new WeightTrait<ItemReference>(Weight.OfKiloGram(5))));

            Context.ItemRegistry.Register(new ReferenceItemDeclaration<ItemReference>(ContentDeclaration)
                                          .WithTrait(new ContainerEntityMarkerResolverTrait<ItemReference>())
                                          .WithTrait(new ContainerEntityMarkerTrait<ItemReference, ActorReference>())
                                          .WithTrait(new ContainerEntityMarkerTrait<ItemReference, ItemReference>())
                                          .WithTrait(new EquipmentSlotRequirementsTrait<ItemReference>(EquipmentSlotRequirements.ForRequiredSlots(slotLeftHand)))
                                          .WithTrait(new WeightViewTrait<ItemReference>(Context.ItemResolver))
                                          .WithTrait(new WeightTrait<ItemReference>(Weight.OfKiloGram(7.5f))));

            Context.ItemRegistry.Register(new ReferenceItemDeclaration<ItemReference>(NonPickupContentDeclaration)
                                          .WithTrait(new WeightViewTrait<ItemReference>(Context.ItemResolver))
                                          .WithTrait(new WeightTrait<ItemReference>(Weight.OfKiloGram(7.5f))));

            Context.ItemRegistry.Register(new ReferenceItemDeclaration<ItemReference>(HeavyContentDeclaration)
                                          .WithTrait(new ContainerEntityMarkerResolverTrait<ItemReference>())
                                          .WithTrait(new ContainerEntityMarkerTrait<ItemReference, ActorReference>())
                                          .WithTrait(new ContainerEntityMarkerTrait<ItemReference, ItemReference>())
                                          .WithTrait(new EquipmentSlotRequirementsTrait<ItemReference>(EquipmentSlotRequirements.ForRequiredSlots(slotLeftHand, slotRightHand)))
                                          .WithTrait(new WeightViewTrait<ItemReference>(Context.ItemResolver))
                                          .WithTrait(new WeightTrait<ItemReference>(Weight.OfKiloGram(97.5f))));

            Context.ItemRegistry.Register(new BulkItemDeclaration<ItemReference>(BulkContentDeclaration)
                                          .WithTrait(new EquipmentSlotRequirementsTrait<ItemReference>(EquipmentSlotRequirements.Create()
                                                                                                                                .WithAcceptableSlots(
                                                                                                                                    slotLeftHand, slotRightHand)))
                                          .WithTrait(new WeightViewTrait<ItemReference>(Context.ItemResolver))
                                          .WithTrait(new WeightTrait<ItemReference>(Weight.OfKiloGram(1f))));

            Context.ItemRegistry.Register(new BulkItemDeclaration<ItemReference>(BulkCombinedItemDeclaration)
                                          .WithTrait(new StackingBulkTrait<ItemReference>(60))
                                          .WithTrait(new EquipmentSlotRequirementsTrait<ItemReference>(EquipmentSlotRequirements.Create()
                                                                                                                                .WithRequiredSlots(slotHead)
                                                                                                                                .WithAcceptableSlots(
                                                                                                                                    slotLeftHand, slotRightHand)))
                                          .WithTrait(new WeightViewTrait<ItemReference>(Context.ItemResolver))
                                          .WithTrait(new WeightTrait<ItemReference>(Weight.OfKiloGram(1f))));

            Owner = Context.ActorResolver.Instantiate(ActorDeclaration);
            Context.ActorResolver.TryQueryData(Owner, out ISlottedEquipment<ItemReference> equipment).Should().BeTrue();
            Equipment = equipment;
        }
    }
}
