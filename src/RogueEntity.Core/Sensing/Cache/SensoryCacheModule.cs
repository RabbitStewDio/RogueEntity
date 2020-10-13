using EnTTSharp.Entities;
using RogueEntity.Core.Infrastructure.Commands;
using RogueEntity.Core.Infrastructure.GameLoops;
using RogueEntity.Core.Infrastructure.Modules;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Sensing.Resistance;
using RogueEntity.Core.Sensing.Sources.Light;

namespace RogueEntity.Core.Sensing.Cache
{
    [Module]
    public class SensoryCacheModule: ModuleBase
    {
        public static readonly string ModuleId = "Core.Senses.Cache";
        public static readonly EntityRole SenseCacheSourceRole = new EntityRole("Role.Core.Senses.Cache.Source");

        public SensoryCacheModule()
        {
            Id = ModuleId;
            Author = "RogueEntity.Core";
            Name = "RogueEntity Core Module - Sensory Change Tracker";
            Description = "Tracks map changes that require sense sources to recompute their effects.";
            IsFrameworkModule = true;
            
            DeclareDependencies(ModuleDependency.Of(PositionModule.ModuleId));

            RequireRole(SenseCacheSourceRole);
            
            RequireRole(SensoryResistanceModule.ResistanceDataProviderRole).WithImpliedRole(SenseCacheSourceRole);
            RequireRole(LightSenseModule.LightSourceRole).WithImpliedRole(SenseCacheSourceRole);
        }
      
        [ModuleInitializer]
        protected void InitializeSenseCacheSystem<TGameContext>(IServiceResolver resolver,
                                                                IModuleInitializer<TGameContext> moduleInitializer)
        {
            moduleInitializer.Register("cache", 1, RegisterSenseCacheSystem);
        }

        [EntityRoleInitializer("Role.Core.Senses.Cache.Source")]
        protected void InitializeRole<TGameContext, TItemId>(IServiceResolver serviceResolver,
                                                             IModuleInitializer<TGameContext> initializer,
                                                             EntityRole role)
            where TItemId : IEntityKey
            where TGameContext : IGridMapContext<TGameContext, TItemId>
        {
            var ctx = initializer.DeclareEntityContext<TItemId>();
            ctx.Register("RegisterEntitiesId", 0, RegisterSenseCacheLifeCycle);
        }

        void RegisterSenseCacheLifeCycle<TGameContext, TItemId>(IServiceResolver resolver, 
                                                                IGameLoopSystemRegistration<TGameContext> context, 
                                                                EntityRegistry<TItemId> registry, 
                                                                ICommandHandlerRegistration<TGameContext, TItemId> handler)
            where TGameContext : IGridMapContext<TGameContext, TItemId>
            where TItemId : IEntityKey
        {
            var system = new SenseCacheSetUpSystem<TGameContext, TItemId>(resolver.ResolveToReference<SenseStateCacheProvider>());
            context.AddInitializationStepHandler(system.Start);
            context.AddDisposeStepHandler(system.Stop);
        }

        void RegisterSenseCacheSystem<TGameContext>(IServiceResolver resolver, IGameLoopSystemRegistration<TGameContext> context)
        {
            if (resolver.TryResolve(out SenseStateCacheProvider cache))
            {
                cache = new SenseStateCacheProvider(16);
                resolver.Store(cache);
                resolver.Store<ISenseStateCacheProvider>(cache);
            }

            context.AddFixedStepHandlers(c => cache.MarkClean());
        }
    }
}