using FluentAssertions;
using NUnit.Framework;
using RogueEntity.Core.Utils;

namespace RogueEntity.Core.Tests.Utils
{
    public class BoundingBoxTest
    {
        [Test]
        public void Intersection_Match()
        {
            var bb1 = BoundingBox.From(10, 10, 20, 20);
            var bb2 = BoundingBox.From(10, 10, 20, 20);

            bb1.Intersects(bb2).Should().BeTrue();
        }

        [Test]
        public void Intersection_Partial_Larger()
        {
            var bb1 = BoundingBox.From(10, 10, 20, 20);
            var bb2 = BoundingBox.From(9, 10, 20, 20);
            var bb3 = BoundingBox.From(10, 9, 20, 20);
            var bb4 = BoundingBox.From(10, 10, 21, 20);
            var bb5 = BoundingBox.From(10, 10, 20, 21);

            bb1.Intersects(bb2).Should().BeTrue();
            bb1.Intersects(bb3).Should().BeTrue();
            bb1.Intersects(bb4).Should().BeTrue();
            bb1.Intersects(bb5).Should().BeTrue();
        }

        [Test]
        public void Intersection_Partial_Smaller()
        {
            var bb1 = BoundingBox.From(10, 10, 20, 20);
            var bb2 = BoundingBox.From(11, 10, 20, 20);
            var bb3 = BoundingBox.From(10, 11, 20, 20);
            var bb4 = BoundingBox.From(10, 10, 19, 20);
            var bb5 = BoundingBox.From(10, 10, 20, 19);

            bb1.Intersects(bb2).Should().BeTrue();
            bb1.Intersects(bb3).Should().BeTrue();
            bb1.Intersects(bb4).Should().BeTrue();
            bb1.Intersects(bb5).Should().BeTrue();
        }

        [Test]
        public void Intersection_ZeroSpacer()
        {
            var bb1 = BoundingBox.From(10, 10, 20, 20);
            var bb2 = BoundingBox.From(11, 10, 11, 20);
            var bb3 = BoundingBox.From(10, 11, 20, 11);

            bb1.Intersects(bb2).Should().BeTrue();
            bb1.Intersects(bb3).Should().BeTrue();

            bb2.Intersects(bb1).Should().BeTrue();
            bb3.Intersects(bb1).Should().BeTrue();
        }

        [Test]
        public void Bug()
        {
            var b1 = BoundingBox.From(120, 0, 0, 0);
            var b2 = BoundingBox.From(0, 0, 127, 127);

            b1.Intersects(b2).Should().BeTrue();
            b2.Intersects(b1).Should().BeTrue();
        }
    }
}
