using FluentAssertions;
using NUnit.Framework;
using RogueEntity.Core.Meta.EntityKeys;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Tests.Fixtures;
using RogueEntity.Core.Utils.DataViews;

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
            this.GameFixture.ServiceResolver.TryResolve(out IGridMapContext<ItemReference> gr).Should().BeTrue();
            gr.TryGetGridDataFor(TestMapLayers.Ground, out var data).Should().BeTrue();

            data.TryGetWritableView(0, out var view, DataViewCreateMode.CreateMissing);
            view[0,0] = ItemReference.FromBulkItem(1, 1);
            
            var x = (IGridMapContextInitializer<ItemReference>)data;
            x.ResetState();

            view[0,0].Should().Be(ItemReference.Empty);
        }

        [Test]
        public void ValidateMapIsClearedOnGameStop()
        {
            this.GameFixture.ServiceResolver.TryResolve(out IGridMapContext<ItemReference> gr).Should().BeTrue();
            gr.TryGetGridDataFor(TestMapLayers.Ground, out var data).Should().BeTrue();

            this.GameFixture.StartGame();
            
            data.TryGetWritableView(0, out var view, DataViewCreateMode.CreateMissing);
            view[0,0] = ItemReference.FromBulkItem(1, 1);

            this.GameFixture.Stop();
            
            view[0,0].Should().Be(ItemReference.Empty);
        }

        [Test]
        public void ValidateMapSendsDirtySignalsOnReset()
        {
            this.GameFixture.ServiceResolver.TryResolve(out IGridMapContext<ItemReference> gr).Should().BeTrue();
            gr.TryGetGridDataFor(TestMapLayers.Ground, out var data).Should().BeTrue();

            data.TryGetWritableView(0, out var view, DataViewCreateMode.CreateMissing);
            view[0,0] = ItemReference.FromBulkItem(1, 1);
            
            var x = (IGridMapContextInitializer<ItemReference>)data;
            
            bool eventFired = false;
            data.ViewReset += (_, _) => eventFired = true;
            x.ResetState();

            eventFired.Should().BeTrue();

        }
    }
}
