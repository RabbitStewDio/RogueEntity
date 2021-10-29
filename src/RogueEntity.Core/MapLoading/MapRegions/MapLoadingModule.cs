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
        static readonly EntitySystemId RegisterChangeLevelRequestComponentId = "Entities.Core.MapLoading.ChangeLevelRequest";

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

        [ModuleInitializer]
        protected void InitializeGlobalMapLoadingSystem(in ModuleInitializationParameter initParameter,
                                                        IModuleInitializer moduleInitializer)
        {
            moduleInitializer.Register(MapLoadingInitSystemId, 0, RegisterInitializeMapLoader);
        }


        [EntityRoleInitializer("Role.Core.MapLoading.ControlLevelLoading")]
        protected void InitializeMapLoadingSystem<TItemId>(in ModuleEntityInitializationParameter<TItemId> initParameter,
                                                           IModuleInitializer initializer,
                                                           EntityRole r)
            where TItemId : IEntityKey
        {
            var entityContext = initializer.DeclareEntityContext<TItemId>();
            entityContext.Register(RegisterChangeLevelRequestComponentId, -10_000, RegisterCommandEntities);
            entityContext.Register(MapLoadingCommandsSystemId, 45_500, RegisterMapLoaderCommandsSystem);
            entityContext.Register(MapLoadingSystemId, 48_500, RegisterMapLoaderSystem);
        }

        void RegisterInitializeMapLoader(in ModuleInitializationParameter mip, IGameLoopSystemRegistration context)
        {
            var sr = mip.ServiceResolver;
            if (sr.TryResolve(out IMapRegionLoaderService service))
            {
                context.AddInitializationStepHandler(service.Initialize);
                context.AddDisposeStepHandler(service.Initialize);
            }

            // Ensure that the map loader is always created.
            if (!TryGetOrResolveMapRegionSystem(sr, out var system))
            {
                Logger.Error(
                    "Unable to resolve a valid map region system. Either provide a custom implementation, " +
                    "provide a basic z-level keyed MapRegionLoaderService or override the map loading system {SystemId}", MapLoadingInitSystemId);
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
                                                  .WithInputParameter<ChangeLevelRequest>()
                                                  .CreateSystem(system.RequestLoadLevelFromChangeLevelCommand);
            context.AddFixedStepHandlerSystem(handleChangeLevelSystem);

            var handleChangePositionSystem = registry.BuildSystem()
                                                     .WithoutContext()
                                                     .WithInputParameter<ChangeLevelPositionRequest>()
                                                     .CreateSystem(system.RequestLoadLevelFromChangePositionCommand);
            context.AddFixedStepHandlerSystem(handleChangePositionSystem);
        }

        void RegisterCommandEntities<TItemId>(in ModuleEntityInitializationParameter<TItemId> initParameter,
                                              EntityRegistry<TItemId> registry)
            where TItemId : IEntityKey
        {
            registry.RegisterNonConstructable<ChangeLevelRequest>();
            registry.RegisterNonConstructable<ChangeLevelPositionRequest>();
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
                return;
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

            // Unable to detect a map region system. That means we try to assemble one
            // from fragment services instead.
            if (!sr.TryResolve(out IMapRegionLoadingStrategy<int> strategy))
            {
                return false;
            }

            if (!sr.TryResolve(out IMapRegionLoaderService<int> loader))
            {
                loader = new BasicMapRegionLoaderService<int>();
                sr.Store(loader);
                sr.Store<IMapRegionLoaderService>(loader);
            }

            if (sr.TryResolve(out IMapRegionLoaderSystemConfiguration conf))
            {
                rs = new BasicMapRegionSystem(loader, strategy, conf.MapLoadingTimeout);
            }
            else
            {
                rs = new BasicMapRegionSystem(loader, strategy);
            }

            return true;
        }
    }
}
