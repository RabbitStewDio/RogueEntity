using System;
using EnTTSharp.Entities;
using EnTTSharp.Entities.Systems;
using Microsoft.Extensions.Configuration;
using RogueEntity.Api.GameLoops;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Modules;
using RogueEntity.Api.Modules.Attributes;
using RogueEntity.Api.Services;
using RogueEntity.Api.Time;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Sensing.Cache;
using RogueEntity.Core.Sensing.Receptors.InfraVision;
using RogueEntity.Core.Sensing.Sources;
using RogueEntity.Core.Sensing.Sources.Heat;

namespace RogueEntity.Core.Sensing.Map.InfraVision
{
    public class InfraVisionMapModule : ModuleBase
    {
        public const string ModuleId = "Core.Sense.Map.Temperature";

        public static readonly EntitySystemId RegisterEntityId = "Entities.Core.Senses.Map.Temperature";

        public static readonly EntitySystemId ReceptorPreparationSystemId = "Systems.Core.Senses.Map.Temperature.Prepare";
        public static readonly EntitySystemId ReceptorCollectSystemId = "Systems.Core.Senses.Map.Temperature.Collect";
        public static readonly EntitySystemId ReceptorComputeSystemId = "Systems.Core.Senses.Map.Temperature.Compute";
        public static readonly EntitySystemId ReceptorFinalizeSystemId = "Systems.Core.Senses.Map.Light.Finalize";

        public InfraVisionMapModule()
        {
            Id = ModuleId;

            Author = "RogueEntity.Core";
            Name = "RogueEntity Core Module - Senses - Maps - InfraVision";
            Description = "Computes a global sense map.";
            IsFrameworkModule = true;

            DeclareDependencies(ModuleDependency.Of(PositionModule.ModuleId),
                                ModuleDependency.Of(HeatSourceModule.ModuleId),
                                ModuleDependency.Of(InfraVisionSenseModule.ModuleId),
                                ModuleDependency.Of(SensoryCacheModule.ModuleId));
        }

        [ModuleInitializer]
        protected void InitializeSenseCacheSystem(in ModuleInitializationParameter initParameter,
                                                  IModuleInitializer moduleInitializer)
        {
            if (!IsServiceEnabled(initParameter.ServiceResolver))
            {
                return;
            }

            if (initParameter.EntityInformation.RoleExists(HeatSourceModule.SenseSourceRole))
            {
                moduleInitializer.Register(ReceptorPreparationSystemId, 1, RegisterShutdownHook);
            }
        }

        [EntityRoleInitializer("Role.Core.Senses.Source.Temperature")]
        protected void InitializeSenseCollection<TItemId>(in ModuleEntityInitializationParameter<TItemId> initParameter,
                                                          IModuleInitializer initializer,
                                                          EntityRole role)
            where TItemId : IEntityKey
        {
            if (!IsServiceEnabled(initParameter.ServiceResolver))
            {
                return;
            }

            var ctx = initializer.DeclareEntityContext<TItemId>();
            ctx.Register(ReceptorCollectSystemId, 5750, RegisterCollectSenseSourcesSystem);
            ctx.Register(ReceptorComputeSystemId, 5850, RegisterComputeSenseMapSystem);
            ctx.Register(ReceptorFinalizeSystemId, 5950, RegisterFinalizeSenseMapSystem);
        }

        bool IsServiceEnabled(IServiceResolver serviceResolver)
        {
            return serviceResolver.TryResolve(out IConfiguration config) && config.GetValue("RogueEntity:Core:Sensing:Map:InfraVision:Enabled", false);
        }

        void RegisterCollectSenseSourcesSystem<TItemId>(in ModuleInitializationParameter initParameter,
                                                        IGameLoopSystemRegistration context,
                                                        EntityRegistry<TItemId> registry)
            where TItemId : IEntityKey
        {
            var resolver = initParameter.ServiceResolver;
            var hs = GetOrCreate(resolver);

            var system =
                registry.BuildSystem()
                        .WithoutContext()
                        .WithInputParameter<HeatSourceDefinition, SenseSourceState<TemperatureSense>>()
                        .CreateSystem(hs.CollectSenseSources);

            context.AddInitializationStepHandler(system);
            context.AddFixedStepHandlers(system);
        }

        void RegisterComputeSenseMapSystem<TItemId>(in ModuleInitializationParameter initParameter,
                                                    IGameLoopSystemRegistration context,
                                                    EntityRegistry<TItemId> registry)
            where TItemId : IEntityKey
        {
            var resolver = initParameter.ServiceResolver;
            var hs = GetOrCreate(resolver);

            context.AddInitializationStepHandler(() => hs.ProcessSenseMap(registry));
            context.AddFixedStepHandlers(() => hs.ProcessSenseMap(registry));
        }

        void RegisterFinalizeSenseMapSystem<TItemId>(in ModuleInitializationParameter initParameter,
                                                     IGameLoopSystemRegistration context,
                                                     EntityRegistry<TItemId> registry)
            where TItemId : IEntityKey
        {
            var resolver = initParameter.ServiceResolver;
            var hs = GetOrCreate(resolver);

            context.AddInitializationStepHandler(hs.EndSenseCalculation);
            context.AddFixedStepHandlers(hs.EndSenseCalculation);
        }

        void RegisterShutdownHook(in ModuleInitializationParameter initParameter,
                                  IGameLoopSystemRegistration context)
        {
            var resolver = initParameter.ServiceResolver;
            var hs = GetOrCreate(resolver);

            context.AddDisposeStepHandler(hs.ShutDown);
        }

        InfraVisionMapSystem GetOrCreate(IServiceResolver resolver)
        {
            if (resolver.TryResolve(out InfraVisionMapSystem system))
            {
                return system;
            }

            if (!resolver.TryResolve(out ISenseMapDataBlitter blitter))
            {
                blitter = new DefaultSenseMapDataBlitter();
            }

            if (!resolver.TryResolve(out IInfraVisionSenseReceptorPhysicsConfiguration physics))
            {
                if (!resolver.TryResolve(out IHeatPhysicsConfiguration sourcePhysics))
                {
                    throw new InvalidOperationException("There is neither a heat-physics nor a infra-vision physics configuration defined");
                }

                physics = new InfraVisionSenseReceptorPhysicsConfiguration(sourcePhysics);
            }

            system = new InfraVisionMapSystem(resolver.ResolveToReference<ITimeSource>(),
                                              physics,
                                              blitter);
            return system;
        }
    }
}
