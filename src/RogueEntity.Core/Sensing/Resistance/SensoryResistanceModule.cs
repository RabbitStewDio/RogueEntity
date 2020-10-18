using EnTTSharp.Entities;
using RogueEntity.Core.Infrastructure.Commands;
using RogueEntity.Core.Infrastructure.GameLoops;
using RogueEntity.Core.Infrastructure.Modules;
using RogueEntity.Core.Meta;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Sensing.Cache;
using RogueEntity.Core.Sensing.Resistance.Maps;
using RogueEntity.Core.Utils.MapChunks;

namespace RogueEntity.Core.Sensing.Resistance
{
    [Module]
    public class SensoryResistanceModule : ModuleBase
    {
        public static readonly string ModuleId = "Core.Sense.Resistance";

        public static readonly EntitySystemId RegisterEntitiesId = "Core.Entities.Senses.Resistance";
        public static readonly EntitySystemId RegisterSystem = "Core.Systems.Senses.Resistance.SetUp";
        public static readonly EntitySystemId ExecuteSystem = "Core.Systems.Senses.Resistance.Run";

        public static readonly EntityRole ResistanceDataProviderRole = new EntityRole("Role.Core.Senses.Resistance.ResistanceDataProvider");

        public SensoryResistanceModule()
        {
            Id = ModuleId;
            Author = "RogueEntity.Core";
            Name = "RogueEntity Core Module - Senses - Sensory Resistance";
            Description = "Provides support for declaring sensory resistance on items and aggregating this information on a map.";
            IsFrameworkModule = true;

            DeclareDependencies(ModuleDependency.Of(CoreModule.ModuleId));

            RequireRole(ResistanceDataProviderRole);
            RequireRole(ResistanceDataProviderRole).WithImpliedRole(SensoryCacheModule.SenseCacheSourceRole);
        }

        [EntityRoleInitializer("Role.Core.Senses.Resistance.ResistanceDataProvider")]
        protected void InitializeRole<TGameContext, TItemId>(IServiceResolver serviceResolver,
                                                             IModuleInitializer<TGameContext> initializer,
                                                             EntityRole role)
            where TItemId : IEntityKey
            where TGameContext : IMapBoundsContext
        {
            var ctx = initializer.DeclareEntityContext<TItemId>();
            ctx.Register(RegisterEntitiesId, 0, RegisterEntities);
            ctx.Register(ExecuteSystem, 5400, RegisterResistanceSystemExecution);
            ctx.Register(RegisterSystem, 500, RegisterResistanceSystemLifecycle);
        }

        void RegisterResistanceSystemLifecycle<TGameContext, TItemId>(IServiceResolver resolver, 
                                                                      IGameLoopSystemRegistration<TGameContext> context, 
                                                                      EntityRegistry<TItemId> registry, 
                                                                      ICommandHandlerRegistration<TGameContext, TItemId> handler)
            where TGameContext : IMapBoundsContext
            where TItemId : IEntityKey
        {
            var system = GetOrCreateSensePropertiesSystem<TGameContext>(resolver);
            context.AddInitializationStepHandler(system.Start);            
            context.AddDisposeStepHandler(system.Stop);            
        }


        void RegisterResistanceSystemExecution<TGameContext, TActorId>(IServiceResolver serviceResolver,
                                                                       IGameLoopSystemRegistration<TGameContext> context,
                                                                       EntityRegistry<TActorId> registry,
                                                                       ICommandHandlerRegistration<TGameContext, TActorId> handler)
            where TActorId : IEntityKey
        {
            var system = GetOrCreateSensePropertiesSystem<TGameContext>(serviceResolver);

            context.AddInitializationStepHandler(system.ProcessSenseProperties);
            context.AddFixedStepHandlers(system.ProcessSenseProperties);
        }

        static SensePropertiesSystem<TGameContext> GetOrCreateSensePropertiesSystem<TGameContext>(IServiceResolver serviceResolver)
        {
            if (serviceResolver.TryResolve(out SensePropertiesSystem<TGameContext> system))
            {
                return system;
            }

            if (!serviceResolver.TryResolve(out IAddByteBlitter blitter))
            {
                blitter = new DefaultAddByteBlitter();
            }
            
            system = new SensePropertiesSystem<TGameContext>(blitter);
            serviceResolver.Store(system);
            serviceResolver.Store<ISensePropertiesSystem<TGameContext>>(system);
            serviceResolver.Store<ISensePropertiesSource>(system);

            return system;
        }

        void RegisterEntities<TActorId>(IServiceResolver serviceResolver,
                                        EntityRegistry<TActorId> registry)
            where TActorId : IEntityKey
        {
            registry.RegisterNonConstructable<SensoryResistance>();
        }
    }
}