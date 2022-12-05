using NUnit.Framework;
using RogueEntity.Core.Positioning;

namespace RogueEntity.Generator.Tests
{
    [TestFixture]
    public class MapOperationsTest : MapOperationsFixtureBase
    {
        [Test]
        public void Clear_Bulk()
        {
            var (_, pos) = GivenAnItem(BulkItem1).IsPlacedAt(Position.Of(ItemLayer, 0, 0));

            When(_ => MapBuilder.Clear(Position.Of(ItemLayer, 0, 0)));

            Then_Operation_Should_Succeed();
            Items.ThenPosition(pos).Should().BeEmpty();
        }

        [Test]
        public void Clear_ReferenceItem()
        {
            var (item, pos) = GivenAnItem(ReferenceItem1).IsPlacedAt(Position.Of(ItemLayer, 0, 0));

            When(_ => MapBuilder.Clear(Position.Of(ItemLayer, 0, 0)));

            Then_Operation_Should_Succeed();
            Items.ThenPosition(pos).Should().BeEmpty();
            Items.ThenItem(item).Should().BeDestroyed();
        }

        [Test]
        public void Clear_Empty()
        {
            var (_, pos) = GivenAnEmptyItem().IsPlacedAt(Position.Of(ItemLayer, 0, 0));

            When(_ => MapBuilder.Clear(Position.Of(ItemLayer, 0, 0)));

            Then_Operation_Should_Succeed();
            Items.ThenPosition(pos).Should().BeEmpty();
        }

        [Test]
        public void Instantiate_BulkItem_Over_Empty()
        {
            var (_, pos) = GivenAnEmptyItem().IsPlacedAt(Position.Of(ItemLayer, 0, 0));

            When(_ => MapBuilder.Instantiate(BulkItem1, Position.Of(ItemLayer, 0, 0)));

            Then_Operation_Should_Succeed();
            Items.ThenPosition(pos).Should().ContainEntityOfType(BulkItem1).WithStackSize(5);
        }

        [Test]
        public void Instantiate_ReferenceItem_Over_Empty()
        {
            var (_, pos) = GivenAnEmptyItem().IsPlacedAt(Position.Of(ItemLayer, 0, 0));

            When(_ => MapBuilder.Instantiate(ReferenceItem1, Position.Of(ItemLayer, 0, 0)));

            Then_Operation_Should_Succeed();
            Items.ThenPosition(pos).Should().ContainEntityOfType(ReferenceItem1)
                 .AndThatItem().BeActive();
        }
    }
}
