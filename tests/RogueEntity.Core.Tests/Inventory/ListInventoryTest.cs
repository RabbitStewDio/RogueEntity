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
        ListInventory<InventoryTestContext, ActorReference, ItemReference> Inventory { get; set; }
        ActorReference Owner { get; set; }

        [SetUp]
        public void SetUp()
        {
            Context = new InventoryTestContext();
            Context.ActorEntities.Register<DestroyedMarker>();
            Context.ActorEntities.Register<CascadingDestroyedMarker>();
            Context.ActorEntities.RegisterNonConstructable<ItemDeclarationHolder<InventoryTestContext, ActorReference>>();
            Context.ActorEntities.RegisterNonConstructable<ListInventoryData<ActorReference, ItemReference>>();

            Context.ItemEntities.Register<DestroyedMarker>();
            Context.ItemEntities.Register<CascadingDestroyedMarker>();
            Context.ItemEntities.RegisterNonConstructable<ItemDeclarationHolder<InventoryTestContext, ItemReference>>();
            Context.ItemEntities.RegisterNonConstructable<ListInventoryData<ItemReference, ItemReference>>();
            Context.ItemEntities.RegisterNonConstructable<ContainerEntityMarker<ActorReference>>();
            Context.ItemEntities.RegisterNonConstructable<ContainerEntityMarker<ItemReference>>();

            Context.ActorRegistry.Register(new ReferenceItemDeclaration<InventoryTestContext, ActorReference>(ActorDeclaration)
                                           .WithTrait(new WeightViewTrait<InventoryTestContext, ActorReference>(Context.ActorResolver))
                                           .WithTrait(new ListInventoryTrait<InventoryTestContext, ActorReference, ItemReference>(new ItemReferenceMetaData(), Context.ItemResolver, Weight.OfKiloGram(100))));

            Context.ItemRegistry.Register(new ReferenceItemDeclaration<InventoryTestContext, ItemReference>(ContainerDeclaration)
                                          .WithTrait(new ContainerEntityMarkerResolverTrait<InventoryTestContext, ItemReference>())
                                          .WithTrait(new ContainerEntityMarkerTrait<InventoryTestContext, ItemReference, ActorReference>())
                                          .WithTrait(new ContainerEntityMarkerTrait<InventoryTestContext, ItemReference, ItemReference>())
                                          .WithTrait(new WeightViewTrait<InventoryTestContext, ItemReference>(Context.ItemResolver))
                                          .WithTrait(new WeightTrait<InventoryTestContext, ItemReference>(Weight.OfKiloGram(5)))
                                          .WithTrait(new ListInventoryTrait<InventoryTestContext, ItemReference, ItemReference>(new ItemReferenceMetaData(), Context.ItemResolver, Weight.OfKiloGram(40))));

            Context.ItemRegistry.Register(new ReferenceItemDeclaration<InventoryTestContext, ItemReference>(ContentDeclaration)
                                          .WithTrait(new ContainerEntityMarkerResolverTrait<InventoryTestContext, ItemReference>())
                                          .WithTrait(new ContainerEntityMarkerTrait<InventoryTestContext, ItemReference, ActorReference>())
                                          .WithTrait(new ContainerEntityMarkerTrait<InventoryTestContext, ItemReference, ItemReference>())
                                          .WithTrait(new WeightViewTrait<InventoryTestContext, ItemReference>(Context.ItemResolver))
                                          .WithTrait(new WeightTrait<InventoryTestContext, ItemReference>(Weight.OfKiloGram(7.5f))));

            Context.ItemRegistry.Register(new ReferenceItemDeclaration<InventoryTestContext, ItemReference>(NonPickupContentDeclaration)
                                          .WithTrait(new WeightViewTrait<InventoryTestContext, ItemReference>(Context.ItemResolver))
                                          .WithTrait(new WeightTrait<InventoryTestContext, ItemReference>(Weight.OfKiloGram(7.5f))));

            Context.ItemRegistry.Register(new ReferenceItemDeclaration<InventoryTestContext, ItemReference>(HeavyContentDeclaration)
                                          .WithTrait(new ContainerEntityMarkerResolverTrait<InventoryTestContext, ItemReference>())
                                          .WithTrait(new ContainerEntityMarkerTrait<InventoryTestContext, ItemReference, ActorReference>())
                                          .WithTrait(new ContainerEntityMarkerTrait<InventoryTestContext, ItemReference, ItemReference>())
                                          .WithTrait(new WeightViewTrait<InventoryTestContext, ItemReference>(Context.ItemResolver))
                                          .WithTrait(new WeightTrait<InventoryTestContext, ItemReference>(Weight.OfKiloGram(97.5f))));

            Context.ItemRegistry.Register(new BulkItemDeclaration<InventoryTestContext, ItemReference>(BulkContentDeclaration)
                                          .WithTrait(new StackingBulkTrait<InventoryTestContext, ItemReference>(60))
                                          .WithTrait(new WeightViewTrait<InventoryTestContext, ItemReference>(Context.ItemResolver))
                                          .WithTrait(new WeightTrait<InventoryTestContext, ItemReference>(Weight.OfKiloGram(1f))));

            Owner = Context.ActorResolver.Instantiate(Context, ActorDeclaration);
            if (!Context.ActorResolver.TryQueryData(Owner, Context, out IInventory<InventoryTestContext, ItemReference> i) ||
                !(i is ListInventory<InventoryTestContext, ActorReference, ItemReference> inv))
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

            var weight = Context.ActorResolver.QueryWeight(Owner, Context);
            weight.BaseWeight.Should().Be(Weight.Empty);
            weight.InventoryWeight.Should().Be(Weight.Empty);
        }

        [Test]
        public void Validate_AddItem_On_EmptyInventory()
        {
            var itemToAdd = Context.ItemResolver.Instantiate(Context, ContentDeclaration);
            Inventory.TryAddItem(Context, itemToAdd, out var remainderItem).Should().BeTrue();
            remainderItem.Should().Be(ItemReference.Empty);

            // item is contained in inventory
            Inventory.Items.Should().ContainInOrder(itemToAdd);

            // inventory total weight changes
            Inventory.TotalWeight.Should().Be(Weight.OfKiloGram(7.5f));
            Inventory.AvailableCarryWeight.Should().Be(Weight.OfKiloGram(92.5f));

            Context.ActorResolver.TryUpdateData(Owner, Context, (IInventory<InventoryTestContext, ItemReference>)Inventory, out _).Should().BeTrue();

            // actor total weight changes
            var weight = Context.ActorResolver.QueryWeight(Owner, Context);
            weight.BaseWeight.Should().Be(Weight.Empty);
            weight.InventoryWeight.Should().Be(Weight.OfKiloGram(7.5f));
        }

        [Test]
        public void Validate_RemoveItem_On_EmptyInventory()
        {
            // This should not fail with exceptions. 

            var itemToAdd = Context.ItemResolver.Instantiate(Context, ContentDeclaration);
            Inventory.TryRemoveItemStack(Context, itemToAdd, 0).Should().BeFalse();
            Inventory.TryRemoveItem(Context, ContentDeclaration, out _).Should().BeFalse();
        }

        [Test]
        public void Validate_AddItem_Non_Pickup()
        {
            var itemToAdd = Context.ItemResolver.Instantiate(Context, NonPickupContentDeclaration);
            Inventory.TryAddItem(Context, itemToAdd, out _).Should().BeFalse();
        }

        [Test]
        public void Validate_AddItem_ExceedWeight()
        {
            var heavyItemToAdd = Context.ItemResolver.Instantiate(Context, HeavyContentDeclaration);
            var itemToAdd = Context.ItemResolver.Instantiate(Context, ContentDeclaration);
            Inventory.TryAddItem(Context, heavyItemToAdd, out _).Should().BeTrue();
            Inventory.TotalWeight.Should().Be(Weight.OfKiloGram(97.5f));
            Inventory.AvailableCarryWeight.Should().Be(Weight.OfKiloGram(2.5f));

            Inventory.TryAddItem(Context, itemToAdd, out _).Should().BeFalse();
            Inventory.TotalWeight.Should().Be(Weight.OfKiloGram(97.5f));
            Inventory.AvailableCarryWeight.Should().Be(Weight.OfKiloGram(2.5f));
        }

        [Test]
        public void Validate_AddStackingItem()
        {
            var stackingItem = Context.ItemResolver.Instantiate(Context, BulkContentDeclaration).WithData(50);
            Inventory.TryAddItem(Context, stackingItem, out var stackedRemainder).Should().BeTrue();
            stackedRemainder.Should().Be(ItemReference.Empty);
            Inventory.TotalWeight.Should().Be(Weight.OfKiloGram(50f));
            Inventory.AvailableCarryWeight.Should().Be(Weight.OfKiloGram(50f));

            Inventory.TryAddItem(Context, stackingItem.WithData(60), out var stackedRemainder2).Should().BeTrue();
            stackedRemainder2.Should().Be(stackingItem.WithData(10));
            Inventory.TotalWeight.Should().Be(Weight.OfKiloGram(100f));
            Inventory.AvailableCarryWeight.Should().Be(Weight.OfKiloGram(0f));

            Inventory.Items.Should().ContainInOrder(stackingItem.WithData(60), stackingItem.WithData(40));
        }

        [Test]
        public void Validate_AddStack_To_Existing()
        {
            var stackingItem = Context.ItemResolver.Instantiate(Context, BulkContentDeclaration).WithData(50);
            Inventory.TryAddItem(Context, stackingItem, out _).Should().BeTrue();

            var contentItem = Context.ItemResolver.Instantiate(Context, ContentDeclaration);
            Inventory.TryAddItem(Context, contentItem, out _).Should().BeTrue();

            Inventory.TryAddItem(Context, stackingItem, out var remainder).Should().BeTrue();
            remainder.Should().Be(stackingItem.WithData(8));

            Inventory.TotalWeight.Should().Be(Weight.OfKiloGram(99.5f));
            Inventory.AvailableCarryWeight.Should().Be(Weight.OfKiloGram(0.5f));
        }

        [Test]
        public void Validate_RemoveItem()
        {
            var stackingItem = Context.ItemResolver.Instantiate(Context, BulkContentDeclaration).WithData(50);
            var contentItem = Context.ItemResolver.Instantiate(Context, ContentDeclaration);
            Inventory.TryAddItem(Context, stackingItem, out _).Should().BeTrue();
            Inventory.TryAddItem(Context, contentItem, out _).Should().BeTrue();

            Inventory.TryRemoveItem(Context, ContentDeclaration, out var itemRemoved).Should().BeTrue();
            itemRemoved.Should().Be(contentItem);

            Inventory.TryRemoveItem(Context, BulkContentDeclaration, out var itemRemovedBulk).Should().BeTrue();
            itemRemovedBulk.Should().Be(stackingItem.WithData(1));

            Inventory.Items.Should().ContainInOrder(stackingItem.WithData(49));
        }

        [Test]
        public void Validate_RemoveItem_PartialStack()
        {
            var stackingItem = Context.ItemResolver.Instantiate(Context, BulkContentDeclaration).WithData(60);
            var contentItem = Context.ItemResolver.Instantiate(Context, ContentDeclaration);
            Inventory.TryAddItem(Context, stackingItem, out _).Should().BeTrue();
            Inventory.TryAddItem(Context, contentItem, out _).Should().BeTrue();
            Inventory.TryAddItem(Context, stackingItem.WithData(15), out _).Should().BeTrue();

            Inventory.TryRemoveItemsInBulk(Context, BulkContentDeclaration, 5).Should().ContainInOrder(stackingItem.WithData(5));
            Inventory.Items.Should().ContainInOrder(stackingItem.WithData(60), contentItem, stackingItem.WithData(10));
            Inventory.TryRemoveItemsInBulk(Context, BulkContentDeclaration, 25).Should().ContainInOrder(stackingItem.WithData(10), stackingItem.WithData(15));
            Inventory.Items.Should().ContainInOrder(stackingItem.WithData(45), contentItem);
            Inventory.TryRemoveItemsInBulk(Context, ContentDeclaration, 25).Should().ContainInOrder(contentItem);
            Inventory.Items.Should().ContainInOrder(stackingItem.WithData(45));
            Inventory.TryRemoveItemsInBulk(Context, ContentDeclaration, 125).Should().BeEmpty();
            Inventory.TryRemoveItemsInBulk(Context, BulkContentDeclaration, 125).Should().ContainInOrder(stackingItem.WithData(45));
            Inventory.Items.Should().BeEmpty();
        }

        [Test]
        public void Validate_Unable_To_Add_If_Already_In_Other_Container()
        {
            var chest = Context.ItemResolver.Instantiate(Context, ContainerDeclaration);
            Context.ItemResolver.TryQueryData(chest, Context, out IInventory<InventoryTestContext, ItemReference> chestInventory).Should().BeTrue();

            var contentItem = Context.ItemResolver.Instantiate(Context, ContentDeclaration);
            chestInventory.TryAddItem(Context, contentItem, out _).Should().BeTrue();

            Inventory.TryAddItem(Context, contentItem, out _).Should().BeFalse();
            Inventory.TryAddItem(Context, chest, out _).Should().BeTrue();
        }

        [Test]
        public void Validate_DestroyCascade()
        {
            var chest = Context.ItemResolver.Instantiate(Context, ContainerDeclaration);
            var contentItem = Context.ItemResolver.Instantiate(Context, ContentDeclaration);

            Context.ItemResolver.TryQueryData(chest, Context, out IInventory<InventoryTestContext, ItemReference> chestInventory).Should().BeTrue();
            chestInventory.TryAddItem(Context, contentItem, out _).Should().BeTrue();
            Context.ItemResolver.TryUpdateData(chest, Context, chestInventory, out _).Should().BeTrue();
            
            Inventory.TryAddItem(Context, chest, out _).Should().BeTrue();
            Context.ActorResolver.TryUpdateData(Owner, Context, (IInventory<InventoryTestContext, ItemReference>)Inventory, out _).Should().BeTrue();
            

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

        void RunActionSystem(List<Action<InventoryTestContext>> actions)
        {
            Console.WriteLine("Running actions");
            foreach (var a in actions)
            {
                a(Context);
            }
        }
        

        List<Action<InventoryTestContext>> CreateActionSystem()
        {
            var dsa = new DestroyContainerContentsSystem<InventoryTestContext, ActorReference, ItemReference>(Context.ItemResolver); 
            var dsi = new DestroyContainerContentsSystem<InventoryTestContext, ItemReference, ItemReference>(Context.ItemResolver); 

            
            return new List<Action<InventoryTestContext>>
            {
                // Any inventory item that had been marked for delayed destruction is now marked as destroyable.
                Context.ActorEntities.BuildSystem()
                       .WithContext<InventoryTestContext>()
                       .CreateSystem<CascadingDestroyedMarker>(DestroyedEntitiesSystem<ActorReference>.SchedulePreviouslyMarkedItemsForDestruction),
                Context.ItemEntities.BuildSystem()
                       .WithContext<InventoryTestContext>()
                       .CreateSystem<CascadingDestroyedMarker>(DestroyedEntitiesSystem<ItemReference>.SchedulePreviouslyMarkedItemsForDestruction),

                
                // any destroyed item that is a container must mark its remaining container contents as scheduled for destruction at the 
                // next turn.
                Context.ActorEntities.BuildSystem()
                       .WithContext<InventoryTestContext>()
                       .CreateSystem<DestroyedMarker, ListInventoryData<ActorReference, ItemReference>>(dsa.MarkDestroyedContainerEntities),
                Context.ItemEntities.BuildSystem()
                       .WithContext<InventoryTestContext>()
                       .CreateSystem<DestroyedMarker, ListInventoryData<ItemReference, ItemReference>>(dsi.MarkDestroyedContainerEntities),

                // Finally clean up any item marked as destroyed.
                new DestroyedEntitiesSystem<ItemReference>(Context.ItemEntities).DeleteMarkedEntities,
                new DestroyedEntitiesSystem<ActorReference>(Context.ActorEntities).DeleteMarkedEntities,
            };
        }
    }
}