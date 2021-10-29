using FluentAssertions;
using NUnit.Framework;
using RogueEntity.Core.MapLoading.Builder;
using RogueEntity.Core.Tests.Fixtures;

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
    }
}
