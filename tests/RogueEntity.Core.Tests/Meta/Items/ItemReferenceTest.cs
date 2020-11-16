using FluentAssertions;
using NUnit.Framework;
using RogueEntity.Core.Meta.EntityKeys;

namespace RogueEntity.Core.Tests.Meta.Items
{
    public class ItemReferenceTest
    {
        [Test]
        public void TestReferenceItemConstruction()
        {
            var ir = ItemReference.FromReferencedItem(2, 1);
            ir.IsEmpty.Should().BeFalse();
            ir.IsReference.Should().BeTrue();
            ir.Key.Should().Be(1);
            ir.Age.Should().Be(2);
        }
    }
}