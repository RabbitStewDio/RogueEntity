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
            var buffer = regionLoader.QueryPendingRequests(MapRegionLoadingStatus.ImmediateLoadRequested);
            buffer.Count.Should().Be(1);
            buffer[0].RegionKey.Should().Be(0);
            buffer[0].MarkLoaded();

            regionLoader.IsRegionLoaded(0).Should().BeTrue();
            regionLoader.IsRegionLoaded(1).Should().BeFalse();
        }

        [Test]
        public void RegionLoadingStateResets()
        {
            RegionLoading();

            regionLoader.Initialize();
            
            regionLoader.IsRegionLoaded(0).Should().BeFalse();
            regionLoader.IsRegionLoaded(1).Should().BeFalse();
        }
    }
}
