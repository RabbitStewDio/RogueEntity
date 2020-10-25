using System;
using FluentAssertions;
using NUnit.Framework;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Sensing;
using RogueEntity.Core.Sensing.Cache;
using RogueEntity.Core.Sensing.Resistance.Maps;

namespace RogueEntity.Core.Tests.Sensing.Cache
{
    public class SenseCacheTest
    {
        [Test]
        public void GlobalOperations()
        {
            var ctx = new SenseMappingTestContext();
            var cache = new SenseStateCache(2, 0, 0, 4, 4);

            cache.ActivateGlobalCacheLayer(TestMapLayers.One);
            cache.ActivateGlobalCacheLayer(TestMapLayers.Two);
            cache.ActivateTrackedSenseSource(typeof(VisionSense));
            cache.ActivateTrackedSenseSource(typeof(TouchSense));
            cache.MarkClean();

            cache.TryGetSenseCache<SmellSense>(out _).Should().BeFalse();
            cache.TryGetSenseCache<TouchSense>(out var touchCacheView).Should().BeTrue();
            cache.TryGetSenseCache<VisionSense>(out var visionCacheView).Should().BeTrue();
            cache.TryGetGlobalSenseCache(out var globalCacheView).Should().BeTrue();
            
            cache.MarkAllSensesDirty(Position.Of(TestMapLayers.Indeterminate, 10, 10, 10));
            globalCacheView.IsDirty(Position.Of(TestMapLayers.Indeterminate, 10, 10, 10)).Should().BeTrue();
            globalCacheView.IsDirty(Position.Of(TestMapLayers.Indeterminate, 11, 11, 10)).Should().BeTrue();
            globalCacheView.IsDirty(Position.Of(TestMapLayers.Indeterminate, 11, 11, 11)).Should().BeFalse();
            globalCacheView.IsDirty(Position.Of(TestMapLayers.Indeterminate, 12, 12, 10)).Should().BeFalse();
            
            // Global cache state does not affect per sense state.
            visionCacheView.IsDirty(Position.Of(TestMapLayers.Indeterminate, 10, 10, 10)).Should().BeFalse();
            
            cache.MarkClean();
            
            globalCacheView.IsDirty(Position.Of(TestMapLayers.Indeterminate, 10, 10, 10)).Should().BeFalse();
        }
        
        [Test]
        public void LocalOperations()
        {
            var ctx = new SenseMappingTestContext();
            var cache = new SenseStateCache(2, 4, 4);

            cache.ActivateGlobalCacheLayer(TestMapLayers.One);
            cache.ActivateGlobalCacheLayer(TestMapLayers.Two);
            cache.ActivateTrackedSenseSource(typeof(VisionSense));
            cache.ActivateTrackedSenseSource(typeof(TouchSense));
            cache.MarkClean();

            cache.TryGetSenseCache<SmellSense>(out _).Should().BeFalse();
            cache.TryGetSenseCache<TouchSense>(out var touchCacheView).Should().BeTrue();
            cache.TryGetSenseCache<VisionSense>(out var visionCacheView).Should().BeTrue();
            cache.TryGetGlobalSenseCache(out var globalCacheView).Should().BeTrue();
            
            cache.MarkDirty<VisionSense>(Position.Of(TestMapLayers.Indeterminate, 10, 10, 10));
            visionCacheView.IsDirty(Position.Of(TestMapLayers.Indeterminate, 10, 10, 10)).Should().BeTrue();
            visionCacheView.IsDirty(Position.Of(TestMapLayers.Indeterminate, 11, 11, 10)).Should().BeTrue();
            visionCacheView.IsDirty(Position.Of(TestMapLayers.Indeterminate, 11, 11, 11)).Should().BeFalse();
            visionCacheView.IsDirty(Position.Of(TestMapLayers.Indeterminate, 12, 12, 10)).Should().BeFalse();
            
            // Global cache state does not affect per sense state.
            globalCacheView.IsDirty(Position.Of(TestMapLayers.Indeterminate, 10, 10, 10)).Should().BeFalse();
            touchCacheView.IsDirty(Position.Of(TestMapLayers.Indeterminate, 10, 10, 10)).Should().BeFalse();
            
            cache.MarkClean();
            
            globalCacheView.IsDirty(Position.Of(TestMapLayers.Indeterminate, 10, 10, 10)).Should().BeFalse();
            
            cache.MarkDirty<VisionSense>(Position.Of(TestMapLayers.Two, 1, 1, 1));
            visionCacheView.IsDirty(Position.Of(TestMapLayers.Indeterminate, 1, 1, 1)).Should().BeTrue("because layers are ignored for sense specific caches.");
            touchCacheView.IsDirty(Position.Of(TestMapLayers.Indeterminate, 1, 1, 1)).Should().BeFalse();
        }

        [Test]
        public void ValidateMapConnection()
        {
            var sps = new SensePropertiesSystem<SenseMappingTestContext>(4, 4);
            
            var ctx = new SenseMappingTestContext();
            var cache = new SenseStateCache(2, 4, 4);    
            var conn = new SensePropertiesConnectorSystem<SenseMappingTestContext>(sps, cache);
            
            cache.ActivateGlobalCacheLayer(TestMapLayers.One);
            cache.ActivateGlobalCacheLayer(TestMapLayers.Two);
            cache.ActivateTrackedSenseSource(typeof(VisionSense));
            cache.ActivateTrackedSenseSource(typeof(TouchSense));
            
            conn.Start(ctx);
            sps.OnPositionDirty(this, new PositionDirtyEventArgs(new Position(10, 10, 10, 1)));
            
            cache.TryGetGlobalSenseCache(out var globalCacheView).Should().BeTrue();
            globalCacheView.IsDirty(Position.Of(TestMapLayers.Indeterminate, 10, 10, 10)).Should().BeFalse("Because as long as the layer is not affecting sense properties, it should not dirty the cache");

            // force the creation of a sense properties layer.
            sps.GetOrCreate(10);
            
            sps.OnPositionDirty(this, new PositionDirtyEventArgs(new Position(10, 10, 10, 1)));
            globalCacheView.IsDirty(Position.Of(TestMapLayers.Indeterminate, 10, 10, 10)).Should().BeTrue("now that the layer is tracked, it should forward map change events");
            globalCacheView.IsDirty(Position.Of(TestMapLayers.Indeterminate, 11, 11, 10)).Should().BeTrue();
            globalCacheView.IsDirty(Position.Of(TestMapLayers.Indeterminate, 11, 11, 11)).Should().BeFalse();
            globalCacheView.IsDirty(Position.Of(TestMapLayers.Indeterminate, 12, 12, 10)).Should().BeFalse();
            
        }
    }
}