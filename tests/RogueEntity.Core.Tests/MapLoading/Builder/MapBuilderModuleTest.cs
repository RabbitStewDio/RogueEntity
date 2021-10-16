using FluentAssertions;
using NUnit.Framework;
using RogueEntity.Core.MapLoading.Builder;
using RogueEntity.Core.Meta.EntityKeys;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Tests.Fixtures;
using RogueEntity.Core.Tests.Players;
using RogueEntity.Core.Utils.DataViews;

namespace RogueEntity.Core.Tests.MapLoading.Builder
{
    [TestFixture]
    public class MapBuilderModuleTest: BasicGameIntegrationTestBase
    {
        protected override void PrepareMapService(StaticTestMapService mapService)
        {
            base.PrepareMapService(mapService);
            mapService.AddMap(0, EmptyCorridor);

        }

        [Test]
        public void ValidateMapBuilderCreated()
        {
            this.GameFixture.ServiceResolver.TryResolve(out MapBuilder b).Should().BeTrue();
            b.Layers.Should().ContainInOrder(TestMapLayers.Ground, TestMapLayers.Actors);
        }

        [Test]
        public void MapBuilderReactsToGridMapChanges()
        {
            this.GameFixture.ServiceResolver.TryResolve(out MapBuilder b).Should().BeTrue();
            this.GameFixture.ServiceResolver.TryResolve(out IGridMapContext<ItemReference> gr).Should().BeTrue();
            gr.TryGetGridDataFor(TestMapLayers.Ground, out var data).Should().BeTrue();

            data.TryGetWritableView(0, out var view, DataViewCreateMode.CreateMissing);
            view[0,0] = ItemReference.FromBulkItem(1, 1);
            
            var x = (IGridMapContextInitializer<ItemReference>)data;
            
            bool eventFired = false;
            b.MapLayerDirty += (s, e) => eventFired = true;
            x.ResetState();

            eventFired.Should().BeTrue();

        }
    }
}
