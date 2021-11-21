using EnTTSharp.Entities;
using EnTTSharp.Entities.Systems;
using RogueEntity.Api.GameLoops;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Modules;
using RogueEntity.Api.Modules.Attributes;
using RogueEntity.Core.Inputs.Commands;
using RogueEntity.Core.MapLoading.FlatLevelMaps;
using RogueEntity.Core.MapLoading.MapRegions;
using RogueEntity.Core.Positioning;

namespace RogueEntity.Generator
{
    public class GeneratorModule : ModuleBase
    {
        public static readonly string ModuleId = "Extras.Generator";

        static readonly EntitySystemId RegisterChangeLevelCommandComponentId = "Entities.Extras.Generator.ChangeLevelCommand";
        static readonly EntitySystemId RegisterCommandSystemId = "Systems.Extras.Generator.RegisterCommandSystem";
        static readonly EntitySystemId RegisterInitializeMapLoaderSystemId = "Systems.Extras.Generator.RegisterMapLoader";

        // Role.Core.Inputs.Commands.Executor[RogueEntity.Generator.Commands.ChangeLevelCommand]
        public static readonly EntityRole ChangeLevelCommandRole = CommandRoles.CreateRoleFor(CommandType.Of<ChangeLevelCommand>());

        public GeneratorModule()
        {
            Id = ModuleId;
            Author = "RogueEntity.Extras.Generator";
            Name = "RogueEntity Map Generator Module";
            Description = "Provides services to generate and load maps";
            IsFrameworkModule = true;

            DeclareDependencies(ModuleDependency.Of(FlatLevelMapModule.ModuleId));

            this.RequireRole(ChangeLevelCommandRole)
                .WithRequiredRole(PositionModule.PositionedRole)
                .WithRequiredRole(MapLoadingModule.ControlLevelLoadingRole);
        }

        [ModuleFinalizer]
        protected void InitializeGlobalMapLoadingSystem(in ModuleInitializationParameter initParameter,
                                                        IModuleInitializer moduleInitializer)
        {
            moduleInitializer.Register(RegisterInitializeMapLoaderSystemId, 0, RegisterInitializeMapLoader);
        }

        void RegisterInitializeMapLoader(in ModuleInitializationParameter initParameter, IGameLoopSystemRegistration context)
        {
            if (initParameter.ServiceResolver.TryResolve(out IMapRegionMetaDataService<int> mds) &&
                mds is StaticMapLevelDataSource ds)
            {
                ds.Initialize();
            }
        }


        [EntityRoleInitializer("Role.Core.Inputs.Commands.Executor[RogueEntity.Generator.Commands.ChangeLevelCommand]")]
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
            var sys = new ChangeLevelCommandSystem<TActorId>(sr.Resolve<IMapRegionMetaDataService<int>>(),
                                                             sr.Resolve<IItemResolver<TActorId>>(),
                                                             sr.Resolve<IItemPlacementService<TActorId>>());

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
        }
    }
}
