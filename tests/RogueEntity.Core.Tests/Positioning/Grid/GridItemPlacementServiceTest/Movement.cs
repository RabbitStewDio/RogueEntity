using FluentAssertions;
using NUnit.Framework;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.MapLayers;
using System.Diagnostics.CodeAnalysis;

namespace RogueEntity.Core.Tests.Positioning.Grid.GridItemPlacementServiceTest
{
    [TestFixture]
    [SuppressMessage("ReSharper", "UnusedVariable")]
    public class Movement : GridItemPlacementServiceFixture<Movement>
    {
        [Test]
        public void ValidatePreConditions()
        {
            var dummyLayer = new MapLayer(2, "Dummy Layer");
            
            var (refA, _) = this.With(ReferenceItemA).IsPlacedAt(Position.Of(DefaultLayer, 0, 0));

            PlacementService.TryMoveItem(refA, Position.Invalid, Position.Invalid).Should().BeTrue("because moving from an invalid position to an invalid position is a no-op");
            PlacementService.TryMoveItem(default, Position.Of(DefaultLayer, 1, 0), Position.Of(DefaultLayer, 0, 0)).Should().BeTrue("moving an empty entity is a no-op");
            PlacementService.TryMoveItem(refA, Position.Of(dummyLayer, 1, 0), Position.Of(DefaultLayer, 0, 0)).Should().BeFalse("because there is no dummy layer");
            PlacementService.TryMoveItem(refA, Position.Of(DefaultLayer, 1, 0), Position.Of(dummyLayer, 0, 0)).Should().BeFalse("because there is no dummy layer");
            PlacementService.TryMoveItem(refA, Position.Of(DefaultLayer, 1, 0, 1), Position.Of(DefaultLayer, 0, 0)).Should().BeFalse("because there is no Z-Level 1");
            PlacementService.TryMoveItem(refA, Position.Of(DefaultLayer, 0, 0), Position.Of(DefaultLayer, 0, 0, 1)).Should().BeTrue("because we do create Z-Levels on demand");
        }

        public void MovingAnEmptyEntityIsANoOp()
        {
            var (refA, posA) = this.GivenAnEntity(ReferenceItemA).IsPlacedAt(Position.Of(DefaultLayer, 0, 0));
            When(() => PlacementService.TryMoveItem(default, Position.Of(DefaultLayer, 1, 0), Position.Of(DefaultLayer, 0, 0)));

            Then_Operation_Should_Succeed();
            this.ThenPosition(posA).Should().ContainEntity(refA);
        }

        [Test]
        public void MoveReferenceItem()
        {
            var (refA, posA) = this.GivenAnEntity(ReferenceItemA).IsPlacedAt(Position.Of(DefaultLayer, 0, 0));
            var (_, posB) = this.GivenAnEmptySpace().IsPlacedAt(Position.Of(DefaultLayer, 1, 0));

            When(() => PlacementService.TryMoveItem(refA, posA, posB));

            Then_Operation_Should_Succeed();
            this.ThenPosition(posA).Should().BeEmpty();
            this.ThenPosition(posB).Should().ContainEntity(refA);
        }

        [Test]
        public void MoveReferenceItem_Source_Invalid()
        {
            var (refA, posA) = this.GivenAnEntity(ReferenceItemA).IsPlacedAt(Position.Of(DefaultLayer, 0, 0));
            var (refB, posB) = this.GivenAnEntity(ReferenceItemA).IsPlacedAt(Position.Of(DefaultLayer, 0, 10));
            var (_, posC) = this.GivenAnEmptySpace().IsPlacedAt(Position.Of(DefaultLayer, 1, 0));

            When(() => PlacementService.TryMoveItem(refA, posB, posC));

            Then_Operation_Should_Fail();
            this.ThenPosition(posA).Should().ContainEntity(refA);
            this.ThenPosition(posB).Should().ContainEntity(refB);
            this.ThenPosition(posC).Should().BeEmpty();
        }

        [Test]
        public void MoveReferenceItem_Target_Occupied()
        {
            var (refA, posA) = this.GivenAnEntity(ReferenceItemA).IsPlacedAt(Position.Of(DefaultLayer, 0, 0));
            var (refB, posB) = this.GivenAnEntity(StackingBulkItemA).WithStackSize(10).IsPlacedAt(Position.Of(DefaultLayer, 1, 0));

            When(() => PlacementService.TryMoveItem(refA, posA, posB));

            Then_Operation_Should_Fail();
            this.ThenPosition(posA).Should().ContainEntity(refA);
            this.ThenPosition(posB).Should().ContainEntity(refB);
        }

