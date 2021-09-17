using EnTTSharp.Entities;
using EnTTSharp.Entities.Systems;
using RogueEntity.Api.GameLoops;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Modules;
using RogueEntity.Api.Modules.Attributes;
using RogueEntity.Api.Services;
using RogueEntity.Core.Players;
using System;

namespace RogueEntity.Core.MapLoading.MapRegions
{
    public class MapLoadingModule : ModuleBase
    {
        public static readonly string ModuleId = "Core.MapLoading";
        static readonly EntitySystemId MapLoadingCommandsSystemId = "System.Core.MapLoading.CommandSystem";
        static readonly EntitySystemId MapLoadingSystemId = "System.Core.MapLoading.LoaderSystem";
        static readonly EntitySystemId RegisterChangeLevelCommandComponentId = "Entities.Core.MapLoading.ChangeLevelCommands";

        public static readonly EntityRole ControlLevelLoadingRole = new EntityRole("Role.Core.MapLoading.ControlLevelLoading");

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
            var entityContext = initializer.DeclareEntityContext<TItemId>();
            entityContext.Register(RegisterChangeLevelCommandComponentId, -10_000, RegisterCommandEntities);
            entityContext.Register(MapLoadingCommandsSystemId, 30_000, RegisterMapLoaderCommandsSystem);
            entityContext.Register(MapLoadingSystemId, 31_000, RegisterMapLoaderSystem);
        }

        void RegisterMapLoaderCommandsSystem<TEntityId>(in ModuleEntityInitializationParameter<TEntityId> initParameter,
                                                        IGameLoopSystemRegistration context,
                                                        EntityRegistry<TEntityId> registry)
            where TEntityId : IEntityKey
        {
            var system = GetOrResolveMapRegionSystem(initParameter.ServiceResolver);
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
            var system = GetOrResolveMapRegionSystem(initParameter.ServiceResolver);
            context.AddLateVariableStepHandlers(system.LoadChunks);
        }


        IMapRegionSystem GetOrResolveMapRegionSystem(IServiceResolver sr)
        {
            if (sr.TryResolve(out IMapRegionSystem rs))
            {
                return rs;
            }

            if (!sr.TryResolve(out IMapRegionLoaderService<int> loader))
            {
                throw new ArgumentException("Unable to locale map loader; either provide a map source or declare a custom map region server");
            }

            if (sr.TryResolve(out IMapRegionLoaderSystemConfiguration conf))
            {
                rs = new BasicMapRegionSystem(loader, conf.MapLoadingTimeout);
            }
            else
            {
                rs = new BasicMapRegionSystem(loader);
            }

            return rs;
        }
    }
}
