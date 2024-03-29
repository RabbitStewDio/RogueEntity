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
        protected void Initialize<TActorId>(in ModuleEntityInitializationParameter<TActorId> initParameter,
                                            IModuleInitializer initializer,
                                            EntityRole role)
            where TActorId : struct, IEntityKey
        {
            var ctx = initializer.DeclareEntityContext<TActorId>();
            ctx.Register(RegisterEntities, 0, RegisterDiscoveryEntities);
        }

        [EntityRoleInitializer("Role.Core.Senses.DiscoveredArea",
                               ConditionalRoles = new[] {"Role.Core.Senses.Receptor.Vision.ActorRole"})]
        protected void InitializeVision<TActorId>(in ModuleEntityInitializationParameter<TActorId> initParameter,
                                                  IModuleInitializer initializer,
                                                  EntityRole role)
            where TActorId : struct, IEntityKey
        {
            var ctx = initializer.DeclareEntityContext<TActorId>();
            ctx.Register(RegisterVisionSystem, 60_000, RegisterDiscoveryActionsVision);
        }

        [EntityRoleInitializer("Role.Core.Senses.DiscoveredArea",
                               ConditionalRoles = new[] {"Role.Core.Senses.Receptor.InfraVision.ActorRole"})]
        protected void InitializeInfraVision<TActorId>(in ModuleEntityInitializationParameter<TActorId> initParameter,
                                                       IModuleInitializer initializer,
                                                       EntityRole role)
            where TActorId : struct, IEntityKey
        {
            var ctx = initializer.DeclareEntityContext<TActorId>();
            ctx.Register(RegisterInfraVisionSystem, 60_100, RegisterDiscoveryActionsInfraVision);
        }

        [EntityRoleInitializer("Role.Core.Senses.DiscoveredArea",
                               ConditionalRoles = new[] {"Role.Core.Senses.Receptor.Touch.ActorRole"})]
        protected void InitializeTouch<TActorId>(in ModuleEntityInitializationParameter<TActorId> initParameter,
                                                 IModuleInitializer initializer,
                                                 EntityRole role)
            where TActorId : struct, IEntityKey
        {
            var ctx = initializer.DeclareEntityContext<TActorId>();
            ctx.Register(RegisterTouchSystem, 60_200, RegisterDiscoveryActionsTouch);
        }

        void RegisterDiscoveryActionsVision<TActorId>(in ModuleEntityInitializationParameter<TActorId> initParameter,
                                                      IGameLoopSystemRegistration context,
                                                      EntityRegistry<TActorId> registry)
            where TActorId : struct, IEntityKey
        {
            var serviceResolver = initParameter.ServiceResolver;
            var system = GetOrCreate(serviceResolver);

            var entitySystem = registry.BuildSystem()
                                       .WithoutContext()
                                       .WithInputParameter<DiscoveryMapData,
                                           SensoryReceptorState<VisionSense, VisionSense>,
                                           SingleLevelSenseDirectionMapData<VisionSense, VisionSense>,
                                           SenseReceptorDirtyFlag<VisionSense, VisionSense>>()
                                       .CreateSystem(system.ExpandDiscoveredArea);

            context.AddFixedStepHandlerSystem(entitySystem);
            context.AddInitializationStepHandlerSystem(entitySystem);
        }

        void RegisterDiscoveryActionsInfraVision<TActorId>(in ModuleEntityInitializationParameter<TActorId> initParameter,
                                                           IGameLoopSystemRegistration context,
                                                           EntityRegistry<TActorId> registry)
            where TActorId : struct, IEntityKey
        {
            var serviceResolver = initParameter.ServiceResolver;
            var system = GetOrCreate(serviceResolver);

            var entitySystem = registry.BuildSystem()
                                       .WithoutContext()
                                       .WithInputParameter<DiscoveryMapData,
                                           SensoryReceptorState<VisionSense, TemperatureSense>,
                                           SingleLevelSenseDirectionMapData<VisionSense, TemperatureSense>,
                                           SenseReceptorDirtyFlag<VisionSense, TemperatureSense>>()
                                       .CreateSystem(system.ExpandDiscoveredArea);

            context.AddFixedStepHandlerSystem(entitySystem);
            context.AddInitializationStepHandlerSystem(entitySystem);
        }

        void RegisterDiscoveryActionsTouch<TActorId>(in ModuleEntityInitializationParameter<TActorId> initParameter,
                                                     IGameLoopSystemRegistration context,
                                                     EntityRegistry<TActorId> registry)
            where TActorId : struct, IEntityKey
        {
            var serviceResolver = initParameter.ServiceResolver;
            var system = GetOrCreate(serviceResolver);

            var entitySystem = registry.BuildSystem()
                                       .WithoutContext()
                                       .WithInputParameter<DiscoveryMapData,
                                           SensoryReceptorState<TouchSense, TouchSense>,
                                           SingleLevelSenseDirectionMapData<TouchSense, TouchSense>,
                                           SenseReceptorDirtyFlag<TouchSense, TouchSense>>()
                                       .CreateSystem(system.ExpandDiscoveredArea);

            context.AddFixedStepHandlerSystem(entitySystem);
            context.AddInitializationStepHandlerSystem(entitySystem);
        }

        DiscoveryMapSystem GetOrCreate(IServiceResolver resolver)
        {
            if (resolver.TryResolve<DiscoveryMapSystem>(out var s))
            {
                return s;
            }

            s = new DiscoveryMapSystem();
            resolver.Store(s);
            return s;
        }

        void RegisterDiscoveryEntities<TActorId>(in ModuleEntityInitializationParameter<TActorId> initParameter,
                                                 EntityRegistry<TActorId> registry)
            where TActorId : struct, IEntityKey
        {
            registry.RegisterNonConstructable<DiscoveryMapData>();
        }
    }
}
