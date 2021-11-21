using NUnit.Framework;
using RogueEntity.Api.Modules;
using RogueEntity.Core.MapLoading.Builder;
using RogueEntity.Core.MapLoading.FlatLevelMaps;
using RogueEntity.Core.MapLoading.MapRegions;
using RogueEntity.Core.Meta.EntityKeys;
using RogueEntity.Core.Movement.GridMovement;
using RogueEntity.Core.Players;
using RogueEntity.Core.Positioning;
using RogueEntity.Generator;

namespace RogueEntity.Core.Tests.Fixtures
{
    public abstract class BasicGameIntegrationTestBase
    {
        
        public const string EmptyCorridor = @"
// 9x3; an empty room
 ### , ### , ### , ### , ### , ### , ### , ### , ###  
 ### ,  <  ,  .  ,  .  ,  .  ,  .  ,  .  ,  .  , ###
 ### , ### , ### , ### , ### , ### , ### , ### , ###  
";

        protected GameFixture<ActorReference> GameFixture;

        [SetUp]
        public virtual void SetUp()
        {
            var tm = new TestModuleBase(GetType().Name);
            tm.AddModuleInitializer((mip, _) =>
            {
                var mapService = new StaticTestMapService(mip.ServiceResolver.ResolveToReference<MapBuilder>(), 0);
                PrepareMapService(mapService);
                
                mip.ServiceResolver.Store(mapService);
                mip.ServiceResolver.Store<IFlatLevelPlayerSpawnInformationSource>(mapService);
                mip.ServiceResolver.Store<IMapRegionLoadingStrategy<int>>(mapService);
                mip.ServiceResolver.Store<IMapRegionMetaDataService<int>>(mapService);
            });
            
            ConfigureTestModule(tm);

            GameFixture = new GameFixture<ActorReference>();
            GameFixture.AddExtraModule(tm);
            GameFixture.InitializeSystems();
        }

        protected virtual void ConfigureTestModule(TestModuleBase tm)
        {
            tm.DeclareDependencies(ModuleDependency.Of(PositionModule.ModuleId),
                                   ModuleDependency.Of(PlayerModule.ModuleId),
                                   ModuleDependency.Of(GridMovementModule.ModuleId),
                                   ModuleDependency.Of(MapBuilderModule.ModuleId),
                                   ModuleDependency.Of(MapLoadingModule.ModuleId));
            tm.AddContentInitializer(StandardEntityDefinitions.DeclarePlayer);
            tm.AddContentInitializer(StandardEntityDefinitions.DeclareWalls);
            tm.AddContentInitializer(StandardEntityDefinitions.DeclareSpawnPoint);
        }

        protected virtual void PrepareMapService(StaticTestMapService mapService)
        {
            mapService.AddMapToken("###", StandardEntityDefinitions.Wall.Id);
            mapService.AddMapToken(".", StandardEntityDefinitions.EmptyFloor.Id);
            mapService.AddMapToken("<", StandardEntityDefinitions.SpawnPointFloor.Id);
        }

        [TearDown]
        public void TearDown()
        {
            GameFixture.Stop();
        }

    }
}
