using EnTTSharp.Entities;
using EnTTSharp.Entities.Systems;
using RogueEntity.Api.GameLoops;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Modules;
using RogueEntity.Api.Modules.Attributes;
using RogueEntity.Api.Services;
using RogueEntity.Api.Time;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Infrastructure.Randomness;
using RogueEntity.Core.Inputs.Commands;
using RogueEntity.Core.MapLoading.MapRegions;
using RogueEntity.Core.MapLoading.PlayerSpawning;
using RogueEntity.Core.Players;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.SpatialQueries;
using RogueEntity.Generator;
using Serilog;
using System;

namespace RogueEntity.Core.MapLoading.FlatLevelMaps
{
    [Module]
    public class FlatLevelMapModule : ModuleBase
    {
        public static readonly string ModuleId = "Core.MapLoading.FlatLevel";

        static readonly EntitySystemId SpawnActorsSystemId = "System.Core.MapLoading.FlatLevel.SpawnNewActors";
        static readonly EntitySystemId PlaceActorsSystemId = "System.Core.MapLoading.FlatLevel.PlaceActors";
        static readonly EntitySystemId MapLoadingRequestHandlerSystemId = "System.Core.MapLoading.FlatLevel.RequestHandlers";
        static readonly EntitySystemId MapLoadingSystemId = "System.Core.MapLoading.FlatLevel.LoaderSystem";
        static readonly EntitySystemId MapEvictionSystemId = "System.Core.MapLoading.FlatLevel.EvictionSystem";
        static readonly EntitySystemId MapLoadingInitSystemId = "System.Core.MapLoading.FlatLevel.InitializeOnce";
        static readonly EntitySystemId MapAutoEvictionSystemId = "System.Core.MapLoading.FlatLevel.AutoEvictionSystem";
        static readonly EntitySystemId RegisterMapChangeRequestComponentId = "Entities.Core.MapLoading.FlatLevel.ChangeLevelRequest";
        static readonly EntitySystemId RegisterChangeLevelCommandComponentId = "Entities.Core.MapLoading.FlatLevel.ChangeLevelCommand";
        static readonly EntitySystemId RegisterCommandSystemId = "Systems.Core.MapLoading.FlatLevel.RegisterCommandSystem";

        static readonly EntityRole ChangeLevelCommandRole = CommandRoles.CreateRoleFor(CommandType.Of<ChangeLevelCommand>());

        static readonly ILogger Logger = SLog.ForContext<FlatLevelMapModule>();

        public FlatLevelMapModule()
        {
            Id = ModuleId;
            Author = "RogueEntity.Core";
            Name = "RogueEntity Core Module - Flat Level Map Support";
            Description = "Provides default services for flat maps organized along a depth layer.";
            IsFrameworkModule = true;

            this.RequireRole(ChangeLevelCommandRole)
                .WithRequiredRole(PositionModule.PositionedRole)
                .WithRequiredRole(MapLoadingModule.ControlLevelLoadingRole);

            DeclareDependencies(ModuleDependency.Of(PlayerSpawningModule.ModuleId),
                                ModuleDependency.Of(MapLoadingModule.ModuleId));
        }


        [EntityRelationInitializer("Relation.Core.PlayerSpawning.PlayerToSpawnPoint")]
        protected void InitializeSpawnPlayerRelation<TActorId, TItemId>(in ModuleEntityInitializationParameter<TActorId> initParameter,
                                                                        IModuleInitializer initializer,
                                                                        EntityRelation r)
            where TActorId : IEntityKey
            where TItemId : IEntityKey
        {
            var entityContext = initializer.DeclareEntityContext<TActorId>();
            entityContext.Register(SpawnActorsSystemId, 45_000, RegisterSpawnNewPlayers);
            entityContext.Register(PlaceActorsSystemId, 49_500, RegisterPlacePlayersAfterLevelLoading<TActorId, TItemId>);
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
            entityContext.Register(RegisterMapChangeRequestComponentId, -10_000, RegisterRequestEntities);
            entityContext.Register(MapLoadingRequestHandlerSystemId, 45_500, RegisterMapLoadingRequestHandlerSystem);
            entityContext.Register(MapEvictionSystemId, 48_000, RegisterMapEvictionSystem);
            entityContext.Register(MapLoadingSystemId, 48_500, RegisterMapLoaderSystem);
        }

        [EntityRoleInitializer("Role.Core.Player")]
        protected void InitializeAutomaticMapEviction<TActorId>(in ModuleEntityInitializationParameter<TActorId> initParameter,
                                                                IModuleInitializer initializer,
                                                                EntityRole r)
            where TActorId : IEntityKey
        {
            var entityContext = initializer.DeclareEntityContext<TActorId>();
            entityContext.Register(MapAutoEvictionSystemId, 47_500, RegisterMapAutoEvictionSystem);
        }

