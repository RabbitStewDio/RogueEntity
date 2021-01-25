using FluentAssertions;
using NUnit.Framework;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.MapLayers;
using RogueEntity.Core.Tests.Fixtures;

namespace RogueEntity.Core.Tests.Positioning.Grid.GridItemPlacementServiceTest
{
    [TestFixture]
    public class Swapping : GridItemPlacementServiceFixture
    {
        [Test]
        public void ValidatePreConditions()
        {
            var dummyLayer = new MapLayer(2, "Dummy Layer");
            var refA = this.GivenAnEntity(StackingBulkItemA).WithStackSize(2).InstantiatedWithoutPosition();
            var refC = this.GivenAnEntity(StackingBulkItemB).WithStackSize(2).InstantiatedWithoutPosition();
            var (refB, posB) = this.GivenAnEntity(StackingBulkItemA).WithStackSize(2).IsPlacedAt(Position.Of(DefaultLayer, 0, 0));
            
            PlacementService.TrySwapItem(refA, Position.Invalid, refB, posB).Should().BeFalse("because we cannot 'place' empty items.");
            PlacementService.TrySwapItem(refB, posB, refA, Position.Invalid).Should().BeFalse("because we cannot 'place' empty items.");
            PlacementService.TrySwapItem(refA, Position.Invalid, refC, Position.Invalid).Should().BeTrue("because swapping non-placed items is fine.");
            
            PlacementService.TrySwapItem(refA, Position.Of(dummyLayer, 1, 0), refB, posB).Should().BeFalse("because there is no dummy layer");
            PlacementService.TrySwapItem(refB, posB, refA, Position.Of(dummyLayer, 1, 0)).Should().BeFalse("because there is no dummy layer");
            
            PlacementService.TrySwapItem(default, Position.Of(DefaultLayer, 0, 0), default, Position.Of(dummyLayer, 1, 0)).Should().BeTrue("because swapping two empty space is a no-op.");
        }

        [Test]
        public void SwapItemsWithEmptySource()
        {
            var (refA, posA) = this.GivenAnEntity(ReferenceItemA).IsPlacedAt(Position.Of(DefaultLayer, 0, 0));
            var (_, posB) = this.GivenAnEmptySpace().IsPlacedAt(Position.Of(DefaultLayer, 1, 0));

            When(_ => PlacementService.TrySwapItem(refA, posA, default, posB));
            
            Then_Operation_Should_Succeed();
            this.Then_Position(posA).Should().BeEmpty();
            this.Then_Position(posB).Should().ContainEntity(refA);
        }
        
        [Test]
        public void Swap_Items_With_Empty_Target()
        {
            var (refA, posA) = this.GivenAnEntity(ReferenceItemA).IsPlacedAt(Position.Of(DefaultLayer, 0, 0));
            var (_, posB) = this.GivenAnEmptySpace().IsPlacedAt(Position.Of(DefaultLayer, 1, 0));

            When(_ => PlacementService.TrySwapItem(default, posB, refA, posA));
            
            Then_Operation_Should_Succeed();
            this.Then_Position(posA).Should().BeEmpty();
            this.Then_Position(posB).Should().ContainEntity(refA);
        }

        [Test]
        public void Swap_ReferenceItem_With_FullStackBulkItem()
        {
            var (refA, posA) = this.GivenAnEntity(ReferenceItemA).IsPlacedAt(Position.Of(DefaultLayer, 0, 0));
            var (refB, posB) = this.GivenAnEntity(StackingBulkItemA).WithStackSize(5).IsPlacedAt(Position.Of(DefaultLayer, 1, 0));
            
            When(_ => PlacementService.TrySwapItem(refA, posA, refB, posB));
            
            Then_Operation_Should_Succeed();
            this.Then_Position(posA).Should().ContainEntityOfType(StackingBulkItemA).WithStackSize(5);
            this.Then_Position(posB).Should().ContainEntity(refA);
        }

        [Test]
        public void Swap_ReferenceItem_With_Reference()
        {
            var (refA, posA) = this.GivenAnEntity(ReferenceItemA).IsPlacedAt(Position.Of(DefaultLayer, 0, 0));
            var (refB, posB) = this.GivenAnEntity(ReferenceItemB).IsPlacedAt(Position.Of(DefaultLayer, 1, 0));
            
            When(_ => PlacementService.TrySwapItem(refA, posA, refB, posB));
            
            Then_Operation_Should_Succeed();
            this.Then_Position(posA).Should().ContainEntity(refB);
            this.Then_Position(posB).Should().ContainEntity(refA);
        }
        
