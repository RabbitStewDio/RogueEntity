using FluentAssertions;
using NUnit.Framework;
using RogueEntity.Core.Utils.SpatialIndex;

namespace RogueEntity.Core.Tests.Utils
{
    public class FreeListTest
    {
        [Test]
        public void ValidateOperation()
        {
            var fl = new FreeList<int>(0, 1);
            fl.Add(10).Should().Be(0);
            fl.Range.Should().Be(1);
            fl.IsEmpty.Should().BeFalse();
            
            fl.Add(20).Should().Be(1);
            fl.Range.Should().Be(2);
            fl.IsEmpty.Should().BeFalse();
            
            fl.Remove(0);
            fl.Range.Should().Be(2, "because the underlying data store and thus the range of valid indices does not change after removal");
            fl.IsEmpty.Should().BeFalse("because the list still contains one item");
            
            fl[1].Should().Be(20, "because existing entries do not change");
            fl.Add(30).Should().Be(0, "because the existing index will be reused");

            fl.Add(40).Should().Be(2);
            fl.Add(50).Should().Be(3);
            fl.Add(60).Should().Be(4);

            fl.Remove(4);
            fl.Remove(1);
            fl.Remove(2);
            
            fl.Add(70).Should().Be(2);
            fl.Add(80).Should().Be(1);
            fl.Add(90).Should().Be(4);
        }
    }
}
