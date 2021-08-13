using FluentAssertions;
using NUnit.Framework;
using RogueEntity.Core.Positioning;

namespace RogueEntity.Core.Tests.Positioning
{
    [TestFixture]
    public class PositionTest
    {
        [Test]
        public void TestGridPos()
        {
            var pos = new Position(-1, -2, 1, 0);
            pos.X.Should().Be(-1);
            pos.Y.Should().Be(-2);
            pos.Z.Should().Be(1);
            
            pos.GridX.Should().Be(-1);
            pos.GridY.Should().Be(-2);
            pos.GridZ.Should().Be(1);
        }
    }
}
