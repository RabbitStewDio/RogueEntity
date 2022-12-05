using FluentAssertions;
using NUnit.Framework;
using RogueEntity.Core.MapLoading.Builder;
using RogueEntity.Core.MapLoading.FlatLevelMaps;
using RogueEntity.Core.MapLoading.MapRegions;
using RogueEntity.Core.Meta.EntityKeys;
using RogueEntity.Core.Meta.ItemBuilder;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Tests.Fixtures;
using System;
using System.Diagnostics.CodeAnalysis;

namespace RogueEntity.Core.Tests.MapLoading.MapRegions
{
    [SuppressMessage("ReSharper", "NotNullOrRequiredMemberIsNotInitialized")]
    public class MapRegionUnloadingTest : BasicGameIntegrationTestBase
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
            Assert.NotNull(GameFixture.ServiceResolver);
            GameFixture.ServiceResolver.TryResolve(out IMapRegionTrackerService<int> regionLoader).Should().BeTrue();
            Assert.NotNull(regionLoader);
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

            tm.AddModuleInitializer((mp, _) =>
            {
                mp.ServiceResolver.Store(new MapRegionModuleConfiguration()
                {
                    MapEvictionTimer = TimeSpan.FromSeconds(1)
                });
                mp.ServiceResolver.Store(new FlatLevelMapConfiguration()
                {
                    ChangingLevelResetsMapData = true
                });
                Console.WriteLine("Registering flat-level eviction strategy");
                mp.ServiceResolver.Store<IMapRegionEvictionStrategy<int>>(new FlatLevelRegionEvictionStrategy(mp.ServiceResolver.ResolveToReference<MapBuilder>(),
                                                                                                              mp.ServiceResolver.Resolve<IMapRegionMetaDataService<int>>()));
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
            var placementService = GameFixture.ServiceResolver.Resolve<IItemPlacementService<ItemReference>>();

            GameFixture.StartGame();
            GameFixture.AdvanceFrame();
            var playerEntity = GameFixture.PlayerData.Value.EntityId;
            GameFixture.ActorResolver.TryQueryData(playerEntity, out EntityGridPosition pos).Should().BeTrue();
            pos.Should().Be(EntityGridPosition.Of(TestMapLayers.Actors, 1, 1));
            placementService.TryQueryItem(EntityGridPosition.Of(TestMapLayers.Ground, 1, 1), out var spawnPoint).Should().BeTrue();
            spawnPoint.IsReference.Should().BeTrue();

            this.GameFixture.CommandService.TrySubmit(playerEntity, new ChangeLevelCommand(1)).Should().BeTrue();

            // Step 1: Unloads the current level.
            GameFixture.AdvanceFrame();
            GameFixture.AdvanceFrame();
            GameFixture.ActorResolver.TryQueryData(playerEntity, out pos).Should().BeTrue();
            pos.Should().Be(EntityGridPosition.Of(TestMapLayers.Actors, 4, 3, 1));

            GameFixture.AdvanceTime(TimeSpan.FromSeconds(1));
            
            regionTracker.QueryRegionStatus(0).Should().Be(MapRegionStatus.Unloaded);
            placementService.TryQueryItem(EntityGridPosition.Of(TestMapLayers.Ground, 1, 1), out var spawnPointAfterUnload).Should().BeTrue();
            spawnPointAfterUnload.Should().Be(ItemReference.Empty);
        }
        
        [Test]
        public void ValidateReloadingLevel()
        {
            var serviceResolver = GameFixture.ServiceResolver ?? throw new ArgumentException();
            var placementService = serviceResolver.Resolve<IItemPlacementService<ItemReference>>();

            GameFixture.StartGame();
            GameFixture.AdvanceFrame();
            var playerEntity = GameFixture.PlayerData.Value.EntityId;
            GameFixture.ActorResolver.TryQueryData(playerEntity, out EntityGridPosition pos).Should().BeTrue();
            pos.Should().Be(EntityGridPosition.Of(TestMapLayers.Actors, 1, 1));
            placementService.TryQueryItem(EntityGridPosition.Of(TestMapLayers.Ground, 1, 1), out var spawnPoint).Should().BeTrue();
            spawnPoint.IsReference.Should().BeTrue();

            this.GameFixture.CommandService!.TrySubmit(playerEntity, new ChangeLevelCommand(0)).Should().BeTrue();

            // Step 1: Unloads the current level.
            GameFixture.AdvanceFrame();
            // Step 2: Loads the just unloaded level.
            GameFixture.AdvanceFrame();
            GameFixture.ActorResolver.TryQueryData(playerEntity, out pos).Should().BeTrue();
            pos.Should().Be(EntityGridPosition.Of(TestMapLayers.Actors, 1, 1));

            regionTracker.QueryRegionStatus(0).Should().Be(MapRegionStatus.Loaded);
            placementService.TryQueryItem(EntityGridPosition.Of(TestMapLayers.Ground, 1, 1), out var spawnPointAfterUnload).Should().BeTrue();
            spawnPointAfterUnload.IsReference.Should().Be(true);

        }
    }
}
