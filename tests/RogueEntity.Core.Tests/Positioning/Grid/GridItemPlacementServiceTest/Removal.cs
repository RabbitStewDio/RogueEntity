using FluentAssertions;
using NUnit.Framework;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.MapLayers;
using RogueEntity.Core.Tests.Fixtures;

namespace RogueEntity.Core.Tests.Positioning.Grid.GridItemPlacementServiceTest
{
    [TestFixture]
    public class Removal: GridItemPlacementServiceFixture<Removal>
    {
        [Test]
        public void ValidatePreConditions()
        {
            var dummyLayer = new MapLayer(2, "Dummy Layer");
            var refA = this.GivenAnEntity(StackingBulkItemA).WithStackSize(2).InstantiatedWithoutPosition();
            
            PlacementService.TryRemoveItem(refA, Position.Invalid).Should().BeFalse("because we cannot 'place' empty items.");
            PlacementService.TryRemoveItem(refA, Position.Of(dummyLayer, 1, 0)).Should().BeFalse("because there is no dummy layer");
        }

        [Test]
        public void RemoveReferenceItem_When_CellEmpty()
        {
            var refC = this.GivenAnEntity(ReferenceItemA).InstantiatedWithoutPosition();
            var (_, posB) = this.GivenAnEmptySpace().IsPlacedAt(Position.Of(DefaultLayer, 0, 0));

            When(_ => PlacementService.TryRemoveItem(refC, posB));
            
            Then_Operation_Should_Fail();
            this.ThenPosition(posB).Should().BeEmpty();

            this.ItemEntityContext.ItemResolver.TryQueryData(refC, out Position pos).Should().BeTrue();
            pos.Should().Be(Position.Invalid);
        }

        [Test]
        public void RemoveReferenceItem_When_CellDifferentOccupant()
        {
            var refC = this.GivenAnEntity(ReferenceItemA).InstantiatedWithoutPosition();
            var (refA, posA) = this.GivenAnEntity(ReferenceItemA).IsPlacedAt(Position.Of(DefaultLayer, 0, 0));

            When(_ => PlacementService.TryRemoveItem(refC, posA));
            
            Then_Operation_Should_Fail();
            this.ThenPosition(posA).Should().ContainEntity(refA);

        }

        [Test]
        public void RemoveReferenceItem()
        {
            var (refA, posA) = this.GivenAnEntity(ReferenceItemA).IsPlacedAt(Position.Of(DefaultLayer, 0, 0));

            When(_ => PlacementService.TryRemoveItem(refA, posA));
            
            Then_Operation_Should_Succeed();
            this.ThenPosition(posA).Should().BeEmpty();
            
            this.ItemEntityContext.ItemResolver.TryQueryData(refA, out Position pos).Should().BeTrue();
            pos.Should().Be(Position.Invalid);
        }

        [Test]
        public void RemoveBulkItem_Matching()
        {
            var (refA, posA) = this.GivenAnEntity(StackingBulkItemA).IsPlacedAt(Position.Of(DefaultLayer, 0, 0));

            When(_ => PlacementService.TryRemoveItem(refA, posA));
            
            Then_Operation_Should_Succeed();
            this.ThenPosition(posA).Should().BeEmpty();
        }

        [Test]
        public void RemoveBulkItem_When_CellEmpty()
        {
            var refC = this.GivenAnEntity(StackingBulkItemA).InstantiatedWithoutPosition();
            var (_, posB) = this.GivenAnEmptySpace().IsPlacedAt(Position.Of(DefaultLayer, 0, 0));

            When(_ => PlacementService.TryRemoveItem(refC, posB));
            
            Then_Operation_Should_Fail();
            this.ThenPosition(posB).Should().BeEmpty();
        }

        [Test]
        public void RemoveBulkItem_When_CellDifferentOccupant()
        {
            var refC = this.GivenAnEntity(StackingBulkItemA).InstantiatedWithoutPosition();
            var (refA, posA) = this.GivenAnEntity(ReferenceItemA).IsPlacedAt(Position.Of(DefaultLayer, 0, 0));

            When(_ => PlacementService.TryRemoveItem(refC, posA));
            
            Then_Operation_Should_Fail();
            this.ThenPosition(posA).Should().ContainEntity(refA);
        }

        [Test]
        public void RemoveBulkItem_Stacking_LessThanInCell()
        {
            var refC = this.GivenAnEntity(StackingBulkItemA).WithStackSize(2).InstantiatedWithoutPosition();
            var (_, posA) = this.GivenAnEntity(StackingBulkItemA).WithStackSize(5).IsPlacedAt(Position.Of(DefaultLayer, 0, 0));

            When(_ => PlacementService.TryRemoveItem(refC, posA));
            
            Then_Operation_Should_Succeed();
            this.ThenPosition(posA).Should().ContainEntityOfType(StackingBulkItemA).WithStackSize(3);
        }

        [Test]
        public void RemoveBulkItem_Stacking_MoreThanInCell()
        {
            var refC = this.GivenAnEntity(StackingBulkItemA).WithStackSize(5).InstantiatedWithoutPosition();
            var (_, posA) = this.GivenAnEntity(StackingBulkItemA).WithStackSize(2).IsPlacedAt(Position.Of(DefaultLayer, 0, 0));

            When(_ => PlacementService.TryRemoveItem(refC, posA));
            
            Then_Operation_Should_Fail();
            this.ThenPosition(posA).Should().ContainEntityOfType(StackingBulkItemA).WithStackSize(2);

        }
    }
}
