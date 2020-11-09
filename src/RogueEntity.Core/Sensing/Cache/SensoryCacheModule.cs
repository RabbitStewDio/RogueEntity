using EnTTSharp.Entities;
using RogueEntity.Core.Infrastructure.Commands;
using RogueEntity.Core.Infrastructure.GameLoops;
using RogueEntity.Core.Infrastructure.Modules;
using RogueEntity.Core.Infrastructure.Modules.Attributes;
using RogueEntity.Core.Infrastructure.Modules.Services;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Grid;

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
        protected void InitializeSenseCacheSystem<TGameContext>(IServiceResolver resolver,
                                                                IModuleInitializer<TGameContext> moduleInitializer)
        {
            moduleInitializer.Register(SenseCacheResetId, 100, RegisterSenseCacheSystem);
        }

        [EntityRoleInitializer("Role.Core.Senses.Cache.InvalidationSource",
                               ConditionalRoles = new[] {"Role.Core.Position.GridPositioned"})]
        protected void InitializeRole<TGameContext, TItemId>(IServiceResolver serviceResolver,
                                                             IModuleInitializer<TGameContext> initializer,
                                                             EntityRole role)
            where TItemId : IEntityKey
            where TGameContext : IGridMapContext<TItemId>
        {
            var ctx = initializer.DeclareEntityContext<TItemId>();
            ctx.Register(SenseCacheLifecycleId, 0, RegisterSenseCacheLifeCycle);
        }

        void RegisterSenseCacheLifeCycle<TGameContext, TItemId>(IServiceResolver resolver,
                                                                IGameLoopSystemRegistration<TGameContext> context,
                                                                EntityRegistry<TItemId> registry,
                                                                ICommandHandlerRegistration<TGameContext, TItemId> handler)
            where TGameContext : IGridMapContext<TItemId>
            where TItemId : IEntityKey
        {
            if (!resolver.TryResolve(out SenseCacheSetUpSystem<TGameContext> system))
            {
                system = new SenseCacheSetUpSystem<TGameContext>(resolver.ResolveToReference<SenseStateCache>());
                resolver.Store(system);
                resolver.Store<ISenseCacheSetupSystem>(system);

                context.AddInitializationStepHandler(system.Start);
                context.AddDisposeStepHandler(system.Stop);
            }
        }

        void RegisterSenseCacheSystem<TGameContext>(IServiceResolver resolver, IGameLoopSystemRegistration<TGameContext> context)
        {
            if (!resolver.TryResolve(out SenseStateCache cache))
            {
                cache = new SenseStateCache(4, 64, 64);
                resolver.Store(cache);
                resolver.Store<ISenseStateCacheProvider>(cache);
                resolver.Store<IGlobalSenseStateCacheProvider>(cache);
            }

            context.AddInitializationStepHandler(c => cache.MarkClean());
            context.AddFixedStepHandlers(c => cache.MarkClean());
        }
    }
}