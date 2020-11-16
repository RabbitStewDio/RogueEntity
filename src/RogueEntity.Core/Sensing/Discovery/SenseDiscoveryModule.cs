using EnTTSharp.Entities;
using EnTTSharp.Entities.Systems;
using RogueEntity.Api.GameLoops;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Modules;
using RogueEntity.Api.Modules.Attributes;
using RogueEntity.Api.Services;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Sensing.Receptors;
using RogueEntity.Core.Sensing.Receptors.InfraVision;
using RogueEntity.Core.Sensing.Receptors.Light;
using RogueEntity.Core.Sensing.Receptors.Touch;

namespace RogueEntity.Core.Sensing.Discovery
{
    [Module]
    public class SenseDiscoveryModule : ModuleBase
    {
        public const string ModuleId = "Core.Sense.Discovery";

        public static readonly EntitySystemId RegisterEntities = "Entities.Core.Senses.DiscoveredArea";
        public static readonly EntitySystemId RegisterVisionSystem = "Systems.Core.Senses.DiscoveredArea.Vision";
        public static readonly EntitySystemId RegisterInfraVisionSystem = "Systems.Core.Senses.DiscoveredArea.InfraVision";
        public static readonly EntitySystemId RegisterTouchSystem = "Systems.Core.Senses.DiscoveredArea.Touch";

        public static readonly EntityRole DiscoveryActorRole = new EntityRole("Role.Core.Senses.DiscoveredArea");

        public SenseDiscoveryModule()
        {
            Id = ModuleId;
            Author = "RogueEntity.Core";
            Name = "RogueEntity Core Module - Senses - Discovered Area";
            Description = "Provides support for computing discovered areas for a fog-of-war effect.";
            IsFrameworkModule = true;

            DeclareDependencies(ModuleDependency.Of(PositionModule.ModuleId),
                                ModuleDependency.Of(VisionSenseModule.ModuleId),
                                ModuleDependency.Of(InfraVisionSenseModule.ModuleId),
                                ModuleDependency.Of(TouchSenseModule.ModuleId)
            );

            RequireRole(DiscoveryActorRole);
        }

        [EntityRoleInitializer("Role.Core.Senses.DiscoveredArea")]
        protected void Initialize<TGameContext, TActorId>(in ModuleEntityInitializationParameter<TGameContext, TActorId> initParameter,
                                                          IModuleInitializer<TGameContext> initializer,
                                                          EntityRole role)
            where TActorId : IEntityKey
        {
            var ctx = initializer.DeclareEntityContext<TActorId>();
            ctx.Register(RegisterEntities, 0, RegisterDiscoveryEntities);
        }

        [EntityRoleInitializer("Role.Core.Senses.DiscoveredArea",
                               ConditionalRoles = new[] {"Role.Core.Senses.Receptor.Vision.ActorRole"})]
        protected void InitializeVision<TGameContext, TActorId>(in ModuleEntityInitializationParameter<TGameContext, TActorId> initParameter,
                                                                IModuleInitializer<TGameContext> initializer,
                                                                EntityRole role)
            where TActorId : IEntityKey
        {
            var ctx = initializer.DeclareEntityContext<TActorId>();
            ctx.Register(RegisterVisionSystem, 60_000, RegisterDiscoveryActionsVision);
        }

        [EntityRoleInitializer("Role.Core.Senses.DiscoveredArea",
                               ConditionalRoles = new[] {"Role.Core.Senses.Receptor.InfraVision.ActorRole"})]
        protected void InitializeInfraVision<TGameContext, TActorId>(in ModuleEntityInitializationParameter<TGameContext, TActorId> initParameter,
                                                                     IModuleInitializer<TGameContext> initializer,
                                                                     EntityRole role)
            where TActorId : IEntityKey
        {
            var ctx = initializer.DeclareEntityContext<TActorId>();
            ctx.Register(RegisterInfraVisionSystem, 60_100, RegisterDiscoveryActionsInfraVision);
        }

