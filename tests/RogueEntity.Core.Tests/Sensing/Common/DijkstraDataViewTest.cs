using FluentAssertions;
using GoRogue;
using NUnit.Framework;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Utils.Algorithms;
using RogueEntity.Core.Utils.Maps;

namespace RogueEntity.Core.Tests.Sensing.Common
{
    public class DijkstraDataViewTest
    {
        [Test]
        public void TestAddressing()
        {
            var dview = new BoundedDataView<float>(new Rectangle(-1, -1, 11, 11));
            dview.TryGetRawIndex(new Position2D(4, 4), out var idx).Should().BeTrue();
            idx.Should().Be(5 + 5 * 11);

            dview.TryGetFromRawIndex(idx, out var pos).Should().BeTrue();
            pos.Should().Be(new Position2D(4, 4));

        }
    }
}