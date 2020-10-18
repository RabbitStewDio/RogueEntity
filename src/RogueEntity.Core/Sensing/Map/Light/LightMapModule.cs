using EnTTSharp.Entities;
using EnTTSharp.Entities.Systems;
using RogueEntity.Core.Infrastructure.Commands;
using RogueEntity.Core.Infrastructure.GameLoops;
using RogueEntity.Core.Infrastructure.Modules;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Sensing.Cache;
using RogueEntity.Core.Sensing.Common.Blitter;
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
        public static readonly EntitySystemId ReceptorComputeSystemId = "Systems.Core.Senses.Map.Light.Compute";

        public LightMapModule()
        {
            Id = ModuleId;

            Author = "RogueEntity.Core";
            Name = "RogueEntity Core Module - Senses - Maps";
            Description = "Computes a global sense map.";
            IsFrameworkModule = true;

            DeclareDependencies(ModuleDependency.Of(PositionModule.ModuleId),
                                ModuleDependency.Of(SensoryResistanceModule.ModuleId),
                                ModuleDependency.Of(LightSourceModule.ModuleId),
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
            ctx.Register(ReceptorComputeSystemId, 5750, RegisterCollectSenseSourcesSystem);
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
            
            context.AddInitializationStepHandler(c => hs.ProcessSenseMap(registry));
            context.AddFixedStepHandlers(c => hs.ProcessSenseMap(registry));
        }

        void RegisterPrepareSystem<TGameContext>(IServiceResolver resolver, IGameLoopSystemRegistration<TGameContext> context)
        {
            var hs = GetOrCreate(resolver);

            context.AddInitializationStepHandler(hs.EnsureSenseCacheAvailable);
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

            system = new LightMapSystem(resolver.ResolveToReference<ISenseStateCacheProvider>(), blitter);
            return system;
        }
    }
}