        [EntityRoleInitializer("Role.Core.Senses.DiscoveredArea",
                               ConditionalRoles = new[] {"Role.Core.Senses.Receptor.Touch.ActorRole"})]
        protected void InitializeTouch<TGameContext, TActorId>(in ModuleEntityInitializationParameter<TGameContext, TActorId> initParameter,
                                                               IModuleInitializer<TGameContext> initializer,
                                                               EntityRole role)
            where TActorId : IEntityKey
        {
            var ctx = initializer.DeclareEntityContext<TActorId>();
            ctx.Register(RegisterTouchSystem, 60_200, RegisterDiscoveryActionsTouch);
        }

        void RegisterDiscoveryActionsVision<TGameContext, TActorId>(in ModuleInitializationParameter initParameter,
                                                                    IGameLoopSystemRegistration<TGameContext> context,
                                                                    EntityRegistry<TActorId> registry)
            where TActorId : IEntityKey
        {
            var serviceResolver = initParameter.ServiceResolver;
            var system = GetOrCreate(serviceResolver);

            var entitySystem = registry.BuildSystem()
                                       .WithContext<TGameContext>()
                                       .CreateSystem<DiscoveryMapData,
                                           SensoryReceptorState<VisionSense, VisionSense>,
                                           SingleLevelSenseDirectionMapData<VisionSense, VisionSense>,
                                           SenseReceptorDirtyFlag<VisionSense, VisionSense>>(system.ExpandDiscoveredArea);

            context.AddFixedStepHandlers(entitySystem);
            context.AddInitializationStepHandler(entitySystem);
        }

        void RegisterDiscoveryActionsInfraVision<TGameContext, TActorId>(in ModuleInitializationParameter initParameter,
                                                                         IGameLoopSystemRegistration<TGameContext> context,
                                                                         EntityRegistry<TActorId> registry)
            where TActorId : IEntityKey
        {
            var serviceResolver = initParameter.ServiceResolver;
            var system = GetOrCreate(serviceResolver);

            var entitySystem = registry.BuildSystem()
                                       .WithContext<TGameContext>()
                                       .CreateSystem<DiscoveryMapData,
                                           SensoryReceptorState<VisionSense, TemperatureSense>,
                                           SingleLevelSenseDirectionMapData<VisionSense, TemperatureSense>,
                                           SenseReceptorDirtyFlag<VisionSense, TemperatureSense>>(system.ExpandDiscoveredArea);

            context.AddFixedStepHandlers(entitySystem);
            context.AddInitializationStepHandler(entitySystem);
        }

        void RegisterDiscoveryActionsTouch<TGameContext, TActorId>(in ModuleInitializationParameter initParameter,
                                                                   IGameLoopSystemRegistration<TGameContext> context,
                                                                   EntityRegistry<TActorId> registry)
            where TActorId : IEntityKey
        {
            var serviceResolver = initParameter.ServiceResolver;
            var system = GetOrCreate(serviceResolver);

            var entitySystem = registry.BuildSystem()
                                       .WithContext<TGameContext>()
                                       .CreateSystem<DiscoveryMapData,
                                           SensoryReceptorState<TouchSense, TouchSense>,
                                           SingleLevelSenseDirectionMapData<TouchSense, TouchSense>,
                                           SenseReceptorDirtyFlag<TouchSense, TouchSense>>(system.ExpandDiscoveredArea);

            context.AddFixedStepHandlers(entitySystem);
            context.AddInitializationStepHandler(entitySystem);
        }

        DiscoveryMapSystem GetOrCreate(IServiceResolver resolver)
        {
            if (resolver.TryResolve(out DiscoveryMapSystem s))
            {
                return s;
            }

            s = new DiscoveryMapSystem();
            resolver.Store(s);
            return s;
        }

        void RegisterDiscoveryEntities<TActorId>(in ModuleInitializationParameter initParameter,
                                                 EntityRegistry<TActorId> registry)
            where TActorId : IEntityKey
        {
            registry.RegisterNonConstructable<DiscoveryMapData>();
        }
    }
}