        [Test]
        public void MoveBulkItem()
        {
            var (refA, posA) = this.GivenAnEntity(StackingBulkItemA).WithStackSize(2).IsPlacedAt(Position.Of(DefaultLayer, 0, 0));
            var (_, posB) = this.GivenAnEmptySpace().IsPlacedAt(Position.Of(DefaultLayer, 1, 0));

            When(() => PlacementService.TryMoveItem(refA, posA, posB));

            Then_Operation_Should_Succeed();
            this.ThenPosition(posA).Should().BeEmpty();
            this.ThenPosition(posB).Should().ContainEntityOfType(StackingBulkItemA).WithStackSize(2);
        }

        [Test]
        public void MoveBulkItem_Source_Invalid()
        {
            var (refA, posA) = this.GivenAnEntity(StackingBulkItemA).WithStackSize(2).IsPlacedAt(Position.Of(DefaultLayer, 0, 0));
            var (refB, posB) = this.GivenAnEntity(ReferenceItemA).IsPlacedAt(Position.Of(DefaultLayer, 1, 0));
            var (_, posC) = this.GivenAnEmptySpace().IsPlacedAt(Position.Of(DefaultLayer, 3, 0));
            this.ThenPosition(posA).Should().ContainEntity(refA);
            this.ThenPosition(posB).Should().ContainEntity(refB);
            this.ThenPosition(posC).Should().BeEmpty();

            When(() => PlacementService.TryMoveItem(refA, posB, posC));

            Then_Operation_Should_Fail();
            this.ThenPosition(posA).Should().ContainEntity(refA);
            this.ThenPosition(posB).Should().ContainEntity(refB);
            this.ThenPosition(posC).Should().BeEmpty();
        }

        [Test]
        public void MoveBulkItem_Target_Invalid()
        {
            var (refA, posA) = this.GivenAnEntity(StackingBulkItemA).WithStackSize(2).IsPlacedAt(Position.Of(DefaultLayer, 0, 0));
            var (refB, posB) = this.GivenAnEntity(ReferenceItemA).IsPlacedAt(Position.Of(DefaultLayer, 1, 0));

            When(() => PlacementService.TryMoveItem(refA, posA, posB));

            Then_Operation_Should_Fail();
            this.ThenPosition(posA).Should().ContainEntity(refA);
            this.ThenPosition(posB).Should().ContainEntity(refB);
        }

        [Test]
        public void MoveBulkItem_Stackable()
        {
            var (refA, posA) = this.GivenAnEntity(StackingBulkItemA).WithStackSize(2).IsPlacedAt(Position.Of(DefaultLayer, 0, 0));
            var (refB, posB) = this.GivenAnEntity(StackingBulkItemA).WithStackSize(3).IsPlacedAt(Position.Of(DefaultLayer, 1, 0));

            When(() => PlacementService.TryMoveItem(refA, posA, posB));

            Then_Operation_Should_Succeed();
            this.ThenPosition(posA).Should().BeEmpty();
            this.ThenPosition(posB).Should().ContainEntityOfType(StackingBulkItemA).WithStackSize(5);
        }

        [Test]
        public void MoveBulkItem_Stackable_Take_LessThanSource()
        {
            var refC = this.GivenAnEntity(StackingBulkItemA).WithStackSize(2).InstantiatedWithoutPosition();
            var (refA, posA) = this.GivenAnEntity(StackingBulkItemA).WithStackSize(5).IsPlacedAt(Position.Of(DefaultLayer, 0, 0));
            var (_, posB) = this.GivenAnEmptySpace().IsPlacedAt(Position.Of(DefaultLayer, 1, 0));

            When(() => PlacementService.TryMoveItem(refC, posA, posB));

            Then_Operation_Should_Succeed();
            this.ThenPosition(posA).Should().ContainEntityOfType(StackingBulkItemA).WithStackSize(3);
            this.ThenPosition(posB).Should().ContainEntityOfType(StackingBulkItemA).WithStackSize(2);
        }

        [Test]
        public void MoveBulkItem_Stackable_Take_LessThanSource_TargetCompatible()
        {
            var refC = this.GivenAnEntity(StackingBulkItemA).WithStackSize(2).InstantiatedWithoutPosition();
            var (refA, posA) = this.GivenAnEntity(StackingBulkItemA).WithStackSize(5).IsPlacedAt(Position.Of(DefaultLayer, 0, 0));
            var (refB, posB) = this.GivenAnEntity(StackingBulkItemA).WithStackSize(3).IsPlacedAt(Position.Of(DefaultLayer, 1, 0));

            When(() => PlacementService.TryMoveItem(refC, posA, posB));

            Then_Operation_Should_Succeed();
            this.ThenPosition(posA).Should().ContainEntityOfType(StackingBulkItemA).WithStackSize(3);
            this.ThenPosition(posB).Should().ContainEntityOfType(StackingBulkItemA).WithStackSize(5);
        }

