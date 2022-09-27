using FluentAssertions;
using NUnit.Framework;
using RogueEntity.Core.Utils.SpatialIndex;
using System;

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
        
        
        struct Payload : ISmartFreeListElement<Payload>, IEquatable<Payload>
        {
            public bool Active;
            int Data { get; set; }
            public Payload AsFreePointer(FreeListIndex ptr)
            {
                return new Payload()
                {
                    Active = false,
                    Data = ptr.Value + 1
                };
            }

            public FreeListIndex FreePointer => FreeListIndex.Of(Data);

            public Payload(bool active, int freePointer)
            {
                Active = active;
                Data = freePointer;
            }

            public bool Equals(Payload other)
            {
                return Active == other.Active && Data == other.Data;
            }

            public override bool Equals(object? obj)
            {
                return obj is Payload other && Equals(other);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (Active.GetHashCode() * 397) ^ Data;
                }
            }

            public static bool operator ==(Payload left, Payload right)
            {
                return left.Equals(right);
            }

            public static bool operator !=(Payload left, Payload right)
            {
                return !left.Equals(right);
            }
        }

        [Test]
        public void BasicUseTest()
        {
            var fl = new SmartFreeList<Payload>();
            var ix0 = fl.Add(new Payload(true, 0));
            var ix1 = fl.Add(new Payload(true, 1));
            var ix2 = fl.Add(new Payload(true, 2));
            fl.TryGetValue(ix0, out var t1).Should().Be(true);
            t1.Should().Be(new Payload(true, 0));
            fl[ix0].Active.Should().BeTrue();

            fl.TryGetValue(ix1, out var t2).Should().Be(true);
            t2.Should().Be(new Payload(true, 1));
            fl[ix1].Active.Should().BeTrue();
            
            fl.TryGetValue(ix2, out var t3).Should().Be(true);
            t3.Should().Be(new Payload(true, 2));
            fl[ix2].Active.Should().BeTrue();

            fl.Range.Should().Be(128);
            fl.Count.Should().Be(3);
            
            fl.Remove(ix0);
            fl.Count.Should().Be(2);
            fl[ix0].Active.Should().BeFalse();

            var ix3 = fl.Add(new Payload(true, 3));
            ix3.Should().Be(ix0);
            
            fl.Add(new Payload(true, 4));
            fl.Count.Should().Be(4);
        }
    }
}
