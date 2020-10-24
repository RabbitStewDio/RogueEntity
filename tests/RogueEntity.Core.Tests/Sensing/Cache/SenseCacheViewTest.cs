using FluentAssertions;
using NUnit.Framework;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Sensing.Cache;
using RogueEntity.Core.Utils;

namespace RogueEntity.Core.Tests.Sensing.Cache
{
    public class SenseCacheViewTest
    {
        [Test]
        public void ValidateInitialState()
        {
            var sv = new SenseStateCacheView(64, 64, 2);
            sv.IsDirty(new Position(10, 10, 5, 1)).Should().Be(false);
            sv.IsDirty(new Position(10, 10, 5, 1), 5).Should().Be(false);
            sv.IsDirty(5, new Rectangle(1, 1, 50, 50)).Should().Be(false);
        }
        
        [Test]
        public void ValidateGloballyDirtyTests()
        {
            var sv = new SenseStateCacheView(64, 64, 2);

            sv.MarkGloballyDirty();
            sv.IsDirty(new Position(10, 10, 5, 1)).Should().Be(true);
            sv.IsDirty(new Position(10, 10, 5, 1), 5).Should().Be(true);
            sv.IsDirty(5, new Rectangle(1, 1, 50, 50)).Should().Be(true);
            
            sv.MarkClean();
            sv.IsDirty(new Position(10, 10, 5, 1)).Should().Be(false);
            sv.IsDirty(new Position(10, 10, 5, 1), 5).Should().Be(false);
            sv.IsDirty(5, new Rectangle(1, 1, 50, 50)).Should().Be(false);
        }

        [Test]
        public void ValidateDirtyTest()
        {
            var sv = new SenseStateCacheView(64, 64, 2);

            sv.MarkDirty(new Position(10, 10, 5, 1));
            sv.IsDirty(new Position(10, 10, 5, 1)).Should().Be(true);
            sv.IsDirty(new Position(10, 10, 5, 1), 5).Should().Be(true);
            sv.IsDirty(5, new Rectangle(1, 1, 50, 50)).Should().Be(true);
        }
    }
}