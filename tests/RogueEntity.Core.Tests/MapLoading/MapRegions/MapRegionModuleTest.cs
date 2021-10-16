using FluentAssertions;
using NUnit.Framework;
using RogueEntity.Core.MapLoading.MapRegions;
using RogueEntity.Core.Meta.EntityKeys;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Tests.Fixtures;
using RogueEntity.Core.Tests.Players;

namespace RogueEntity.Core.Tests.MapLoading.MapRegions
{
    [TestFixture]
    public class MapRegionModuleTest: BasicGameIntegrationTestBase
    {
        IMapRegionLoaderService<int> regionLoader;

        protected override void PrepareMapService(StaticTestMapService mapService)
        {
            base.PrepareMapService(mapService);
            mapService.AddMap(0, EmptyCorridor);

        }

        public override void SetUp()
        {
            base.SetUp();
            GameFixture.ServiceResolver.TryResolve(out IMapRegionLoaderService<int> regionLoader).Should().BeTrue();
            this.regionLoader = regionLoader;

        }

        [Test]
        public void ValidateMapRegionLoaderExists()
        {
            // condition already checked in setup.
        }

        [Test]
        public void RegionLoading()
        {
            regionLoader.IsRegionLoaded(0).Should().BeFalse();
            regionLoader.IsRegionLoaded(1).Should().BeFalse();

            regionLoader.RequestImmediateLoading(0);
            int iterations = 10;
            while (regionLoader.PerformLoadNextChunk())
            {
                iterations.Should().NotBe(0);
                iterations -= 1;
            }

            regionLoader.IsRegionLoaded(0).Should().BeTrue();
            regionLoader.IsRegionLoaded(1).Should().BeFalse();
        }

        [Test]
        public void RegionLoadingStateResets()
        {
            RegionLoading();
            this.GameFixture.ServiceResolver.TryResolve(out IGridMapContext<ItemReference> gr).Should().BeTrue();
            gr.TryGetGridDataFor(TestMapLayers.Ground, out var data).Should().BeTrue();
            
            var x = (IGridMapContextInitializer<ItemReference>)data;

            x.ResetState();
            
            regionLoader.IsRegionLoaded(0).Should().BeFalse();
            regionLoader.IsRegionLoaded(1).Should().BeFalse();
        }
    }
}
