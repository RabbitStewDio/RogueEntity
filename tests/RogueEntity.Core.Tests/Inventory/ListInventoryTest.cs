using System;
using System.Collections.Generic;
using EnTTSharp.Entities.Systems;
using FluentAssertions;
using FluentAssertions.Execution;
using NUnit.Framework;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Core.Inventory;
using RogueEntity.Core.Meta.Base;
using RogueEntity.Core.Meta.EntityKeys;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Meta.ItemTraits;

namespace RogueEntity.Core.Tests.Inventory
{
    public class ListInventoryTest
    {
        static readonly ItemDeclarationId ActorDeclaration = "actor";
        static readonly ItemDeclarationId ContainerDeclaration = "container";
        static readonly ItemDeclarationId ContentDeclaration = "content";
        static readonly ItemDeclarationId NonPickupContentDeclaration = "non-pickup-content";
        static readonly ItemDeclarationId HeavyContentDeclaration = "heavy-content";
        static readonly ItemDeclarationId BulkContentDeclaration = "bulk-content";

        InventoryTestContext Context { get; set; }
        ListInventory<ActorReference, ItemReference> Inventory { get; set; }
        ActorReference Owner { get; set; }

        [SetUp]
        public void SetUp()
        {
            Context = new InventoryTestContext();
            Context.ActorEntities.Register<DestroyedMarker>();
            Context.ActorEntities.Register<CascadingDestroyedMarker>();
            Context.ActorEntities.RegisterNonConstructable<ItemDeclarationHolder<ActorReference>>();
            Context.ActorEntities.RegisterNonConstructable<ListInventoryData<ActorReference, ItemReference>>();

            Context.ItemEntities.Register<DestroyedMarker>();
            Context.ItemEntities.Register<CascadingDestroyedMarker>();
            Context.ItemEntities.RegisterNonConstructable<ItemDeclarationHolder<ItemReference>>();
            Context.ItemEntities.RegisterNonConstructable<ListInventoryData<ItemReference, ItemReference>>();
            Context.ItemEntities.RegisterNonConstructable<ContainerEntityMarker<ActorReference>>();
            Context.ItemEntities.RegisterNonConstructable<ContainerEntityMarker<ItemReference>>();

            Context.ActorRegistry.Register(new ReferenceItemDeclaration<ActorReference>(ActorDeclaration)
                                           .WithTrait(new WeightViewTrait<ActorReference>(Context.ActorResolver))
                                           .WithTrait(new ListInventoryTrait<ActorReference, ItemReference>(new ItemReferenceMetaData(), Context.ItemResolver, Weight.OfKiloGram(100))));

            Context.ItemRegistry.Register(new ReferenceItemDeclaration<ItemReference>(ContainerDeclaration)
                                          .WithTrait(new ContainerEntityMarkerResolverTrait<ItemReference>())
                                          .WithTrait(new ContainerEntityMarkerTrait<ItemReference, ActorReference>())
                                          .WithTrait(new ContainerEntityMarkerTrait<ItemReference, ItemReference>())
                                          .WithTrait(new WeightViewTrait<ItemReference>(Context.ItemResolver))
                                          .WithTrait(new WeightTrait<ItemReference>(Weight.OfKiloGram(5)))
                                          .WithTrait(new ListInventoryTrait<ItemReference, ItemReference>(new ItemReferenceMetaData(), Context.ItemResolver, Weight.OfKiloGram(40))));

            Context.ItemRegistry.Register(new ReferenceItemDeclaration<ItemReference>(ContentDeclaration)
                                          .WithTrait(new ContainerEntityMarkerResolverTrait<ItemReference>())
                                          .WithTrait(new ContainerEntityMarkerTrait<ItemReference, ActorReference>())
                                          .WithTrait(new ContainerEntityMarkerTrait<ItemReference, ItemReference>())
                                          .WithTrait(new WeightViewTrait<ItemReference>(Context.ItemResolver))
                                          .WithTrait(new WeightTrait<ItemReference>(Weight.OfKiloGram(7.5f))));

            Context.ItemRegistry.Register(new ReferenceItemDeclaration<ItemReference>(NonPickupContentDeclaration)
                                          .WithTrait(new WeightViewTrait<ItemReference>(Context.ItemResolver))
                                          .WithTrait(new WeightTrait<ItemReference>(Weight.OfKiloGram(7.5f))));

            Context.ItemRegistry.Register(new ReferenceItemDeclaration<ItemReference>(HeavyContentDeclaration)
                                          .WithTrait(new ContainerEntityMarkerResolverTrait<ItemReference>())
                                          .WithTrait(new ContainerEntityMarkerTrait<ItemReference, ActorReference>())
                                          .WithTrait(new ContainerEntityMarkerTrait<ItemReference, ItemReference>())
                                          .WithTrait(new WeightViewTrait<ItemReference>(Context.ItemResolver))
                                          .WithTrait(new WeightTrait<ItemReference>(Weight.OfKiloGram(97.5f))));

            Context.ItemRegistry.Register(new BulkItemDeclaration<ItemReference>(BulkContentDeclaration)
                                          .WithTrait(new StackingBulkTrait<ItemReference>(60))
                                          .WithTrait(new WeightViewTrait<ItemReference>(Context.ItemResolver))
                                          .WithTrait(new WeightTrait<ItemReference>(Weight.OfKiloGram(1f))));

            Owner = Context.ActorResolver.Instantiate(ActorDeclaration);
            if (!Context.ActorResolver.TryQueryData(Owner, out IInventory<ItemReference> i) ||
                !(i is ListInventory<ActorReference, ItemReference> inv))
            {
                throw new AssertionFailedException("Unable to set up test environment");
            }

            Inventory = inv;
        }

