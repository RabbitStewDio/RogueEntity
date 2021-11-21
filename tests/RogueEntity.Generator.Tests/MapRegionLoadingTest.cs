using FluentAssertions;
using NUnit.Framework;
using RogueEntity.Core.MapLoading.Builder;
using RogueEntity.Core.MapLoading.FlatLevelMaps;
using RogueEntity.Core.MapLoading.MapRegions;
using RogueEntity.Core.Meta.EntityKeys;
using RogueEntity.Core.Meta.ItemBuilder;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Tests.Fixtures;

namespace RogueEntity.Generator.Tests
{
    public class MapRegionLoadingTest : BasicGameIntegrationTestBase
    {
        public const string SecondLevelCorridor = @"
// 9x3; an empty room
 ### , ### , ### , ### , ### , ### , ### , ### , ###  
 ### ,  .  ,  .  ,  .  ,  .  ,  .  ,  .  ,  .  , ###
 ### ,  .  ,  .  ,  .  ,  .  ,  .  ,  .  ,  .  , ###
 ### ,  .  ,  .  ,  .  ,  <  ,  .  ,  .  ,  .  , ###
 ### ,  .  ,  .  ,  .  ,  .  ,  .  ,  .  ,  .  , ###
 ### , ### , ### , ### , ### , ### , ### , ### , ###  
";


        IMapRegionTrackerService<int> regionTracker;

        public override void SetUp()
        {
            base.SetUp();
            GameFixture.ServiceResolver.TryResolve(out IMapRegionTrackerService<int> regionLoader).Should().BeTrue();
            this.regionTracker = regionLoader;
        }

        protected override void ConfigureTestModule(TestModuleBase tm)
        {
            base.ConfigureTestModule(tm);
            tm.AddContentInitializer((mp, mi) =>
            {
                var ctx = mi.DeclareContentContext<ActorReference>();
                if (ctx.TryGetDefinedReferenceItem(StandardEntityDefinitions.Player.Id, out var playerDefinition))
                {
                    playerDefinition.AsBuilder(mp.ServiceResolver).WithChangeLevelCommand();
                }
            });
        }

        protected override void PrepareMapService(StaticTestMapService mapService)
        {
            base.PrepareMapService(mapService);
            mapService.AddMap(0, EmptyCorridor);
            mapService.AddMap(1, SecondLevelCorridor);
        }

        [Test]
        public void ValidateMapExistsWhenCreatingPlayer()
        {
            GameFixture.StartGame();
            GameFixture.AdvanceFrame();
            var playerEntity = GameFixture.PlayerData.Value.EntityId;
            GameFixture.ActorResolver.TryQueryData(playerEntity, out EntityGridPosition pos).Should().BeTrue();
            pos.Should().Be(EntityGridPosition.Of(TestMapLayers.Actors, 1, 1));
        }

        [Test]
        public void SubmittingInvalidLevelRequestsFails()
        {
            GameFixture.StartGame();
            GameFixture.AdvanceFrame();
            var playerEntity = GameFixture.PlayerData.Value.EntityId;
            GameFixture.ActorResolver.TryQueryData(playerEntity, out EntityGridPosition pos).Should().BeTrue();
            pos.Should().Be(EntityGridPosition.Of(TestMapLayers.Actors, 1, 1));

            this.GameFixture.CommandService.TrySubmit(playerEntity, new ChangeLevelCommand(100)).Should().BeFalse();
        }

        [Test]
        public void ValidateChangingLevels()
        {
            GameFixture.StartGame();
            GameFixture.AdvanceFrame();
            var playerEntity = GameFixture.PlayerData.Value.EntityId;
            GameFixture.ActorResolver.TryQueryData(playerEntity, out EntityGridPosition pos).Should().BeTrue();
            pos.Should().Be(EntityGridPosition.Of(TestMapLayers.Actors, 1, 1));

            this.GameFixture.CommandService.TrySubmit(playerEntity, new ChangeLevelCommand(1)).Should().BeTrue();

            // Step 1: Unloads the current level.
            GameFixture.AdvanceFrame();
            GameFixture.ActorResolver.TryQueryData(playerEntity, out pos).Should().BeTrue();
            pos.Should().Be(EntityGridPosition.Of(TestMapLayers.Actors, 4, 3, 1));
        }
    }
}
