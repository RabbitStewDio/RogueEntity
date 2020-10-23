using FluentAssertions;
using NUnit.Framework;
using RogueEntity.Core.Sensing.Resistance;
using RogueEntity.Core.Utils;

namespace RogueEntity.Core.Tests.Sensing.Resistance
{
    public class SensoryResistanceTest
    {
        [Test]
        public void DefaultCreationTest()
        {
            var sr = new SensoryResistance();
            sr.BlocksHeat.Should().Be(Percentage.Empty);
            sr.BlocksSmell.Should().Be(Percentage.Empty);
            sr.BlocksSound.Should().Be(Percentage.Empty);
            sr.BlocksLight.Should().Be(Percentage.Empty);
        }
        
        [Test]
        public void CreationTest()
        {
            var sr = new SensoryResistance(Percentage.Of(0.1f), Percentage.Of(0.2f), Percentage.Of(0.3f), Percentage.Of(0.4f));
            sr.BlocksSmell.Should().Be(Percentage.Of(0.4f));
            sr.BlocksHeat.Should().Be(Percentage.Of(0.3f));
            sr.BlocksSound.Should().Be(Percentage.Of(0.2f));
            sr.BlocksLight.Should().Be(Percentage.Of(0.1f));
        }

        [Test]
        public void CombineTest()
        {
            var sr = new SensoryResistance(Percentage.Of(0.1f), Percentage.Of(0.2f), Percentage.Of(0.3f), Percentage.Of(0.4f));
            sr += new SensoryResistance(Percentage.Of(0.4f), Percentage.Of(0.5f), Percentage.Of(0.6f), Percentage.Of(1));
            sr.BlocksLight.Should().Be(Percentage.Of(0.5));
            sr.BlocksSound.Should().Be(Percentage.Of(0.7f));
            sr.BlocksHeat.Should().Be(Percentage.Of(0.9f));
            sr.BlocksSmell.Should().Be(Percentage.Of(1f));
        }

        [Test]
        public void EqualityTest()
        {
            var sr = new SensoryResistance(Percentage.Of(0.1f), Percentage.Of(0.2f), Percentage.Of(0.3f), Percentage.Of(0.4f));
            sr.Should().Be(new SensoryResistance(Percentage.Of(0.1f), Percentage.Of(0.2f), Percentage.Of(0.3f), Percentage.Of(0.4f)));
            
            sr.Should().NotBe(new SensoryResistance(Percentage.Of(0.66f), Percentage.Of(0.2f), Percentage.Of(0.3f), Percentage.Of(0.4f)));
            sr.Should().NotBe(new SensoryResistance(Percentage.Of(0.1f), Percentage.Of(0.66), Percentage.Of(0.3f), Percentage.Of(0.4f)));
            sr.Should().NotBe(new SensoryResistance(Percentage.Of(0.1f), Percentage.Of(0.2f), Percentage.Of(0.66f), Percentage.Of(0.4f)));
            sr.Should().NotBe(new SensoryResistance(Percentage.Of(0.1f), Percentage.Of(0.2f), Percentage.Of(0.3f), Percentage.Of(0.66f)));
            
        }
    }
}