        [EntityRoleInitializer("Role.Core.Inputs.Commands.Executor[RogueEntity.Core.MapLoading.FlatLevelMaps.ChangeLevelCommand]")]
        protected void InitializeGridPositioned<TActorId>(in ModuleEntityInitializationParameter<TActorId> initParameter,
                                                          IModuleInitializer initializer,
                                                          EntityRole role)
            where TActorId : IEntityKey
        {
            var entityContext = initializer.DeclareEntityContext<TActorId>();
            entityContext.Register(RegisterChangeLevelCommandComponentId, 0, RegisterEntities);
            entityContext.Register(RegisterCommandSystemId, 21_000, RegisterCommandSystem);
        }

        void RegisterCommandSystem<TActorId>(in ModuleEntityInitializationParameter<TActorId> initParameter, IGameLoopSystemRegistration context, EntityRegistry<TActorId> registry)
            where TActorId : IEntityKey
        {
            var sr = initParameter.ServiceResolver;
            var config = sr.ResolveConfiguration<FlatLevelMapConfiguration>();
            var sys = new ChangeLevelCommandSystem<TActorId>(sr.Resolve<IMapRegionMetaDataService<int>>(),
                                                             sr.Resolve<IMapRegionTrackerService<int>>(),
                                                             sr.Resolve<IItemResolver<TActorId>>(),
                                                             sr.Resolve<IItemPlacementService<TActorId>>(),
                                                             config);

            var system = registry.BuildSystem()
                                 .WithoutContext()
                                 .WithInputParameter<ChangeLevelCommand>()
                                 .WithOutputParameter<CommandInProgress>()
                                 .CreateSystem(sys.ProcessCommand);
            context.AddFixedStepHandlerSystem(system);
        }

        void RegisterEntities<TActorId>(in ModuleEntityInitializationParameter<TActorId> initParameter, EntityRegistry<TActorId> registry)
            where TActorId : IEntityKey
        {
            registry.RegisterNonConstructable<ChangeLevelCommand>();
            registry.RegisterNonConstructable<ChangeLevelCommandState>();
        }

        void RegisterMapAutoEvictionSystem<TActorId>(in ModuleEntityInitializationParameter<TActorId> initParameter,
                                                     IGameLoopSystemRegistration context,
                                                     EntityRegistry<TActorId> registry)
            where TActorId : IEntityKey
        {
            var sr = initParameter.ServiceResolver;
            var config = GetOrCreateMapRegionConfig(sr);
            if (config.MapEvictionTimer < TimeSpan.Zero)
            {
                return;
            }

            var service = GetOrCreateAutoEvictionService<TActorId>(sr);
            context.AddFixedStepHandlers(service.ProcessLevels);
        }

        void RegisterSpawnNewPlayers<TActorId>(in ModuleEntityInitializationParameter<TActorId> initParameter,
                                               IGameLoopSystemRegistration context,
                                               EntityRegistry<TActorId> registry)
            where TActorId : IEntityKey
        {
            var system = GetOrCreateSpawnRequestHandlerSystem<TActorId>(initParameter.ServiceResolver);

            var handleNewPlayersSystem = registry.BuildSystem()
                                                 .WithoutContext()
                                                 .WithInputParameter<PlayerTag>()
                                                 .WithInputParameter<NewPlayerSpawnRequest>()
                                                 .CreateSystem(system.RequestLoadLevelFromNewPlayer);
            context.AddFixedStepHandlerSystem(handleNewPlayersSystem);
        }


        void RegisterPlacePlayersAfterLevelLoading<TActorId, TItemId>(in ModuleEntityInitializationParameter<TActorId> initParameter,
                                                                      IGameLoopSystemRegistration context,
                                                                      EntityRegistry<TActorId> registry)
            where TActorId : IEntityKey
            where TItemId : IEntityKey
        {
            var system = GetOrCreateSpawnSystem<TActorId, TItemId>(initParameter.ServiceResolver);

            var spawnAction = registry.BuildSystem()
                                      .WithoutContext()
                                      .WithInputParameter<PlayerTag>()
                                      .WithInputParameter<ChangeLevelRequest>()
                                      .CreateSystem(system.SpawnPlayer);
            context.AddFixedStepHandlerSystem(spawnAction);

            var placeAction = registry.BuildSystem()
                                      .WithoutContext()
                                      .WithInputParameter<PlayerTag>()
                                      .WithInputParameter<ChangeLevelPositionRequest>()
                                      .CreateSystem(system.PlacePlayerAfterLevelChange);
            context.AddFixedStepHandlerSystem(placeAction);
        }