        [Test]
        public void Validate_Empty()
        {
            Inventory.Items.Should().BeEmpty();
            Inventory.TotalWeight.Should().Be(Weight.Empty);
            Inventory.AvailableCarryWeight.Should().Be(Weight.OfKiloGram(100));

            var weight = Context.ActorResolver.QueryWeight(Owner);
            weight.BaseWeight.Should().Be(Weight.Empty);
            weight.InventoryWeight.Should().Be(Weight.Empty);
        }

        [Test]
        public void Validate_AddItem_On_EmptyInventory()
        {
            var itemToAdd = Context.ItemResolver.Instantiate(ContentDeclaration);
            Inventory.TryAddItem(itemToAdd, out var remainderItem).Should().BeTrue();
            remainderItem.Should().Be(ItemReference.Empty);

            // item is contained in inventory
            Inventory.Items.Should().ContainInOrder(itemToAdd);

            // inventory total weight changes
            Inventory.TotalWeight.Should().Be(Weight.OfKiloGram(7.5f));
            Inventory.AvailableCarryWeight.Should().Be(Weight.OfKiloGram(92.5f));

            Context.ActorResolver.TryUpdateData(Owner, (IInventory<ItemReference>)Inventory, out _).Should().BeTrue();

            // actor total weight changes
            var weight = Context.ActorResolver.QueryWeight(Owner);
            weight.BaseWeight.Should().Be(Weight.Empty);
            weight.InventoryWeight.Should().Be(Weight.OfKiloGram(7.5f));
        }

        [Test]
        public void Validate_RemoveItem_On_EmptyInventory()
        {
            // This should not fail with exceptions. 

            var itemToAdd = Context.ItemResolver.Instantiate(ContentDeclaration);
            Inventory.TryRemoveItemStack(itemToAdd, 0).Should().BeFalse();
            Inventory.TryRemoveItem(ContentDeclaration, out _).Should().BeFalse();
        }

        [Test]
        public void Validate_AddItem_Non_Pickup()
        {
            var itemToAdd = Context.ItemResolver.Instantiate(NonPickupContentDeclaration);
            Inventory.TryAddItem(itemToAdd, out _).Should().BeFalse();
        }

        [Test]
        public void Validate_AddItem_ExceedWeight()
        {
            var heavyItemToAdd = Context.ItemResolver.Instantiate(HeavyContentDeclaration);
            var itemToAdd = Context.ItemResolver.Instantiate(ContentDeclaration);
            Inventory.TryAddItem(heavyItemToAdd, out _).Should().BeTrue();
            Inventory.TotalWeight.Should().Be(Weight.OfKiloGram(97.5f));
            Inventory.AvailableCarryWeight.Should().Be(Weight.OfKiloGram(2.5f));

            Inventory.TryAddItem(itemToAdd, out _).Should().BeFalse();
            Inventory.TotalWeight.Should().Be(Weight.OfKiloGram(97.5f));
            Inventory.AvailableCarryWeight.Should().Be(Weight.OfKiloGram(2.5f));
        }

