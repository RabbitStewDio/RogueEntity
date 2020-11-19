using FluentAssertions;
using NUnit.Framework;
using RogueEntity.Core.Sensing.Common;
using RogueEntity.Core.Utils;

namespace RogueEntity.Core.Tests.Sensing.Common
{
    public class SenseSourceDataTest
    {
        [Test]
        public void TestWrite()
        {
            var sd = new SenseSourceData(5);
            sd.Write(new Position2D(0, 0), 10);
            sd[0, 0].Should().Be(10);
            
            sd.Write(new Position2D(-5, -5), 11);
            sd[-5, -5].Should().Be(11);
            
            sd.Write(new Position2D(5, 5), 12);
            sd[5, 5].Should().Be(12);
            
            sd[54, 54].Should().Be(0);
        }
    }
}