        void RegisterInitializeMapLoader(in ModuleInitializationParameter mip, IGameLoopSystemRegistration context)
        {
            var sr = mip.ServiceResolver;
            if (sr.TryResolve(out IMapRegionTrackerService service))
            {
                context.AddInitializationStepHandler(service.Initialize);
                context.AddDisposeStepHandler(service.Initialize);
            }

            // Ensure that the map loader is always created.
            if (!TryGetOrResolveMapRegionLoaderSystem(sr, out _))
            {
                Logger.Error(
                    "Unable to resolve a valid map region system. Either provide a IFlatLevelRegionLoaderSystem implementation, " +
                    "provide a basic z-level keyed IMapRegionLoadingStrategy<int> implementation or override the map loading system {SystemId}", MapLoadingSystemId);
            }
        }

        void RegisterMapLoadingRequestHandlerSystem<TEntityId>(in ModuleEntityInitializationParameter<TEntityId> initParameter,
                                                               IGameLoopSystemRegistration context,
                                                               EntityRegistry<TEntityId> registry)
            where TEntityId : IEntityKey
        {
            var system = GetOrCreateMapRegionRequestHandlerSystem(initParameter.ServiceResolver);
            var handleChangeLevelSystem = registry.BuildSystem()
                                                  .WithoutContext()
                                                  .WithInputParameter<ChangeLevelRequest>()
                                                  .CreateSystem(system.RequestLoadLevelFromChangeLevelCommand);
            context.AddFixedStepHandlerSystem(handleChangeLevelSystem);

            var handleEvictLevelSystem = registry.BuildSystem()
                                                  .WithoutContext()
                                                  .WithInputParameter<EvictLevelRequest>()
                                                  .CreateSystem(system.RequestEvictLevelFromRequest);
            context.AddFixedStepHandlerSystem(handleEvictLevelSystem);

            var handleChangePositionSystem = registry.BuildSystem()
                                                     .WithoutContext()
                                                     .WithInputParameter<ChangeLevelPositionRequest>()
                                                     .CreateSystem(system.RequestLoadLevelFromChangePositionCommand);
            context.AddFixedStepHandlerSystem(handleChangePositionSystem);
        }

        void RegisterRequestEntities<TItemId>(in ModuleEntityInitializationParameter<TItemId> initParameter,
                                              EntityRegistry<TItemId> registry)
            where TItemId : IEntityKey
        {
            registry.RegisterNonConstructable<EvictLevelRequest>();
            registry.RegisterNonConstructable<ChangeLevelRequest>();
            registry.RegisterNonConstructable<ChangeLevelPositionRequest>();
        }

        void RegisterMapLoaderSystem<TEntityId>(in ModuleEntityInitializationParameter<TEntityId> initParameter,
                                                IGameLoopSystemRegistration context,
                                                EntityRegistry<TEntityId> registry)
            where TEntityId : IEntityKey
        {
            if (!TryGetOrResolveMapRegionLoaderSystem(initParameter.ServiceResolver, out var system))
            {
                Logger.Error(
                    "Unable to resolve a valid map region system. Either provide a IFlatLevelRegionLoaderSystem implementation, " +
                    "provide a basic z-level keyed IMapRegionLoadingStrategy<int> implementation or override the map loading system {SystemId}", MapLoadingSystemId);
                return;
            }

            context.AddFixedStepHandlers(system.LoadChunks);
            context.AddLateVariableStepHandlers(system.LoadChunks);
        }

        void RegisterMapEvictionSystem<TEntityId>(in ModuleEntityInitializationParameter<TEntityId> initParameter,
                                                  IGameLoopSystemRegistration context,
                                                  EntityRegistry<TEntityId> registry)
            where TEntityId : IEntityKey
        {
            if (!TryGetOrResolveMapRegionEvictionSystem(initParameter.ServiceResolver, out var system))
            {
                Logger.Information(
                    "Unable to resolve a valid map eviction service. Loaded map chunks will remain active for the lifetime of the game. " +
                    "To dynamically unload map chunks, either provide a custom implementation of an IMapRegionEvictionSystem, " +
                    "provide a basic z-level keyed IMapRegionEvictionStrategy<int> implementation or override the map loading system with id {SystemId}", MapEvictionSystemId);
                return;
            }

            context.AddFixedStepHandlers(system.EvictChunks);
            context.AddLateVariableStepHandlers(system.EvictChunks);
        }