        [Test]
        public void Validate_AddStackingItem()
        {
            var stackingItem = Context.ItemResolver.Instantiate(BulkContentDeclaration).WithStackData(50);
            Inventory.TryAddItem(stackingItem, out var stackedRemainder).Should().BeTrue();
            stackedRemainder.Should().Be(ItemReference.Empty);
            Inventory.TotalWeight.Should().Be(Weight.OfKiloGram(50f));
            Inventory.AvailableCarryWeight.Should().Be(Weight.OfKiloGram(50f));

            Inventory.TryAddItem(stackingItem.WithStackData(60), out var stackedRemainder2).Should().BeTrue();
            stackedRemainder2.Should().Be(stackingItem.WithStackData(10));
            Inventory.TotalWeight.Should().Be(Weight.OfKiloGram(100f));
            Inventory.AvailableCarryWeight.Should().Be(Weight.OfKiloGram(0f));

            Inventory.Items.Should().ContainInOrder(stackingItem.WithStackData(60), stackingItem.WithStackData(40));
        }

        [Test]
        public void Validate_AddStack_To_Existing()
        {
            // Bulk item weighs 1 kg, adding 50 kgs. 
            var stackingItem = Context.ItemResolver.Instantiate(BulkContentDeclaration).WithStackData(50);
            Inventory.TryAddItem(stackingItem, out _).Should().BeTrue();

            // Content item weighs 7.5 kg
            var contentItem = Context.ItemResolver.Instantiate(ContentDeclaration);
            Inventory.TryAddItem(contentItem, out _).Should().BeTrue();

            // Total inventory capacity is 100kg.
            Inventory.TotalWeight.Should().Be(Weight.OfKiloGram(57.5f));
            Inventory.AvailableCarryWeight.Should().Be(Weight.OfKiloGram(100 - 57.5f));
            // leaves 42.5 kgs of available weight. That is 42 bulk items, leaving 0.5kg unused.
            
            Inventory.TryAddItem(stackingItem, out var remainder).Should().BeTrue();
            remainder.Should().Be(stackingItem.WithStackData(8));

            Inventory.TotalWeight.Should().Be(Weight.OfKiloGram(99.5f));
            Inventory.AvailableCarryWeight.Should().Be(Weight.OfKiloGram(0.5f));
        }

        [Test]
        public void Validate_RemoveItem()
        {
            var stackingItem = Context.ItemResolver.Instantiate(BulkContentDeclaration).WithStackData(50);
            var contentItem = Context.ItemResolver.Instantiate(ContentDeclaration);
            Inventory.TryAddItem(stackingItem, out _).Should().BeTrue();
            Inventory.TryAddItem(contentItem, out _).Should().BeTrue();

            Inventory.TryRemoveItem(ContentDeclaration, out var itemRemoved).Should().BeTrue();
            itemRemoved.Should().Be(contentItem);

            Inventory.TryRemoveItem(BulkContentDeclaration, out var itemRemovedBulk).Should().BeTrue();
            itemRemovedBulk.Should().Be(stackingItem.WithStackData(1));

            Inventory.Items.Should().ContainInOrder(stackingItem.WithStackData(49));
        }

        [Test]
        public void Validate_RemoveItem_PartialStack()
        {
            var stackingItem = Context.ItemResolver.Instantiate(BulkContentDeclaration).WithStackData(60);
            var contentItem = Context.ItemResolver.Instantiate(ContentDeclaration);
            Inventory.TryAddItem(stackingItem, out _).Should().BeTrue();
            Inventory.TryAddItem(contentItem, out _).Should().BeTrue();
            Inventory.TryAddItem(stackingItem.WithStackData(15), out _).Should().BeTrue();

            Inventory.TryRemoveItemsInBulk(BulkContentDeclaration, 5).Should().ContainInOrder(stackingItem.WithStackData(5));
            Inventory.Items.Should().ContainInOrder(stackingItem.WithStackData(60), contentItem, stackingItem.WithStackData(10));
            Inventory.TryRemoveItemsInBulk(BulkContentDeclaration, 25).Should().ContainInOrder(stackingItem.WithStackData(10), stackingItem.WithStackData(15));
            Inventory.Items.Should().ContainInOrder(stackingItem.WithStackData(45), contentItem);
            Inventory.TryRemoveItemsInBulk(ContentDeclaration, 25).Should().ContainInOrder(contentItem);
            Inventory.Items.Should().ContainInOrder(stackingItem.WithStackData(45));
            Inventory.TryRemoveItemsInBulk(ContentDeclaration, 125).Should().BeEmpty();
            Inventory.TryRemoveItemsInBulk(BulkContentDeclaration, 125).Should().ContainInOrder(stackingItem.WithStackData(45));
            Inventory.Items.Should().BeEmpty();
        }

