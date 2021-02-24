using EnTTSharp.Entities;
using EnTTSharp.Entities.Systems;
using RogueEntity.Api.GameLoops;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Modules;
using RogueEntity.Api.Modules.Attributes;
using RogueEntity.Core.Players;

namespace RogueEntity.Core.MapLoading
{
    public class MapLoadingModule : ModuleBase
    {
        public static readonly string ModuleId = "Core.MapLoading";
        static readonly EntitySystemId MapLoadingCommandsSystemId = "System.Core.MapLoading.CommandSystem";
        static readonly EntitySystemId MapLoadingSystemId = "System.Core.MapLoading.LoaderSystem";
        static readonly EntitySystemId RegisterMapLoadingEntitiesSystemId = "Entities.Core.MapLoading";

        public MapLoadingModule()
        {
            Id = ModuleId;
            Author = "RogueEntity.Core";
            Name = "RogueEntity Core Module - Map Loading";
            Description = "Provides base classes and behaviours for loading maps based on observer positions";
            IsFrameworkModule = true;

            DeclareDependency(ModuleDependency.Of(PlayerModule.ModuleId));
        }

        [EntityRoleInitializer("Role.Core.Player.PlayerObserver")]
        protected void InitializeMapLoadingSystem<TItemId>(in ModuleEntityInitializationParameter<TItemId> initParameter,
                                                           IModuleInitializer initializer,
                                                           EntityRole r)
            where TItemId : IEntityKey
        {
            var entityContext = initializer.DeclareEntityContext<TItemId>();
            entityContext.Register(RegisterMapLoadingEntitiesSystemId, 30_000, RegisterEntities);
            entityContext.Register(MapLoadingCommandsSystemId, 30_000, RegisterMapLoaderCommandsSystem);
            entityContext.Register(MapLoadingSystemId, 31_000, RegisterMapLoaderSystem);
        }


        void RegisterMapLoaderCommandsSystem<TEntityId>(in ModuleEntityInitializationParameter<TEntityId> initParameter,
                                                        IGameLoopSystemRegistration context,
                                                        EntityRegistry<TEntityId> registry)
            where TEntityId : IEntityKey
        {
            if (!initParameter.ServiceResolver.TryResolve(out IMapRegionSystem system))
            {
                return;
            }

            var handleNewPlayersSystem = registry.BuildSystem()
                                                 .WithoutContext()
                                                 .WithInputParameter<PlayerObserverTag>()
                                                 .WithInputParameter<NewPlayerTag>()
                                                 .CreateSystem(system.RequestLoadLevelFromNewPlayer);

            context.AddFixedStepHandlerSystem(handleNewPlayersSystem);

            var handleChangeLevelSystem = registry.BuildSystem()
                                                  .WithoutContext()
                                                  .WithInputParameter<PlayerObserverTag>()
                                                  .WithInputParameter<ChangeLevelCommand>()
                                                  .CreateSystem(system.RequestLoadLevelFromChangeLevelCommand);
            context.AddFixedStepHandlerSystem(handleChangeLevelSystem);

            var handleChangePositionSystem = registry.BuildSystem()
                                                     .WithoutContext()
                                                     .WithInputParameter<PlayerObserverTag>()
                                                     .WithInputParameter<ChangeLevelPositionCommand>()
                                                     .CreateSystem(system.RequestLoadLevelFromChangePositionCommand);
            context.AddFixedStepHandlerSystem(handleChangePositionSystem);
        }

        void RegisterMapLoaderSystem<TEntityId>(in ModuleEntityInitializationParameter<TEntityId> initParameter,
                                                IGameLoopSystemRegistration context,
                                                EntityRegistry<TEntityId> registry)
            where TEntityId : IEntityKey
        {
            if (!initParameter.ServiceResolver.TryResolve(out IMapRegionSystem system))
            {
                return;
            }

            context.AddLateVariableStepHandlers(system.LoadChunks);
        }

        void RegisterEntities<TItemId>(in ModuleEntityInitializationParameter<TItemId> initParameter,
                                       EntityRegistry<TItemId> registry)
            where TItemId : IEntityKey
        {
            registry.RegisterNonConstructable<ChangeLevelCommand>();
            registry.RegisterNonConstructable<ChangeLevelPositionCommand>();
        }
    }
}
