using EnTTSharp.Entities;
using RogueEntity.Api.GameLoops;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Modules;
using RogueEntity.Api.Modules.Attributes;
using RogueEntity.Core.Positioning;

namespace RogueEntity.Core.Sensing.Cache
{
    [Module]
    public class SensoryCacheModule : ModuleBase
    {
        public static readonly string ModuleId = "Core.Senses.Cache";
        public static readonly EntitySystemId SenseCacheLifecycleId = "Systems.Core.Senses.Cache.LifeCycle";
        public static readonly EntitySystemId SenseCacheResetId = "Systems.Core.Senses.Cache.Reset";

        public static readonly EntityRole SenseCacheSourceRole = new EntityRole("Role.Core.Senses.Cache.InvalidationSource");

        public SensoryCacheModule()
        {
            Id = ModuleId;
            Author = "RogueEntity.Core";
            Name = "RogueEntity Core Module - Sensory Change Tracker";
            Description = "Tracks map changes that require sense sources to recompute their effects.";
            IsFrameworkModule = true;

            DeclareDependencies(ModuleDependency.Of(PositionModule.ModuleId));

            RequireRole(SenseCacheSourceRole);
        }

        [ModuleInitializer]
        protected void InitializeSenseCacheSystem(in ModuleInitializationParameter initParameter,
                                                  IModuleInitializer moduleInitializer)
        {
            moduleInitializer.Register(SenseCacheResetId, 100, RegisterSenseCacheSystem);
        }

        [EntityRoleInitializer("Role.Core.Senses.Cache.InvalidationSource",
                               ConditionalRoles = new[] {"Role.Core.Position.GridPositioned"})]
        protected void InitializeRole<TItemId>(in ModuleEntityInitializationParameter<TItemId> initParameter,
                                               IModuleInitializer initializer,
                                               EntityRole role)
            where TItemId : IEntityKey
        {
            var ctx = initializer.DeclareEntityContext<TItemId>();
            ctx.Register(SenseCacheLifecycleId, 0, RegisterSenseCacheLifeCycle);
        }

        void RegisterSenseCacheLifeCycle<TItemId>(in ModuleEntityInitializationParameter<TItemId> initParameter,
                                                  IGameLoopSystemRegistration context,
                                                  EntityRegistry<TItemId> registry)
            where TItemId : IEntityKey
        {
            var resolver = initParameter.ServiceResolver;
            if (!resolver.TryResolve(out SenseCacheSetUpSystem system))
            {
                system = new SenseCacheSetUpSystem(resolver.ResolveToReference<SenseStateCache>());
                resolver.Store(system);
                resolver.Store<ISenseCacheSetupSystem>(system);

                context.AddInitializationStepHandler(system.Start, nameof(system.Start));
                context.AddDisposeStepHandler(system.Stop, nameof(system.Stop));
            }
        }

        void RegisterSenseCacheSystem(in ModuleInitializationParameter initParameter,
                                      IGameLoopSystemRegistration context)
        {
            var resolver = initParameter.ServiceResolver;
            if (!resolver.TryResolve(out SenseStateCache cache))
            {
                cache = new SenseStateCache(4, 64, 64);
                resolver.Store(cache);
                resolver.Store<ISenseStateCacheControl>(cache);
                resolver.Store<ISenseStateCacheProvider>(cache);
                resolver.Store<IGlobalSenseStateCacheProvider>(cache);
            }

            context.AddInitializationStepHandler(cache.MarkClean);
            context.AddFixedStepHandlers(cache.MarkClean);
        }
    }
}
