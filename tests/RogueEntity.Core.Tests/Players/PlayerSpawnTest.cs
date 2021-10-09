using FluentAssertions;
using NUnit.Framework;
using RogueEntity.Api.Modules;
using RogueEntity.Core.Meta.EntityKeys;
using RogueEntity.Core.Movement.GridMovement;
using RogueEntity.Core.Players;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Tests.Fixtures;
using RogueEntity.Core.MapLoading.Builder;
using RogueEntity.Core.MapLoading.MapRegions;
using RogueEntity.Core.Runtime;

namespace RogueEntity.Core.Tests.Players
{
    /// <summary>
    ///   Validates that players spawn at predefined spawn points and that
    ///   removing a player removes the entity and all remnants from the map
    ///   as well.
    /// </summary>
    [TestFixture]
    public class PlayerSpawnTest
    {

        const string EmptyRoom = @"
// 9x3; an empty room
 ### , ### , ### , ### , ### , ### , ### , ### , ###  
 ### ,  <  ,  .  ,  .  ,  .  ,  .  ,  .  ,  .  , ###
 ### , ### , ### , ### , ### , ### , ### , ### , ###  
";

        StaticTestMapService mapService;
        GameFixture<ActorReference> gf;

        [SetUp]
        public void SetUp()
        {
            
            
            var tm = new TestModuleBase(nameof(PlayerSpawnTest));
            tm.DeclareDependencies(ModuleDependency.Of(PositionModule.ModuleId),
                                   ModuleDependency.Of(PlayerModule.ModuleId),
                                   ModuleDependency.Of(GridMovementModule.ModuleId),
                                   ModuleDependency.Of(MapBuilderModule.ModuleId),
                                   ModuleDependency.Of(MapLoadingModule.ModuleId));
            
            tm.AddLateModuleInitializer((mip, _) =>
            {
                mapService = new StaticTestMapService(mip.ServiceResolver.ResolveToReference<MapBuilder>(), 0);
                mapService.AddMap(0, EmptyRoom);
                mapService.AddMapToken("###", StandardEntityDefinitions.Wall.Id);
                mapService.AddMapToken(".", StandardEntityDefinitions.EmptyFloor.Id);
                mapService.AddMapToken("<", StandardEntityDefinitions.SpawnPointFloor.Id);
                
                mip.ServiceResolver.Store<IPlayerSpawnInformationSource>(mapService);
                mip.ServiceResolver.Store<IMapAvailabilityService>(mapService);
                mip.ServiceResolver.Store<IMapRegionLoaderService<int>>(mapService);
            });
            
            tm.AddContentInitializer(StandardEntityDefinitions.DeclarePlayer);
            tm.AddContentInitializer(StandardEntityDefinitions.DeclareWalls);
            tm.AddContentInitializer(StandardEntityDefinitions.DeclareSpawnPoint);


            gf = new GameFixture<ActorReference>();
            gf.AddExtraModule(tm);
            gf.InitializeSystems();
        }

        [TearDown]
        public void TearDown()
        {
            gf.Stop();
        }
        
        [Test]
        public void InitialStatusIsInitialized()
        {
            gf.Status.Should().Be(GameStatus.Initialized);
        }
        
        [Test]
        public void AddPlayerToRun()
        {
            gf.StartGame();
            gf.Status.Should().Be(GameStatus.Running);
            gf.PlayerData.HasValue.Should().BeTrue();
            gf.PlayerService.TryQueryPrimaryObserver(gf.PlayerData.Value.Tag, out _).Should().BeFalse("because player observers are only collected during the next update");
            
            gf.Update(gf.Time.FixedTimeStep);
            
            gf.PlayerService.TryQueryPrimaryObserver(gf.PlayerData.Value.Tag, out var obs).Should().BeTrue("because the default player is its own observer when on map");
            
            obs.Player.Should().Be(gf.PlayerData.Value.Tag);
            obs.Primary.Should().BeFalse("Because primary designations must be managed explicitly.");
        }

        [Test]
        public void RemoveExistingPlayer()
        {
            gf.StartGame();
            gf.Update(gf.Time.CurrentTime + gf.Time.FixedTimeStep);
            gf.PlayerService.TryQueryPrimaryObserver(gf.PlayerData.Value.Tag, out _).Should().BeTrue("because the default player is its own observer when on map");
            gf.Update(gf.Time.CurrentTime + gf.Time.FixedTimeStep);
            gf.PlayerManager.TryDeactivatePlayer(gf.PlayerData.Value.Tag.Id).Should().BeTrue();
            gf.Update(gf.Time.CurrentTime + gf.Time.FixedTimeStep);
            gf.PlayerData.HasValue.Should().BeFalse();
        }

        [Test]
        public void ReactivateExistingPlayer()
        {
            gf.StartGame();
            gf.Update(gf.Time.CurrentTime + gf.Time.FixedTimeStep);
            gf.PlayerService.TryQueryPrimaryObserver(gf.PlayerData.Value.Tag, out _).Should().BeTrue("because the default player is its own observer when on map");
            gf.Update(gf.Time.CurrentTime + gf.Time.FixedTimeStep);
            gf.PlayerManager.TryDeactivatePlayer(gf.PlayerData.Value.Tag.Id).Should().BeTrue();
            gf.Update(gf.Time.CurrentTime + gf.Time.FixedTimeStep);
            gf.PlayerData.HasValue.Should().BeFalse();
            
            gf.ReActivatePlayer();
            
            gf.Update(gf.Time.CurrentTime + gf.Time.FixedTimeStep);
            gf.PlayerService.TryQueryPrimaryObserver(gf.PlayerData.Value.Tag, out _).Should().BeTrue("because the default player is its own observer when on map");


        }
    }
}