        [Test]
        public void Swap_ReferenceItem_With_Non_Matching_Source()
        {
            var (refA, posA) = this.GivenAnEntity(ReferenceItemA).IsPlacedAt(Position.Of(DefaultLayer, 0, 0));
            var (refB, posB) = this.GivenAnEntity(ReferenceItemB).IsPlacedAt(Position.Of(DefaultLayer, 1, 0));
            var refC = this.GivenAnEntity(ReferenceItemB).InstantiatedWithoutPosition();
            
            When(_ => PlacementService.TrySwapItem(refC, posA, refB, posB));
            
            Then_Operation_Should_Fail();
            this.Then_Position(posA).Should().ContainEntity(refA);
            this.Then_Position(posB).Should().ContainEntity(refB);
        }
        
        [Test]
        public void Swap_ReferenceItem_With_Non_Matching_Target()
        {
            var (refA, posA) = this.GivenAnEntity(ReferenceItemA).IsPlacedAt(Position.Of(DefaultLayer, 0, 0));
            var (refB, posB) = this.GivenAnEntity(ReferenceItemB).IsPlacedAt(Position.Of(DefaultLayer, 1, 0));
            var refC = this.GivenAnEntity(ReferenceItemB).InstantiatedWithoutPosition();
            
            When(_ => PlacementService.TrySwapItem(refA, posA, refC, posB));
            
            Then_Operation_Should_Fail();
            this.Then_Position(posA).Should().ContainEntity(refA);
            this.Then_Position(posB).Should().ContainEntity(refB);
        }

        [Test]
        public void Swap_FullStackBulk_With_FullStack_Bulk()
        {
            var (refA, posA) = this.GivenAnEntity(StackingBulkItemA).WithStackSize(2).IsPlacedAt(Position.Of(DefaultLayer, 0, 0));
            var (refB, posB) = this.GivenAnEntity(StackingBulkItemB).WithStackSize(3).IsPlacedAt(Position.Of(DefaultLayer, 1, 0));
            
            When(_ => PlacementService.TrySwapItem(refA, posA, refB, posB));
            
            Then_Operation_Should_Succeed();
            this.Then_Position(posA).Should().ContainEntity(refB);
            this.Then_Position(posB).Should().ContainEntity(refA);
        }
        
        [Test]
        public void Swap_ReferenceItem_With_PartialStack()
        {
            var (refA, posA) = this.GivenAnEntity(StackingBulkItemA).WithStackSize(5).IsPlacedAt(Position.Of(DefaultLayer, 0, 0));
            var (refB, posB) = this.GivenAnEntity(ReferenceItemA).IsPlacedAt(Position.Of(DefaultLayer, 1, 0));
            var refC = this.GivenAnEntity(StackingBulkItemA).WithStackSize(2).InstantiatedWithoutPosition();
            
            When(_ => PlacementService.TrySwapItem(refC, posA, refB, posB));
            
            Then_Operation_Should_Fail();
            this.Then_Position(posA).Should().ContainEntity(refA);
            this.Then_Position(posB).Should().ContainEntity(refB);
        }
        
        [Test]
        public void Swap_Incompatible_Partial_BulkItem_Stacks()
        {
            var (refA, posA) = this.GivenAnEntity(StackingBulkItemA).WithStackSize(5).IsPlacedAt(Position.Of(DefaultLayer, 0, 0));
            var (refB, posB) = this.GivenAnEntity(StackingBulkItemB).WithStackSize(4).IsPlacedAt(Position.Of(DefaultLayer, 1, 0));
            var refC = this.GivenAnEntity(StackingBulkItemA).WithStackSize(2).InstantiatedWithoutPosition();
            var refD = this.GivenAnEntity(StackingBulkItemB).WithStackSize(2).InstantiatedWithoutPosition();
            
            When(_ => PlacementService.TrySwapItem(refC, posA, refD, posB));
            
            Then_Operation_Should_Fail();
            this.Then_Position(posA).Should().ContainEntity(refA);
            this.Then_Position(posB).Should().ContainEntity(refB);
        }
        
        [Test]
        public void Swap_Compatible_Partial_BulkItem_Stacks()
        {
            var (refA, posA) = this.GivenAnEntity(StackingBulkItemA).WithStackSize(5).IsPlacedAt(Position.Of(DefaultLayer, 0, 0));
            var (refB, posB) = this.GivenAnEntity(StackingBulkItemA).WithStackSize(4).IsPlacedAt(Position.Of(DefaultLayer, 1, 0));
            var refC = this.GivenAnEntity(StackingBulkItemA).WithStackSize(2).InstantiatedWithoutPosition();
            var refD = this.GivenAnEntity(StackingBulkItemA).WithStackSize(3).InstantiatedWithoutPosition();
            
            When(_ => PlacementService.TrySwapItem(refC, posA, refD, posB));
            
            Then_Operation_Should_Succeed();
            this.Then_Position(posA).Should().ContainEntityOfType(StackingBulkItemA).WithStackSize(6);
            this.Then_Position(posB).Should().ContainEntityOfType(StackingBulkItemA).WithStackSize(3);
        }
        
