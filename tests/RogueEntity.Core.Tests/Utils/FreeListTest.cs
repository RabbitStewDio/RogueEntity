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
            fl.Add(10).Should().Be(FreeListIndex.Of(0));
            fl.Range.Should().Be(1);
            fl.IsEmpty.Should().BeFalse();
            
            fl.Add(20).Should().Be(FreeListIndex.Of(1));
            fl.Range.Should().Be(2);
            fl.IsEmpty.Should().BeFalse();
            
            fl.Remove(FreeListIndex.Of(0));
            fl.Range.Should().Be(2, "because the underlying data store and thus the range of valid indices does not change after removal");
            fl.IsEmpty.Should().BeFalse("because the list still contains one item");
            
            fl[FreeListIndex.Of(1)].Should().Be(20, "because existing entries do not change");
            fl.Add(30).Should().Be(FreeListIndex.Of(0), "because the existing index will be reused");

            fl.Add(40).Should().Be(FreeListIndex.Of(2));
            fl.Add(50).Should().Be(FreeListIndex.Of(3));
            fl.Add(60).Should().Be(FreeListIndex.Of(4));

            fl.Remove(FreeListIndex.Of(4));
            fl.Remove(FreeListIndex.Of(1));
            fl.Remove(FreeListIndex.Of(2));
            
            fl.Add(70).Should().Be(FreeListIndex.Of(2));
            fl.Add(80).Should().Be(FreeListIndex.Of(1));
            fl.Add(90).Should().Be(FreeListIndex.Of(4));
        }
    }
}
