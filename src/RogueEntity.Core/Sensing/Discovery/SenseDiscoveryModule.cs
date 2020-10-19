using System.Runtime.Versioning;
using EnTTSharp.Entities;
using EnTTSharp.Entities.Systems;
using RogueEntity.Core.Infrastructure.Commands;
using RogueEntity.Core.Infrastructure.GameLoops;
using RogueEntity.Core.Infrastructure.Modules;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Continuous;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Sensing.Map.Light;
using RogueEntity.Core.Sensing.Receptors;
using RogueEntity.Core.Sensing.Receptors.Light;
using RogueEntity.Core.Sensing.Sources.Light;

namespace RogueEntity.Core.Sensing.Discovery
{
    [Module]
    public class SenseDiscoveryModule : ModuleBase
    {
        public const string ModuleId = "Core.Sense.Discovery";

        public static readonly EntitySystemId RegisterEntities = "Entities.Core.Senses.DiscoveredArea";
        public static readonly EntitySystemId RegisterSystem = "Systems.Core.Senses.DiscoveredArea";
        public static readonly EntityRole DiscoveryActorRole = new EntityRole("Role.Core.Senses.DiscoveredArea");
        public static readonly EntityRole DiscoveryActorRoleGrid = new EntityRole("Role.Core.Senses.DiscoveredArea.Grid");
        public static readonly EntityRole DiscoveryActorRoleContinuous = new EntityRole("Role.Core.Senses.DiscoveredArea.Continuous");

        public SenseDiscoveryModule()
        {
            Id = ModuleId;
            Author = "RogueEntity.Core";
            Name = "RogueEntity Core Module - Senses - Discovered Area";
            Description = "Provides support for computing discovered areas for a fog-of-war effect.";
            IsFrameworkModule = true;

            DeclareDependencies(ModuleDependency.Of(PositionModule.ModuleId),
                                ModuleDependency.Of(VisionSenseModule.ModuleId));

            RequireRole(DiscoveryActorRole);
        }

        [EntityRoleInitializer("Role.Core.Senses.DiscoveredArea")]
        protected void Initialize<TGameContext, TActorId>(IServiceResolver serviceResolver,
                                                          IModuleInitializer<TGameContext> initializer,
                                                          EntityRole role)
            where TActorId : IEntityKey
        {
            var ctx = initializer.DeclareEntityContext<TActorId>();
            ctx.Register(RegisterEntities, 0, RegisterDiscoveryEntities);
        }

        [EntityRoleInitializer("Role.Core.Senses.DiscoveredArea.Grid")]
        protected void InitializeGrid<TGameContext, TActorId>(IServiceResolver serviceResolver,
                                                              IModuleInitializer<TGameContext> initializer,
                                                              EntityRole role)
            where TActorId : IEntityKey
        {
            var ctx = initializer.DeclareEntityContext<TActorId>();
            ctx.Register(RegisterSystem, 7000, RegisterDiscoveryActionsGrid);
        }

        [EntityRoleInitializer("Role.Core.Senses.DiscoveredArea.Continuous")]
        protected void InitializeContinuous<TGameContext, TActorId>(IServiceResolver serviceResolver,
                                                                    IModuleInitializer<TGameContext> initializer,
                                                                    EntityRole role)
            where TActorId : IEntityKey
        {
            var ctx = initializer.DeclareEntityContext<TActorId>();
            ctx.Register(RegisterSystem, 7000, RegisterDiscoveryActionsContinuous);
        }

        void RegisterDiscoveryActionsGrid<TGameContext, TActorId>(IServiceResolver serviceResolver,
                                                                  IGameLoopSystemRegistration<TGameContext> context,
                                                                  EntityRegistry<TActorId> registry,
                                                                  ICommandHandlerRegistration<TGameContext, TActorId> handler)
            where TActorId : IEntityKey
        {
            var system = new DiscoveryMapSystem(serviceResolver.ResolveToReference<IBrightnessMap>());

            var entitySystem = registry.BuildSystem()
                                       .WithContext<TGameContext>()
                                       .CreateSystem<EntityGridPosition, OnDemandDiscoveryMap, SensoryReceptorState<VisionSense>>(system.ExpandDiscoveredAreaGrid);

            context.AddFixedStepHandlers(entitySystem);
            context.AddInitializationStepHandler(entitySystem);
        }

        void RegisterDiscoveryActionsContinuous<TGameContext, TActorId>(IServiceResolver serviceResolver,
                                                                        IGameLoopSystemRegistration<TGameContext> context,
                                                                        EntityRegistry<TActorId> registry,
                                                                        ICommandHandlerRegistration<TGameContext, TActorId> handler)
            where TActorId : IEntityKey
        {
            var system = new DiscoveryMapSystem(serviceResolver.ResolveToReference<IBrightnessMap>());

            var entitySystem = registry.BuildSystem()
                                       .WithContext<TGameContext>()
                                       .CreateSystem<ContinuousMapPosition, OnDemandDiscoveryMap, SensoryReceptorState<VisionSense>>(system.ExpandDiscoveredAreaContinuous);

            context.AddFixedStepHandlers(entitySystem);
            context.AddInitializationStepHandler(entitySystem);
        }

        void RegisterDiscoveryEntities<TActorId>(IServiceResolver serviceResolver,
                                                 EntityRegistry<TActorId> registry)
            where TActorId : IEntityKey
        {
            registry.RegisterNonConstructable<OnDemandDiscoveryMap>();
        }
    }
}