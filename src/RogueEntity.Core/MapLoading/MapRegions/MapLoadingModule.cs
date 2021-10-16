using EnTTSharp.Entities;
using EnTTSharp.Entities.Systems;
using RogueEntity.Api.GameLoops;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Modules;
using RogueEntity.Api.Modules.Attributes;
using RogueEntity.Api.Services;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Players;
using Serilog;

namespace RogueEntity.Core.MapLoading.MapRegions
{
    public class MapLoadingModule : ModuleBase
    {
        public static readonly string ModuleId = "Core.MapLoading";
        static readonly EntitySystemId MapLoadingCommandsSystemId = "System.Core.MapLoading.CommandSystem";
        static readonly EntitySystemId MapLoadingSystemId = "System.Core.MapLoading.LoaderSystem";
        static readonly EntitySystemId MapLoadingInitSystemId = "System.Core.MapLoading.InitializeOnce";
        static readonly EntitySystemId RegisterChangeLevelCommandComponentId = "Entities.Core.MapLoading.ChangeLevelCommands";

        public static readonly EntityRole ControlLevelLoadingRole = new EntityRole("Role.Core.MapLoading.ControlLevelLoading");

        public static readonly ILogger Logger = SLog.ForContext<MapLoadingModule>(); 
        
        public MapLoadingModule()
        {
            Id = ModuleId;
            Author = "RogueEntity.Core";
            Name = "RogueEntity Core Module - Map Loading";
            Description = "Provides base classes and behaviours for loading maps based on observer positions";
            IsFrameworkModule = true;
            
            DeclareDependency(ModuleDependency.Of(PlayerModule.ModuleId));
        }

        [EntityRoleInitializer("Role.Core.MapLoading.ControlLevelLoading")]
        protected void InitializeMapLoadingSystem<TItemId>(in ModuleEntityInitializationParameter<TItemId> initParameter,
                                                           IModuleInitializer initializer,
                                                           EntityRole r)
            where TItemId : IEntityKey
        {
            initializer.RegisterFinalizer(MapLoadingInitSystemId, 999_999, RegisterInitalizeMapLoader);

            
            var entityContext = initializer.DeclareEntityContext<TItemId>();
            entityContext.Register(RegisterChangeLevelCommandComponentId, -10_000, RegisterCommandEntities);
            entityContext.Register(MapLoadingCommandsSystemId, 45_500, RegisterMapLoaderCommandsSystem);
            entityContext.Register(MapLoadingSystemId, 48_500, RegisterMapLoaderSystem);
        }

        void RegisterInitalizeMapLoader(in ModuleInitializationParameter mip, IGameLoopSystemRegistration context)
        {
            var sr = mip.ServiceResolver;
            if (sr.TryResolve(out IMapRegionLoaderService loader))
            {
                loader.Initialize();
            }
        }

        void RegisterMapLoaderCommandsSystem<TEntityId>(in ModuleEntityInitializationParameter<TEntityId> initParameter,
                                                        IGameLoopSystemRegistration context,
                                                        EntityRegistry<TEntityId> registry)
            where TEntityId : IEntityKey
        {
            if (!TryGetOrResolveMapRegionSystem(initParameter.ServiceResolver, out var system))
            {
                Logger.Error(
                    "Unable to resolve a valid map region system. Either provide a custom implementation, " +
                    "provide a basic z-level keyed MapRegionLoaderService or override the map loading system {SystemId}", MapLoadingCommandsSystemId);
            }
            
            var handleChangeLevelSystem = registry.BuildSystem()
                                                  .WithoutContext()
                                                  .WithInputParameter<ChangeLevelCommand>()
                                                  .CreateSystem(system.RequestLoadLevelFromChangeLevelCommand);
            context.AddFixedStepHandlerSystem(handleChangeLevelSystem);

            var handleChangePositionSystem = registry.BuildSystem()
                                                     .WithoutContext()
                                                     .WithInputParameter<ChangeLevelPositionCommand>()
                                                     .CreateSystem(system.RequestLoadLevelFromChangePositionCommand);
            context.AddFixedStepHandlerSystem(handleChangePositionSystem);
        }

        void RegisterCommandEntities<TItemId>(in ModuleEntityInitializationParameter<TItemId> initParameter,
                                              EntityRegistry<TItemId> registry)
            where TItemId : IEntityKey
        {
            registry.RegisterNonConstructable<ChangeLevelCommand>();
            registry.RegisterNonConstructable<ChangeLevelPositionCommand>();
        }

        void RegisterMapLoaderSystem<TEntityId>(in ModuleEntityInitializationParameter<TEntityId> initParameter,
                                                IGameLoopSystemRegistration context,
                                                EntityRegistry<TEntityId> registry)
            where TEntityId : IEntityKey
        {
            if (!TryGetOrResolveMapRegionSystem(initParameter.ServiceResolver, out var system))
            {
                Logger.Error(
                    "Unable to resolve a valid map region system. Either provide a custom implementation, " +
                    "provide a basic z-level keyed MapRegionLoaderService or override the map loading system {SystemId}", MapLoadingSystemId);
            }
            
            context.AddFixedStepHandlers(system.LoadChunks);
            context.AddLateVariableStepHandlers(system.LoadChunks);
        }


        bool TryGetOrResolveMapRegionSystem(IServiceResolver sr, out IMapRegionSystem rs)
        {
            if (sr.TryResolve(out rs))
            {
                return true;
            }

            if (!sr.TryResolve(out IMapRegionLoaderService<int> loader))
            {
                return false;
            }

            if (sr.TryResolve(out IMapRegionLoaderSystemConfiguration conf))
            {
                rs = new BasicMapRegionSystem(loader, conf.MapLoadingTimeout);
            }
            else
            {
                rs = new BasicMapRegionSystem(loader);
            }

            return true;
        }
    }
}
