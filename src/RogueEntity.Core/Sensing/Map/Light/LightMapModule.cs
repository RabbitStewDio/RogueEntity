using EnTTSharp.Entities;
using EnTTSharp.Entities.Systems;
using RogueEntity.Core.Infrastructure.Commands;
using RogueEntity.Core.Infrastructure.GameLoops;
using RogueEntity.Core.Infrastructure.Modules;
using RogueEntity.Core.Infrastructure.Time;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Sensing.Cache;
using RogueEntity.Core.Sensing.Common.Blitter;
using RogueEntity.Core.Sensing.Receptors;
using RogueEntity.Core.Sensing.Receptors.Light;
using RogueEntity.Core.Sensing.Resistance;
using RogueEntity.Core.Sensing.Sources;
using RogueEntity.Core.Sensing.Sources.Light;

namespace RogueEntity.Core.Sensing.Map.Light
{
    public class LightMapModule: ModuleBase
    {
        public const string ModuleId = "Core.Sense.Map.Light";

        public static readonly EntitySystemId RegisterEntityId = "Entities.Core.Senses.Map.Light";

        public static readonly EntitySystemId ReceptorPreparationSystemId = "Systems.Core.Senses.Map.Light.Prepare";
        public static readonly EntitySystemId ReceptorCollectSystemId = "Systems.Core.Senses.Map.Light.Collect";
        public static readonly EntitySystemId ReceptorComputeSystemId = "Systems.Core.Senses.Map.Light.Compute";
        public static readonly EntitySystemId ReceptorApplySystemId = "Systems.Core.Senses.Map.Light.ApplyReceptorFieldOfView";

        public LightMapModule()
        {
            Id = ModuleId;

            Author = "RogueEntity.Core";
            Name = "RogueEntity Core Module - Senses - Maps - Vision";
            Description = "Computes a global sense map.";
            IsFrameworkModule = true;

            DeclareDependencies(ModuleDependency.Of(PositionModule.ModuleId),
                                ModuleDependency.Of(SensoryResistanceModule.ModuleId),
                                ModuleDependency.Of(LightSourceModule.ModuleId),
                                ModuleDependency.Of(VisionSenseModule.ModuleId),
                                ModuleDependency.Of(SensoryCacheModule.ModuleId));
        }

        [ModuleInitializer]
        protected void InitializeSenseCacheSystem<TGameContext>(IServiceResolver resolver,
                                                                IModuleInitializer<TGameContext> moduleInitializer)
        {
            moduleInitializer.Register(ReceptorPreparationSystemId, 1, RegisterPrepareSystem);
        }

        [EntityRoleInitializer("Role.Core.Senses.Source.Light")]
        protected void InitializeSenseCollection<TGameContext, TItemId>(IServiceResolver serviceResolver,
                                                                        IModuleInitializer<TGameContext> initializer,
                                                                        EntityRole role)
            where TItemId : IEntityKey
        {
            var ctx = initializer.DeclareEntityContext<TItemId>();
            ctx.Register(ReceptorCollectSystemId, 57500, RegisterCollectSenseSourcesSystem);
            ctx.Register(ReceptorComputeSystemId, 58500, RegisterComputeSenseMapSystem);
            ctx.Register(ReceptorApplySystemId, 59000, RegisterApplySenseMapSystem);
            
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
                        .CreateSystem<LightSourceDefinition, SenseSourceState<VisionSense>>(hs.CollectSenseSources);
            
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
                    .CreateSystem<SensoryReceptorState<VisionSense, VisionSense>,
                        SenseReceptorDirtyFlag<VisionSense, VisionSense>,
                        SingleLevelSenseDirectionMapData<VisionSense, VisionSense>>(hs.ApplyReceptorFieldOfView);
            
            context.AddInitializationStepHandler(system);
            context.AddFixedStepHandlers(system);
        }

        void RegisterPrepareSystem<TGameContext>(IServiceResolver resolver, IGameLoopSystemRegistration<TGameContext> context)
        {
            var hs = GetOrCreate(resolver);

            context.AddDisposeStepHandler(hs.ShutDown);
        }

        LightMapSystem GetOrCreate(IServiceResolver resolver)
        {
            if (resolver.TryResolve(out LightMapSystem system))
            {
                return system;
            }

            if (!resolver.TryResolve(out ISenseDataBlitter blitter))
            {
                blitter = new DefaultSenseDataBlitter();
            }

            system = new LightMapSystem(resolver.ResolveToReference<ITimeSource>(),
                                        blitter);
            return system;
        }
    }
}