        [Test]
        public void MoveBulkItem_Stackable_Take_LessThanSource_Incompatible_Target()
        {
            var refC = this.GivenAnEntity(StackingBulkItemA).WithStackSize(2).InstantiatedWithoutPosition();
            var (refA, posA) = this.GivenAnEntity(StackingBulkItemA).WithStackSize(5).IsPlacedAt(Position.Of(DefaultLayer, 0, 0));
            var (refB, posB) = this.GivenAnEntity(StackingBulkItemB).WithStackSize(3).IsPlacedAt(Position.Of(DefaultLayer, 1, 0));

            When(() => PlacementService.TryMoveItem(refC, posA, posB));

            Then_Operation_Should_Fail();
            this.ThenPosition(posA).Should().ContainEntityOfType(StackingBulkItemA).WithStackSize(5);
            this.ThenPosition(posB).Should().ContainEntityOfType(StackingBulkItemB).WithStackSize(3);
        }

        [Test]
        public void MoveBulkItem_Stackable_Take_LessThanSource_Target_Exceeded()
        {
            var refC = this.GivenAnEntity(StackingBulkItemA).WithStackSize(2).InstantiatedWithoutPosition();
            var (refA, posA) = this.GivenAnEntity(StackingBulkItemA).WithStackSize(5).IsPlacedAt(Position.Of(DefaultLayer, 0, 0));
            var (refB, posB) = this.GivenAnEntity(StackingBulkItemA).WithStackSize(9).IsPlacedAt(Position.Of(DefaultLayer, 1, 0));

            // target contains 9/10 items, want to add 2 more, which should fail.
            When(() => PlacementService.TryMoveItem(refC, posA, posB));

            Then_Operation_Should_Fail();
            this.ThenPosition(posA).Should().ContainEntityOfType(StackingBulkItemA).WithStackSize(5);
            this.ThenPosition(posB).Should().ContainEntityOfType(StackingBulkItemA).WithStackSize(9);
        }

        [Test]
        public void MoveBulkItem_Stackable_NotEnoughItemsAtSource()
        {
            var refC = this.GivenAnEntity(StackingBulkItemA).WithStackSize(5).InstantiatedWithoutPosition();
            var (refA, posA) = this.GivenAnEntity(StackingBulkItemA).WithStackSize(2).IsPlacedAt(Position.Of(DefaultLayer, 0, 0));
            var (_, posB) = this.GivenAnEmptySpace().IsPlacedAt(Position.Of(DefaultLayer, 1, 0));

            When(() => PlacementService.TryMoveItem(refC, posA, posB));

            Then_Operation_Should_Fail();
            this.ThenPosition(posA).Should().ContainEntity(refA);
            this.ThenPosition(posB).Should().BeEmpty();
        }

        [Test]
        public void MoveBulkItem_Stackable_Target_NotEmpty()
        {
            var (refA, posA) = this.GivenAnEntity(StackingBulkItemA).WithStackSize(2).IsPlacedAt(Position.Of(DefaultLayer, 0, 0));
            var (refB, posB) = this.GivenAnEntity(ReferenceItemA).IsPlacedAt(Position.Of(DefaultLayer, 1, 0));

            When(() => PlacementService.TryMoveItem(refA, posA, posB));

            Then_Operation_Should_Fail();
            this.ThenPosition(posA).Should().ContainEntity(refA);
            this.ThenPosition(posB).Should().ContainEntity(refB);
        }

        [Test]
        public void MoveBulkItem_Stackable_Target_StackExceeded()
        {
            var (refA, posA) = this.GivenAnEntity(StackingBulkItemA).WithStackSize(8).IsPlacedAt(Position.Of(DefaultLayer, 0, 0));
            var (refB, posB) = this.GivenAnEntity(StackingBulkItemA).WithStackSize(5).IsPlacedAt(Position.Of(DefaultLayer, 1, 0));

            When(() => PlacementService.TryMoveItem(refA, posA, posB));

            Then_Operation_Should_Fail();
            this.ThenPosition(posA).Should().ContainEntity(refA);
            this.ThenPosition(posB).Should().ContainEntity(refB);
        }
    }
}
