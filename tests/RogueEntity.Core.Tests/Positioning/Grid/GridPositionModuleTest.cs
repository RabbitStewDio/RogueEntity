using FluentAssertions;
using NUnit.Framework;
using RogueEntity.Core.Meta.EntityKeys;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Tests.Fixtures;

namespace RogueEntity.Core.Tests.Positioning.Grid
{
    [TestFixture]
    public class GridPositionModuleTest : BasicGameIntegrationTestBase
    {
        protected override void PrepareMapService(StaticTestMapService mapService)
        {
            base.PrepareMapService(mapService);
            mapService.AddMap(0, EmptyCorridor);
        }

        [Test]
        public void ValidateMapIsClearedOnResetState()
        {
            this.GameFixture.ServiceResolver.TryResolve(out IMapContext<ItemReference> gr).Should().BeTrue();
            gr.TryGetMapDataFor(TestMapLayers.Ground, out var data).Should().BeTrue();
            
            data.TryInsertItem(ItemReference.FromBulkItem(1, 1), EntityGridPosition.Of(TestMapLayers.Ground, 0, 0, 0)).Should().BeTrue();
            
            var x = (IMapContextInitializer<ItemReference>)gr;
            x.ResetState();

            data.At(0,0).Should().Be(ItemReference.Empty);
        }

        [Test]
        public void ValidateMapIsClearedOnGameStop()
        {
            this.GameFixture.ServiceResolver.TryResolve(out IMapContext<ItemReference> gr).Should().BeTrue();
            gr.TryGetMapDataFor(TestMapLayers.Ground, out var data).Should().BeTrue();

            this.GameFixture.StartGame();
            
            data.TryInsertItem(ItemReference.FromBulkItem(1, 1), EntityGridPosition.Of(TestMapLayers.Ground, 0, 0)).Should().BeTrue();

            this.GameFixture.Stop();
            
            data.At(0,0).Should().Be(ItemReference.Empty);
        }

        [Test]
        public void ValidateMapSendsDirtySignalsOnReset()
        {
            this.GameFixture.ServiceResolver.TryResolve(out IMapContext<ItemReference> gr).Should().BeTrue();
            gr.TryGetMapDataFor(TestMapLayers.Ground, out var data).Should().BeTrue();

            data.TryInsertItem(ItemReference.FromBulkItem(1, 1), EntityGridPosition.Of(TestMapLayers.Ground, 0, 0)).Should().BeTrue();
            
            var x = (IMapContextInitializer<ItemReference>)gr;
            
            bool eventFired = false;
            data.RegionDirty += (_, _) => eventFired = true;
            x.ResetState();

            eventFired.Should().BeTrue();

        }
    }
}
