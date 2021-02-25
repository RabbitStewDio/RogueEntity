using FluentAssertions;
using NUnit.Framework;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Caching;
using RogueEntity.Core.Utils;

namespace RogueEntity.Core.Tests.Sensing.Cache
{
    public class SenseCacheViewTest
    {
        [Test]
        public void ValidateInitialState()
        {
            var sv = new GridCacheStateView(64, 64, 2);
            sv.IsDirty(Position.Of(TestMapLayers.One,10, 10, 5)).Should().Be(false);
            sv.IsDirty(Position.Of(TestMapLayers.One,10, 10, 5), 5).Should().Be(false);
            sv.IsDirty(5, new Rectangle(1, 1, 50, 50)).Should().Be(false);
        }
        
        [Test]
        public void ValidateGloballyDirtyTests()
        {
            var sv = new GridCacheStateView(64, 64, 2);

            sv.MarkGloballyDirty();
            sv.IsDirty(Position.Of(TestMapLayers.One,10, 10, 5)).Should().Be(true);
            sv.IsDirty(Position.Of(TestMapLayers.One,10, 10, 5), 5).Should().Be(true);
            sv.IsDirty(5, new Rectangle(1, 1, 50, 50)).Should().Be(true);
            
            sv.MarkClean();
            sv.IsDirty(Position.Of(TestMapLayers.One,10, 10, 5)).Should().Be(false);
            sv.IsDirty(Position.Of(TestMapLayers.One,10, 10, 5), 5).Should().Be(false);
            sv.IsDirty(5, new Rectangle(1, 1, 50, 50)).Should().Be(false);
        }

        [Test]
        public void ValidateDirtyTest()
        {
            var sv = new GridCacheStateView(64, 64, 2);

            sv.MarkDirty(Position.Of(TestMapLayers.One, 10, 10, 5));
            sv.IsDirty(Position.Of(TestMapLayers.One, 10, 10, 5)).Should().Be(true);
            sv.IsDirty(Position.Of(TestMapLayers.One, 10, 10, 5), 5).Should().Be(true);
            sv.IsDirty(5, new Rectangle(1, 1, 50, 50)).Should().Be(true);
        }
    }
}