        [Test]
        public void Swap_Compatible_Partial_BulkItem_Stacks_NotEnough_At_Source()
        {
            var (refA, posA) = this.GivenAnEntity(StackingBulkItemA).WithStackSize(5).IsPlacedAt(Position.Of(DefaultLayer, 0, 0));
            var (refB, posB) = this.GivenAnEntity(StackingBulkItemA).WithStackSize(4).IsPlacedAt(Position.Of(DefaultLayer, 1, 0));
            var refC = this.GivenAnEntity(StackingBulkItemA).WithStackSize(6).InstantiatedWithoutPosition();
            var refD = this.GivenAnEntity(StackingBulkItemA).WithStackSize(3).InstantiatedWithoutPosition();
            
            When(_ => PlacementService.TrySwapItem(refC, posA, refD, posB));
            
            Then_Operation_Should_Succeed();
            this.Then_Position(posA).Should().ContainEntityOfType(StackingBulkItemA).WithStackSize(2);
            this.Then_Position(posB).Should().ContainEntityOfType(StackingBulkItemA).WithStackSize(7);
        }
        
        [Test]
        public void Swap_Compatible_Partial_BulkItem_Stacks_NotEnough_At_Target()
        {
            var (refA, posA) = this.GivenAnEntity(StackingBulkItemA).WithStackSize(5).IsPlacedAt(Position.Of(DefaultLayer, 0, 0));
            var (refB, posB) = this.GivenAnEntity(StackingBulkItemA).WithStackSize(4).IsPlacedAt(Position.Of(DefaultLayer, 1, 0));
            var refC = this.GivenAnEntity(StackingBulkItemA).WithStackSize(2).InstantiatedWithoutPosition();
            var refD = this.GivenAnEntity(StackingBulkItemA).WithStackSize(5).InstantiatedWithoutPosition();
            
            When(_ => PlacementService.TrySwapItem(refC, posA, refD, posB));
            
            Then_Operation_Should_Succeed();
            this.Then_Position(posA).Should().ContainEntityOfType(StackingBulkItemA).WithStackSize(8);
            this.Then_Position(posB).Should().ContainEntityOfType(StackingBulkItemA).WithStackSize(1);
        }
        
        [Test]
        public void Swap_Compatible_Partial_BulkItem_Stacks_Result_Exceeds_Source_StackSize()
        {
            var (refA, posA) = this.GivenAnEntity(StackingBulkItemA).WithStackSize(5).IsPlacedAt(Position.Of(DefaultLayer, 0, 0));
            var (refB, posB) = this.GivenAnEntity(StackingBulkItemA).WithStackSize(9).IsPlacedAt(Position.Of(DefaultLayer, 1, 0));
            var refC = this.GivenAnEntity(StackingBulkItemA).WithStackSize(2).InstantiatedWithoutPosition();
            var refD = this.GivenAnEntity(StackingBulkItemA).WithStackSize(8).InstantiatedWithoutPosition();
            
            When(_ => PlacementService.TrySwapItem(refC, posA, refD, posB));
            
            Then_Operation_Should_Fail();
            this.Then_Position(posA).Should().ContainEntity(refA);
            this.Then_Position(posB).Should().ContainEntity(refB);
        }
        
        [Test]
        public void Swap_Compatible_Partial_BulkItem_Stacks_Result_Exceeds_Target_StackSize()
        {
            var (refA, posA) = this.GivenAnEntity(StackingBulkItemA).WithStackSize(9).IsPlacedAt(Position.Of(DefaultLayer, 0, 0));
            var (refB, posB) = this.GivenAnEntity(StackingBulkItemA).WithStackSize(5).IsPlacedAt(Position.Of(DefaultLayer, 1, 0));
            var refC = this.GivenAnEntity(StackingBulkItemA).WithStackSize(8).InstantiatedWithoutPosition();
            var refD = this.GivenAnEntity(StackingBulkItemA).WithStackSize(2).InstantiatedWithoutPosition();
            
            When(_ => PlacementService.TrySwapItem(refC, posA, refD, posB));
            
            Then_Operation_Should_Fail();
            this.Then_Position(posA).Should().ContainEntity(refA);
            this.Then_Position(posB).Should().ContainEntity(refB);
        }
        
        
    }
}
