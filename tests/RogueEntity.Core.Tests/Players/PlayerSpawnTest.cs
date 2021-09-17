using NUnit.Framework;
using RogueEntity.Api.Modules;
using RogueEntity.Core.MapLoading;
using RogueEntity.Core.Meta.EntityKeys;
using RogueEntity.Core.Movement.GridMovement;
using RogueEntity.Core.Players;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Tests.Fixtures;
using RogueEntity.Core.MapLoading.Builder;
using RogueEntity.Core.MapLoading.MapRegions;

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
 ### ,  $  ,  .  ,  .  ,  .  ,  .  ,  .  ,  .  , ###
 ### , ### , ### , ### , ### , ### , ### , ### , ###  
";

        StaticMapService mapService;
        
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
                mapService = new StaticMapService(mip.ServiceResolver.ResolveToReference<MapBuilder>(), 0);
                mip.ServiceResolver.Store<IPlayerSpawnInformationSource>(mapService);
                mip.ServiceResolver.Store<IMapAvailabilityService>(mapService);
                mip.ServiceResolver.Store<IMapRegionLoaderService<int>>(mapService);
            });
            
            tm.AddContentInitializer(StandardEntityDefinitions.DeclarePlayer);
            tm.AddContentInitializer(StandardEntityDefinitions.DeclareWalls);
            tm.AddContentInitializer(StandardEntityDefinitions.DeclareSpawnPoint);


            var gf = new GameFixture<ActorReference>();
            gf.AddExtraModule(tm);
            gf.InitializeSystems();
        }

        [Test]
        public void Test()
        {
            
        }
    }
}