        [Test]
        public void Validate_Unable_To_Add_If_Already_In_Other_Container()
        {
            var chest = Context.ItemResolver.Instantiate(ContainerDeclaration);
            Context.ItemResolver.TryQueryData(chest, out IInventory<ItemReference> chestInventory).Should().BeTrue();

            var contentItem = Context.ItemResolver.Instantiate(ContentDeclaration);
            chestInventory.TryAddItem(contentItem, out _).Should().BeTrue();

            Inventory.TryAddItem(contentItem, out _).Should().BeFalse();
            Inventory.TryAddItem(chest, out _).Should().BeTrue();
        }

        [Test]
        public void Validate_DestroyCascade()
        {
            var chest = Context.ItemResolver.Instantiate(ContainerDeclaration);
            var contentItem = Context.ItemResolver.Instantiate(ContentDeclaration);

            Context.ItemResolver.TryQueryData(chest, out IInventory<ItemReference> chestInventory).Should().BeTrue();
            chestInventory.TryAddItem(contentItem, out _).Should().BeTrue();
            Context.ItemResolver.TryUpdateData(chest, chestInventory, out _).Should().BeTrue();

            Inventory.TryAddItem(chest, out _).Should().BeTrue();
            Context.ActorResolver.TryUpdateData(Owner, (IInventory<ItemReference>)Inventory, out _).Should().BeTrue();


            Context.ActorResolver.Destroy(Owner);

            var actions = CreateActionSystem();
            RunActionSystem(actions);
            Context.ActorEntities.IsValid(Owner).Should().BeFalse();
            Context.ItemEntities.IsValid(chest).Should().BeTrue();
            Context.ItemEntities.IsValid(contentItem).Should().BeTrue();

            RunActionSystem(actions);
            Context.ActorEntities.IsValid(Owner).Should().BeFalse();
            Context.ItemEntities.IsValid(chest).Should().BeFalse();
            Context.ItemEntities.IsValid(contentItem).Should().BeTrue();

            RunActionSystem(actions);
            Context.ActorEntities.IsValid(Owner).Should().BeFalse();
            Context.ItemEntities.IsValid(chest).Should().BeFalse();
            Context.ItemEntities.IsValid(contentItem).Should().BeFalse();
        }

        void RunActionSystem(List<Action> actions)
        {
            Console.WriteLine("Running actions");
            foreach (var a in actions)
            {
                a();
            }
        }


        List<Action> CreateActionSystem()
        {
            var dsa = new DestroyContainerContentsSystem<ActorReference, ItemReference>(Context.ItemResolver);
            var dsi = new DestroyContainerContentsSystem<ItemReference, ItemReference>(Context.ItemResolver);


            return new List<Action>
            {
                // Any inventory item that had been marked for delayed destruction is now marked as destroyable.
                Context.ActorEntities.BuildSystem()
                       .WithoutContext()
                       .WithInputParameter<CascadingDestroyedMarker>()
                       .CreateSystem(DestroyedEntitiesSystem<ActorReference>.SchedulePreviouslyMarkedItemsForDestruction),
                Context.ItemEntities.BuildSystem()
                       .WithoutContext()
                       .WithInputParameter<CascadingDestroyedMarker>()
                       .CreateSystem(DestroyedEntitiesSystem<ItemReference>.SchedulePreviouslyMarkedItemsForDestruction),


                // any destroyed item that is a container must mark its remaining container contents as scheduled for destruction at the 
                // next turn.
                Context.ActorEntities.BuildSystem()
                       .WithoutContext()
                       .WithInputParameter<DestroyedMarker, ListInventoryData<ActorReference, ItemReference>>()
                       .CreateSystem(dsa.MarkDestroyedContainerEntities),
                Context.ItemEntities.BuildSystem()
                       .WithoutContext()
                       .WithInputParameter<DestroyedMarker, ListInventoryData<ItemReference, ItemReference>>()
                       .CreateSystem(dsi.MarkDestroyedContainerEntities),

                // Finally clean up any item marked as destroyed.
                new DestroyedEntitiesSystem<ItemReference>(Context.ItemEntities).DeleteMarkedEntities,
                new DestroyedEntitiesSystem<ActorReference>(Context.ActorEntities).DeleteMarkedEntities,
            };
        }
    }

    static class TestItemStackExtensions
    {
        public static ItemReference WithStackData(this ItemReference r, int stackCount)
        {
            return r.WithData(stackCount - 1);
        }
    }
}