        FlatLevelPlayerSpawnSystem<TActorId, TItemId> GetOrCreateSpawnSystem<TActorId, TItemId>(IServiceResolver serviceResolver)
            where TActorId : IEntityKey
            where TItemId : IEntityKey
        {
            if (serviceResolver.TryResolve(out FlatLevelPlayerSpawnSystem<TActorId, TItemId> spawnSystem))
            {
                return spawnSystem;
            }

            var tracker = GetOrCreateMapRegionTrackerService(serviceResolver);
            var system = new FlatLevelPlayerSpawnSystem<TActorId, TItemId>(serviceResolver.Resolve<IItemPlacementService<TActorId>>(),
                                                                           serviceResolver.Resolve<IItemPlacementLocationService<TActorId>>(),
                                                                           serviceResolver.Resolve<IItemResolver<TActorId>>(),
                                                                           tracker,
                                                                           serviceResolver.Resolve<IMapRegionMetaDataService<int>>(),
                                                                           serviceResolver.Resolve<ISpatialQueryLookup>(),
                                                                           serviceResolver.ResolveOptional<IEntityRandomGeneratorSource>());
            serviceResolver.Store(system);
            return system;
        }


        FlatLevelPlayerSpawnRequestHandlerSystem<TActorId> GetOrCreateSpawnRequestHandlerSystem<TActorId>(IServiceResolver serviceResolver)
            where TActorId : IEntityKey
        {
            if (serviceResolver.TryResolve(out FlatLevelPlayerSpawnRequestHandlerSystem<TActorId> spawnSystem))
            {
                return spawnSystem;
            }

            var system = new FlatLevelPlayerSpawnRequestHandlerSystem<TActorId>(serviceResolver.Resolve<IFlatLevelPlayerSpawnInformationSource>());
            serviceResolver.Store(system);
            return system;
        }

        bool TryGetOrResolveMapRegionLoaderSystem(IServiceResolver sr, out IFlatLevelRegionLoaderSystem rs)
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

            var tracker = GetOrCreateMapRegionTrackerService(sr);

            var config = sr.ResolveConfiguration<MapRegionModuleConfiguration>();
            rs = new MapRegionLoaderSystem<int>(tracker, strategy, config.MapLoadingTimeout);
            return true;
        }

        IFlatLevelRegionRequestHandlerSystem GetOrCreateMapRegionRequestHandlerSystem(IServiceResolver sr)
        {
            if (sr.TryResolve(out IFlatLevelRegionRequestHandlerSystem rs))
            {
                return rs;
            }

            var tracker = GetOrCreateMapRegionTrackerService(sr);
            rs = new BasicFlatLevelRegionRequestHandlerSystem(tracker);
            sr.Store(rs);
            return rs;
        }

        bool TryGetOrResolveMapRegionEvictionSystem(IServiceResolver sr, out IMapRegionEvictionSystem rs)
        {
            if (sr.TryResolve(out rs))
            {
                return true;
            }

            // Unable to detect a map region system. That means we try to assemble one
            // from fragment services instead.
            if (!sr.TryResolve(out IMapRegionEvictionStrategy<int> strategy))
            {
                return false;
            }

            var tracker = GetOrCreateMapRegionTrackerService(sr);
            var config = GetOrCreateMapRegionConfig(sr);
            rs = new MapRegionEvictionSystem<int>(tracker, strategy, config.MapLoadingTimeout);
            sr.Store(rs);
            return true;
        }

        static MapRegionModuleConfiguration GetOrCreateMapRegionConfig(IServiceResolver sr)
        {
            if (sr.TryResolve(out MapRegionModuleConfiguration config))
            {
                return config;
            }

            var configHost = sr.Resolve<IConfigurationHost>();
            config = configHost.GetConfiguration<MapRegionModuleConfiguration>();
            sr.Store(config);
            return config;
        }

        static IMapRegionTrackerService<int> GetOrCreateMapRegionTrackerService(IServiceResolver sr)
        {
            if (!sr.TryResolve(out IMapRegionTrackerService<int> loader))
            {
                loader = new BasicMapRegionTrackerService<int>();
                sr.Store(loader);
                sr.Store<IMapRegionTrackerService>(loader);
            }

            return loader;
        }

        static FlatLevelAutomaticEvictionService<TActorId> GetOrCreateAutoEvictionService<TActorId>(IServiceResolver sr)
            where TActorId : IEntityKey
        {
            var config = GetOrCreateMapRegionConfig(sr);
            var tracker = GetOrCreateMapRegionTrackerService(sr);
            var autoEvictionSystem = new FlatLevelAutomaticEvictionService<TActorId>(sr.Resolve<IPlayerLookup<TActorId>>(),
                                                                                     sr.Resolve<IItemResolver<TActorId>>(),
                                                                                     tracker,
                                                                                     config,
                                                                                     sr.ResolveToReference<ITimeSource>());
            sr.Store(autoEvictionSystem);
            return autoEvictionSystem;
        }
    }
}
