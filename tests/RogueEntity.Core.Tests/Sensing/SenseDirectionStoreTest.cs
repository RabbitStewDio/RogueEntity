using FluentAssertions;
using NUnit.Framework;
using RogueEntity.Core.Sensing.Common;

namespace RogueEntity.Core.Tests.Sensing
{
    public class SenseDirectionStoreTest
    {
        [Test]
        public void TestShit()
        {
            SenseDirectionStore.IsViewBlocked(SenseDirection.North, SenseDirection.South).Should().BeTrue();
        }
    }
}