using EnTTSharp.Entities;
using RogueEntity.Core.Infrastructure.Commands;
using RogueEntity.Core.Infrastructure.GameLoops;
using RogueEntity.Core.Infrastructure.Modules;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Sensing.Resistance;
using RogueEntity.Core.Sensing.Resistance.Maps;

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

            DeclareDependencies(ModuleDependency.Of(PositionModule.ModuleId),
                                ModuleDependency.Of(SensoryResistanceModule.ModuleId));

            RequireRole(SenseCacheSourceRole);
        }

        [ModuleInitializer]
        protected void InitializeSenseCacheSystem<TGameContext>(IServiceResolver resolver,
                                                                IModuleInitializer<TGameContext> moduleInitializer)
        {
            moduleInitializer.Register(SenseCacheResetId, 1, RegisterSenseCacheSystem);
        }

        [EntityRoleInitializer("Role.Core.Senses.Cache.InvalidationSource",
                               ConditionalRoles = new[] {"Role.Core.Position.GridPositioned"})]
        protected void InitializeRole<TGameContext, TItemId>(IServiceResolver serviceResolver,
                                                             IModuleInitializer<TGameContext> initializer,
                                                             EntityRole role)
            where TItemId : IEntityKey
            where TGameContext : IGridMapContext<TGameContext, TItemId>
        {
            var ctx = initializer.DeclareEntityContext<TItemId>();
            ctx.Register(SenseCacheLifecycleId, 0, RegisterSenseCacheLifeCycle);
        }

        /// <summary>
        ///   Any entity that can invalidate the sense-resistance map implicitly also invalidates the sense cache at
        ///   the same position.
        /// </summary>
        /// <param name="serviceResolver"></param>
        /// <param name="initializer"></param>
        /// <param name="role"></param>
        /// <typeparam name="TGameContext"></typeparam>
        /// <typeparam name="TItemId"></typeparam>
        [EntityRoleInitializer("Role.Core.Senses.Resistance.ResistanceDataProvider")]
        protected void InitializeResistanceProviderCacheSources<TGameContext, TItemId>(IServiceResolver serviceResolver,
                                                                                       IModuleInitializer<TGameContext> initializer,
                                                                                       EntityRole role)
            where TItemId : IEntityKey
        {
            var ctx = initializer.DeclareEntityContext<TItemId>();
            ctx.Register(SenseCacheLifecycleId, 0, RegisterSenseResistanceCacheLifeCycle);
        }

        void RegisterSenseResistanceCacheLifeCycle<TGameContext, TItemId>(IServiceResolver resolver,
                                                                          IGameLoopSystemRegistration<TGameContext> context,
                                                                          EntityRegistry<TItemId> registry,
                                                                          ICommandHandlerRegistration<TGameContext, TItemId> handler)
            where TItemId : IEntityKey
        {
            
            // Ensure that the connection is only made once. The sense properties system will
            // forward all invalidation events to the sense cache.
            if (!resolver.TryResolve(out SensePropertiesConnectorSystem<TGameContext> system))
            {
                var sensePropertiesSystem = resolver.Resolve<SensePropertiesSystem<TGameContext>>();
                var cacheProvider = resolver.Resolve<SenseStateCache>();
                system = new SensePropertiesConnectorSystem<TGameContext>(sensePropertiesSystem, cacheProvider);
                resolver.Store(system);
                
                context.AddInitializationStepHandler(system.Start);
                context.AddDisposeStepHandler(system.Stop);
            }
            
        }

        void RegisterSenseCacheLifeCycle<TGameContext, TItemId>(IServiceResolver resolver,
                                                                IGameLoopSystemRegistration<TGameContext> context,
                                                                EntityRegistry<TItemId> registry,
                                                                ICommandHandlerRegistration<TGameContext, TItemId> handler)
            where TGameContext : IGridMapContext<TGameContext, TItemId>
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
                cache = new SenseStateCache(64, 64, 16);
                resolver.Store(cache);
                resolver.Store<ISenseStateCacheProvider>(cache);
                resolver.Store<IGlobalSenseStateCacheProvider>(cache);
            }

            context.AddInitializationStepHandler(c => cache.MarkClean());
            context.AddFixedStepHandlers(c => cache.MarkClean());
        }
    }
}