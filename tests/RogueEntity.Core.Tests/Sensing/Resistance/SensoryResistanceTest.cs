using FluentAssertions;
using NUnit.Framework;
using RogueEntity.Core.Sensing;
using RogueEntity.Core.Sensing.Resistance;
using RogueEntity.Core.Utils;

namespace RogueEntity.Core.Tests.Sensing.Resistance
{
    public class SensoryResistanceTest
    {
        [Test]
        public void DefaultCreationTest()
        {
            var sr = new SensoryResistance<VisionSense>();
            sr.BlocksSense.Should().Be(Percentage.Empty);
        }
        
        [Test]
        public void CreationTest()
        {
            var sr = new SensoryResistance<VisionSense>(Percentage.Of(0.4f));
            sr.BlocksSense.Should().Be(Percentage.Of(0.4f));
        }

        [Test]
        public void CombineTest()
        {
            var sr = new SensoryResistance<VisionSense>(Percentage.Of(0.1f));
            sr += new SensoryResistance<VisionSense>(Percentage.Of(0.4f));
            sr.BlocksSense.Should().Be(Percentage.Of(0.5));
        }

        [Test]
        public void EqualityTest()
        {
            var sr = new SensoryResistance<VisionSense>(Percentage.Of(0.1f));
            sr.Should().Be(new SensoryResistance<VisionSense>(Percentage.Of(0.1f)));
            
            sr.Should().NotBe(new SensoryResistance<VisionSense>(Percentage.Of(0.66f)));
            
        }
    }
}