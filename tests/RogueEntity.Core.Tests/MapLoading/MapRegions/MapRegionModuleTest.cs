using FluentAssertions;
using NUnit.Framework;
using RogueEntity.Core.MapLoading.MapRegions;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Tests.Fixtures;

namespace RogueEntity.Core.Tests.MapLoading.MapRegions
{
    [TestFixture]
    public class MapRegionModuleTest: BasicGameIntegrationTestBase
    {
        IMapRegionTrackerService<int> regionTracker;

        protected override void PrepareMapService(StaticTestMapService mapService)
        {
            base.PrepareMapService(mapService);
            mapService.AddMap(0, EmptyCorridor);

        }

        public override void SetUp()
        {
            base.SetUp();
            GameFixture.ServiceResolver.TryResolve(out IMapRegionTrackerService<int> regionLoader).Should().BeTrue();
            this.regionTracker = regionLoader;
        }

        [Test]
        public void ValidateMapRegionLoaderExists()
        {
            // condition already checked in setup.
        }

        [Test]
        public void RegionLoading()
        {
            regionTracker.IsRegionLoaded(0).Should().BeFalse();
            regionTracker.IsRegionLoaded(1).Should().BeFalse();

            regionTracker.RequestImmediateLoading(0);
            var buffer = regionTracker.QueryActiveRequests(MapRegionStatus.ImmediateLoadRequested);
            buffer.Count.Should().Be(1);
            buffer[0].RegionKey.Should().Be(0);
            buffer[0].MarkLoaded();

            regionTracker.IsRegionLoaded(0).Should().BeTrue();
            regionTracker.IsRegionLoaded(1).Should().BeFalse();
        }

        [Test]
        public void RegionLoadingStateResets()
        {
            RegionLoading();

            regionTracker.Initialize();
            
            regionTracker.IsRegionLoaded(0).Should().BeFalse();
            regionTracker.IsRegionLoaded(1).Should().BeFalse();
        }
    }
}
