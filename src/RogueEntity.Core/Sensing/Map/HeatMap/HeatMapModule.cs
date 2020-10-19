using EnTTSharp.Entities;
using EnTTSharp.Entities.Systems;
using RogueEntity.Core.Infrastructure.Commands;
using RogueEntity.Core.Infrastructure.GameLoops;
using RogueEntity.Core.Infrastructure.Modules;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Sensing.Cache;
using RogueEntity.Core.Sensing.Common.Blitter;
using RogueEntity.Core.Sensing.Receptors;
using RogueEntity.Core.Sensing.Receptors.InfraVision;
using RogueEntity.Core.Sensing.Receptors.Light;
using RogueEntity.Core.Sensing.Resistance;
using RogueEntity.Core.Sensing.Sources;
using RogueEntity.Core.Sensing.Sources.Heat;

namespace RogueEntity.Core.Sensing.Map.HeatMap
{
    public class HeatMapModule : ModuleBase
    {
        public const string ModuleId = "Core.Sense.Map.Temperature";

        public static readonly EntitySystemId RegisterEntityId = "Entities.Core.Senses.Map.Temperature";

        public static readonly EntitySystemId ReceptorPreparationSystemId = "Systems.Core.Senses.Map.Temperature.Prepare";
        public static readonly EntitySystemId ReceptorCollectSystemId = "Systems.Core.Senses.Map.Temperature.Collect";
        public static readonly EntitySystemId ReceptorComputeSystemId = "Systems.Core.Senses.Map.Temperature.Compute";
        public static readonly EntitySystemId ReceptorApplySystemId = "Systems.Core.Senses.Map.Temperature.ApplyReceptorFieldOfView";

        public HeatMapModule()
        {
            Id = ModuleId;

            Author = "RogueEntity.Core";
            Name = "RogueEntity Core Module - Senses - Maps - InfraVision";
            Description = "Computes a global sense map.";
            IsFrameworkModule = true;

            DeclareDependencies(ModuleDependency.Of(PositionModule.ModuleId),
                                ModuleDependency.Of(SensoryResistanceModule.ModuleId),
                                ModuleDependency.Of(HeatSourceModule.ModuleId),
                                ModuleDependency.Of(InfraVisionSenseModule.ModuleId),
                                ModuleDependency.Of(SensoryCacheModule.ModuleId));
        }

        [ModuleInitializer]
        protected void InitializeSenseCacheSystem<TGameContext>(IServiceResolver resolver,
                                                                IModuleInitializer<TGameContext> moduleInitializer)
        {
            moduleInitializer.Register(ReceptorPreparationSystemId, 1, RegisterPrepareSystem);
        }

        [EntityRoleInitializer("Role.Core.Senses.Source.Temperature")]
        protected void InitializeSenseCollection<TGameContext, TItemId>(IServiceResolver serviceResolver,
                                                                        IModuleInitializer<TGameContext> initializer,
                                                                        EntityRole role)
            where TItemId : IEntityKey
        {
            var ctx = initializer.DeclareEntityContext<TItemId>();
            ctx.Register(ReceptorCollectSystemId, 5750, RegisterCollectSenseSourcesSystem);
            ctx.Register(ReceptorComputeSystemId, 5850, RegisterComputeSenseMapSystem);
            ctx.Register(ReceptorApplySystemId, 5900, RegisterApplySenseMapSystem);
            
            // disable the per-receptor handling of sense sources.
            // we already compute a global view, that can be used here as well.
            ctx.Register(VisionSenseModule.SenseSourceCollectionSystemId, 5850, ctx.Empty());
            ctx.Register(VisionSenseModule.ReceptorComputeSystemId, 5850, ctx.Empty());
        }

        void RegisterCollectSenseSourcesSystem<TGameContext, TItemId>(IServiceResolver resolver,
                                                                      IGameLoopSystemRegistration<TGameContext> context,
                                                                      EntityRegistry<TItemId> registry,
                                                                      ICommandHandlerRegistration<TGameContext, TItemId> handler)
            where TItemId : IEntityKey
        {
            var hs = GetOrCreate(resolver);

            var system =
                registry.BuildSystem()
                        .WithContext<TGameContext>()
                        .CreateSystem<HeatSourceDefinition, SenseSourceState<VisionSense>>(hs.CollectSenseSources);
            
            context.AddInitializationStepHandler(system);
            context.AddFixedStepHandlers(system);
        }

        void RegisterComputeSenseMapSystem<TGameContext, TItemId>(IServiceResolver resolver,
                                                                      IGameLoopSystemRegistration<TGameContext> context,
                                                                      EntityRegistry<TItemId> registry,
                                                                      ICommandHandlerRegistration<TGameContext, TItemId> handler)
            where TItemId : IEntityKey
        {
            var hs = GetOrCreate(resolver);

            context.AddInitializationStepHandler(c => hs.ProcessSenseMap(registry));
            context.AddFixedStepHandlers(c => hs.ProcessSenseMap(registry));
        }

        void RegisterApplySenseMapSystem<TGameContext, TItemId>(IServiceResolver resolver,
                                                                      IGameLoopSystemRegistration<TGameContext> context,
                                                                      EntityRegistry<TItemId> registry,
                                                                      ICommandHandlerRegistration<TGameContext, TItemId> handler)
            where TItemId : IEntityKey
        {
            var hs = GetOrCreate(resolver);

            var system = registry.BuildSystem()
                    .WithContext<TGameContext>()
                    .CreateSystem<SensoryReceptorState<VisionSense>,
                        SenseReceptorDirtyFlag<VisionSense>,
                        SingleLevelSenseDirectionMapData<VisionSense>>(hs.ApplyReceptorFieldOfView);
            
            context.AddInitializationStepHandler(system);
            context.AddFixedStepHandlers(system);
        }

        void RegisterPrepareSystem<TGameContext>(IServiceResolver resolver, IGameLoopSystemRegistration<TGameContext> context)
        {
            var hs = GetOrCreate(resolver);

            context.AddInitializationStepHandler(hs.EnsureSenseCacheAvailable);
            context.AddDisposeStepHandler(hs.ShutDown);
        }

        HeatMapSystem GetOrCreate(IServiceResolver resolver)
        {
            if (resolver.TryResolve(out HeatMapSystem system))
            {
                return system;
            }

            if (!resolver.TryResolve(out ISenseDataBlitter blitter))
            {
                blitter = new DefaultSenseDataBlitter();
            }

            system = new HeatMapSystem(resolver.ResolveToReference<ISenseStateCacheProvider>(),
                                       resolver.Resolve<IHeatPhysicsConfiguration>(),
                                       blitter);
            return system;
        }
    }
}