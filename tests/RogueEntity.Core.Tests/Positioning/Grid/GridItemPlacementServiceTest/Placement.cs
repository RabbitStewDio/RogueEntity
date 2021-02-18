using FluentAssertions;
using NUnit.Framework;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.MapLayers;
using RogueEntity.Core.Tests.Fixtures;

namespace RogueEntity.Core.Tests.Positioning.Grid.GridItemPlacementServiceTest
{
    [TestFixture]
    public class Placement: GridItemPlacementServiceFixture
    {
        [Test]
        public void ValidatePreConditions()
        {
            var dummyLayer = new MapLayer(2, "Dummy Layer");
            var refA = this.GivenAnEntity(StackingBulkItemA).WithStackSize(2).InstantiatedWithoutPosition();
            
            PlacementService.TryPlaceItem(refA, Position.Invalid).Should().BeFalse("because we cannot 'place' empty items.");
            PlacementService.TryPlaceItem(refA, Position.Of(dummyLayer, 1, 0)).Should().BeFalse("because there is no dummy layer");
        }
        
        [Test]
        public void PlaceBulkItemInEmptyCell()
        {
            var refC = this.GivenAnEntity(StackingBulkItemA).WithStackSize(2).InstantiatedWithoutPosition();
            var (_, posB) = this.GivenAnEmptySpace().IsPlacedAt(Position.Of(DefaultLayer, 0, 0));

            When(_ => PlacementService.TryPlaceItem(refC, posB));
            
            Then_Operation_Should_Succeed();
            this.Then_Position(posB).Should().ContainEntity(refC);
        }

        [Test]
        public void PlaceBulkItemInOccupiedCell_Stackable_Compatible_WithEnoughSpace()
        {
            var refC = this.GivenAnEntity(StackingBulkItemA).WithStackSize(2).InstantiatedWithoutPosition();
            var (_, posB) = this.GivenAnEntity(StackingBulkItemA).WithStackSize(5).IsPlacedAt(Position.Of(DefaultLayer, 0, 0));

            When(_ => PlacementService.TryPlaceItem(refC, posB));
            
            Then_Operation_Should_Succeed();
            this.Then_Position(posB).Should().ContainEntityOfType(StackingBulkItemA).WithStackSize(7);
        }

        [Test]
        public void PlaceBulkItemInOccupiedCell_Stackable_Compatible_WithoutEnoughSpace()
        {
            var refC = this.GivenAnEntity(StackingBulkItemA).WithStackSize(2).InstantiatedWithoutPosition();
            var (refB, posB) = this.GivenAnEntity(StackingBulkItemA).WithStackSize(9).IsPlacedAt(Position.Of(DefaultLayer, 0, 0));

            When(_ => PlacementService.TryPlaceItem(refC, posB));
            
            Then_Operation_Should_Fail();
            this.Then_Position(posB).Should().ContainEntity(refB);
        }

        [Test]
        public void PlaceBulkItemInOccupiedCell_Stackable_Compatible_WithoutFullStack_Over_Occupied_Cell()
        {
            var refC = this.GivenAnEntity(StackingBulkItemA).WithStackSize(10).InstantiatedWithoutPosition();
            var (refB, posB) = this.GivenAnEntity(StackingBulkItemA).WithStackSize(1).IsPlacedAt(Position.Of(DefaultLayer, 0, 0));

            When(_ => PlacementService.TryPlaceItem(refC, posB));
            
            Then_Operation_Should_Fail();
            this.Then_Position(posB).Should().ContainEntity(refB);
        }

        [Test]
        public void PlaceBulkItemInOccupiedCell_Stackable_Incompatible()
        {
            var refC = this.GivenAnEntity(StackingBulkItemA).WithStackSize(2).InstantiatedWithoutPosition();
            var (refB, posB) = this.GivenAnEntity(StackingBulkItemB).WithStackSize(1).IsPlacedAt(Position.Of(DefaultLayer, 0, 0));

            When(_ => PlacementService.TryPlaceItem(refC, posB));
            
            Then_Operation_Should_Fail();
            this.Then_Position(posB).Should().ContainEntity(refB);
        }

        [Test]
        public void PlaceBulkItemInOccupiedCell_Non_Stackable()
        {
            var refC = this.GivenAnEntity(StackingBulkItemA).WithStackSize(2).InstantiatedWithoutPosition();
            var (refB, posB) = this.GivenAnEntity(ReferenceItemB).IsPlacedAt(Position.Of(DefaultLayer, 0, 0));

            When(_ => PlacementService.TryPlaceItem(refC, posB));
            
            Then_Operation_Should_Fail();
            this.Then_Position(posB).Should().ContainEntity(refB);
        }

        [Test]
        public void PlaceReferenceItemInEmptyCell()
        {
            var refC = this.GivenAnEntity(ReferenceItemA).InstantiatedWithoutPosition();
            var (_, posB) = this.GivenAnEmptySpace().IsPlacedAt(Position.Of(DefaultLayer, 0, 0));

            When(_ => PlacementService.TryPlaceItem(refC, posB));
            
            Then_Operation_Should_Succeed();
            this.Then_Position(posB).Should().ContainEntity(refC);
        }

        [Test]
        public void PlaceReferenceItemInOccupiedCell()
        {
            var refC = this.GivenAnEntity(ReferenceItemA).InstantiatedWithoutPosition();
            var (refB, posB) = this.GivenAnEntity(ReferenceItemB).IsPlacedAt(Position.Of(DefaultLayer, 0, 0));

            When(_ => PlacementService.TryPlaceItem(refC, posB));
            
            Then_Operation_Should_Fail();
            this.Then_Position(posB).Should().ContainEntity(refB);
        }
    }
}
