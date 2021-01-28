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
using RogueEntity.Core.Sensing.Receptors.Light;
using RogueEntity.Core.Sensing.Sources;
using RogueEntity.Core.Sensing.Sources.Light;

namespace RogueEntity.Core.Sensing.Map.Light
{
    public class LightMapModule : ModuleBase
    {
        public const string ModuleId = "Core.Sense.Map.Light";

        public static readonly EntitySystemId RegisterEntityId = "Entities.Core.Senses.Map.Light";

        public static readonly EntitySystemId ReceptorPreparationSystemId = "Systems.Core.Senses.Map.Light.Prepare";
        public static readonly EntitySystemId ReceptorCollectSystemId = "Systems.Core.Senses.Map.Light.Collect";
        public static readonly EntitySystemId ReceptorComputeSystemId = "Systems.Core.Senses.Map.Light.Compute";
        public static readonly EntitySystemId ReceptorFinalizeSystemId = "Systems.Core.Senses.Map.Light.Finalize";

        public LightMapModule()
        {
            Id = ModuleId;

            Author = "RogueEntity.Core";
            Name = "RogueEntity Core Module - Senses - Maps - Vision";
            Description = "Computes a global sense map.";
            IsFrameworkModule = true;

            DeclareDependencies(ModuleDependency.Of(PositionModule.ModuleId),
                                ModuleDependency.Of(LightSourceModule.ModuleId),
                                ModuleDependency.Of(VisionSenseModule.ModuleId),
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

            if (initParameter.EntityInformation.RoleExists(LightSourceModule.SenseSourceRole))
            {
                moduleInitializer.Register(ReceptorPreparationSystemId, 1, RegisterPrepareSystem);
            }
        }

        [EntityRoleInitializer("Role.Core.Senses.Source.Light")]
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
            ctx.Register(ReceptorCollectSystemId, 57500, RegisterCollectSenseSourcesSystem);
            ctx.Register(ReceptorComputeSystemId, 58500, RegisterComputeSenseMapSystem);
            ctx.Register(ReceptorFinalizeSystemId, 59500, RegisterFinalizeSenseMapSystem);
        }

        bool IsServiceEnabled(IServiceResolver serviceResolver)
        {
            return serviceResolver.TryResolve(out IConfiguration config) && config.GetValue("RogueEntity:Core:Sensing:Map:InfraVision:Enabled", false);
        }

        void RegisterCollectSenseSourcesSystem<TItemId>(in ModuleEntityInitializationParameter<TItemId> initParameter,
                                                        IGameLoopSystemRegistration context,
                                                        EntityRegistry<TItemId> registry)
            where TItemId : IEntityKey
        {
            var resolver = initParameter.ServiceResolver;
            var hs = GetOrCreate(resolver);

            var system =
                registry.BuildSystem()
                        .WithoutContext()
                        .WithInputParameter<LightSourceDefinition, SenseSourceState<VisionSense>>()
                        .CreateSystem(hs.CollectSenseSources);

            context.AddInitializationStepHandler(system);
            context.AddFixedStepHandlers(system);
        }

        void RegisterComputeSenseMapSystem<TItemId>(in ModuleEntityInitializationParameter<TItemId> initParameter,
                                                    IGameLoopSystemRegistration context,
                                                    EntityRegistry<TItemId> registry)
            where TItemId : IEntityKey
        {
            var resolver = initParameter.ServiceResolver;
            var hs = GetOrCreate(resolver);

            context.AddInitializationStepHandler(() => hs.ProcessSenseMap(registry));
            context.AddFixedStepHandlers(() => hs.ProcessSenseMap(registry));
        }

        void RegisterFinalizeSenseMapSystem<TItemId>(in ModuleEntityInitializationParameter<TItemId> initParameter,
                                                     IGameLoopSystemRegistration context,
                                                     EntityRegistry<TItemId> registry)
            where TItemId : IEntityKey
        {
            var resolver = initParameter.ServiceResolver;
            var hs = GetOrCreate(resolver);

            context.AddInitializationStepHandler(hs.EndSenseCalculation);
            context.AddFixedStepHandlers(hs.EndSenseCalculation);
        }

        void RegisterPrepareSystem(in ModuleInitializationParameter initParameter,
                                   IGameLoopSystemRegistration context)
        {
            var resolver = initParameter.ServiceResolver;
            var hs = GetOrCreate(resolver);

            context.AddDisposeStepHandler(hs.ShutDown);
        }

        LightMapSystem GetOrCreate(IServiceResolver resolver)
        {
            if (resolver.TryResolve(out LightMapSystem system))
            {
                return system;
            }

            if (!resolver.TryResolve(out ISenseMapDataBlitter blitter))
            {
                blitter = new DefaultSenseMapDataBlitter();
            }

            system = new LightMapSystem(resolver.ResolveToReference<ITimeSource>(),
                                        blitter);
            return system;
        }
    }
}
