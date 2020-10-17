using EnTTSharp.Entities;
using EnTTSharp.Entities.Systems;
using RogueEntity.Core.Infrastructure.Commands;
using RogueEntity.Core.Infrastructure.GameLoops;
using RogueEntity.Core.Infrastructure.Modules;
using RogueEntity.Core.Infrastructure.Time;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Continuous;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Sensing.Cache;
using RogueEntity.Core.Sensing.Common;
using RogueEntity.Core.Sensing.Common.Blitter;
using RogueEntity.Core.Sensing.Resistance;
using RogueEntity.Core.Sensing.Resistance.Maps;

namespace RogueEntity.Core.Sensing.Sources.Smell
{
    public class SmellSourceModule: ModuleBase
    {
       public static readonly string ModuleId = "Core.Senses.Source.Smell";
        public static readonly EntityRole HeatSourceRole = new EntityRole("Role.Core.Senses.Source.Smell");

        public static readonly EntitySystemId PreparationSystemId = "Systems.Core.Senses.Source.Smell.Prepare";
        public static readonly EntitySystemId CollectionGridSystemId = "Systems.Core.Senses.Source.Smell.Collect.Grid";
        public static readonly EntitySystemId CollectionContinuousSystemId = "Systems.Core.Senses.Source.Smell.Collect.Continuous";
        public static readonly EntitySystemId ComputeSystemId = "Systems.Core.Senses.Source.Smell.Compute";
        public static readonly EntitySystemId FinalizeSystemId = "Systems.Core.Senses.Source.Smell.Finalize";

        public static readonly EntitySystemId RegisterEntityId = "Entities.Core.Senses.Source.Smell";

        public SmellSourceModule()
        {
            Id = ModuleId;

            DeclareDependencies(ModuleDependency.Of(SensoryResistanceModule.ModuleId),
                                ModuleDependency.Of(SensoryCacheModule.ModuleId),
                                ModuleDependency.Of(PositionModule.ModuleId));

            RequireRole(HeatSourceRole);
        }

        [EntityRoleInitializer("Role.Core.Senses.Source.Smell")]
        protected void InitializeRole<TGameContext, TItemId>(IServiceResolver serviceResolver,
                                                             IModuleInitializer<TGameContext> initializer,
                                                             EntityRole role)
            where TItemId : IEntityKey
            where TGameContext : ITimeContext
        {
            var ctx = initializer.DeclareEntityContext<TItemId>();
            ctx.Register(RegisterEntityId, 0, RegisterEntities);
            ctx.Register(PreparationSystemId, 5000, RegisterPrepareSystem);
            ctx.Register(ComputeSystemId, 5800, RegisterCalculateSystem);
            ctx.Register(FinalizeSystemId, 5900, RegisterCleanUpSystem);
        }

        [EntityRoleInitializer("Role.Core.Senses.Source.Smell",
                               ConditionalRoles = new[] {"Role.Core.Position.GridPositioned"})]
        protected void InitializeLightCollectionGrid<TGameContext, TItemId>(IServiceResolver serviceResolver,
                                                                            IModuleInitializer<TGameContext> initializer,
                                                                            EntityRole role)
            where TItemId : IEntityKey
        {
            var ctx = initializer.DeclareEntityContext<TItemId>();
            ctx.Register(CollectionGridSystemId, 5700, RegisterCollectLightsGridSystem);
        }

        [EntityRoleInitializer("Role.Core.Senses.Source.Smell",
                               ConditionalRoles = new[] {"Role.Core.Position.ContinuousPositioned"})]
        protected void InitializeLightCollectionContinuous<TGameContext, TItemId>(IServiceResolver serviceResolver,
                                                                                  IModuleInitializer<TGameContext> initializer,
                                                                                  EntityRole role)
            where TItemId : IEntityKey
        {
            var ctx = initializer.DeclareEntityContext<TItemId>();
            ctx.Register(CollectionGridSystemId, 5700, RegisterCollectLightsContinuousSystem);
        }

        void RegisterPrepareSystem<TGameContext, TItemId>(IServiceResolver serviceResolver,
                                                               IGameLoopSystemRegistration<TGameContext> context,
                                                               EntityRegistry<TItemId> registry,
                                                               ICommandHandlerRegistration<TGameContext, TItemId> handler)
            where TItemId : IEntityKey
            where TGameContext : ITimeContext
        {
            if (!GetOrCreateLightSystem(serviceResolver, out var ls))
            {
                return;
            }

            context.AddInitializationStepHandler(ls.EnsureSenseCacheAvailable);
            context.AddInitializationStepHandler(ls.BeginSenseCalculation);
            context.AddFixedStepHandlers(ls.BeginSenseCalculation);
        }

        void RegisterCollectLightsGridSystem<TGameContext, TItemId>(IServiceResolver resolver,
                                                                    IGameLoopSystemRegistration<TGameContext> context,
                                                                    EntityRegistry<TItemId> registry,
                                                                    ICommandHandlerRegistration<TGameContext, TItemId> handler)
            where TItemId : IEntityKey
        {
            if (!GetOrCreateLightSystem(resolver, out var ls))
            {
                return;
            }

            var system = registry.BuildSystem().WithContext<TGameContext>().CreateSystem<SmellSourceDefinition, SenseSourceState<SmellSense>, ContinuousMapPosition>(ls.FindDirtySenseSources);
            context.AddInitializationStepHandler(system);
            context.AddFixedStepHandlers(system);
        }

        void RegisterCollectLightsContinuousSystem<TGameContext, TItemId>(IServiceResolver resolver,
                                                                          IGameLoopSystemRegistration<TGameContext> context,
                                                                          EntityRegistry<TItemId> registry,
                                                                          ICommandHandlerRegistration<TGameContext, TItemId> handler)
            where TItemId : IEntityKey
        {
            if (!GetOrCreateLightSystem(resolver, out var ls))
            {
                return;
            }

            var system = registry.BuildSystem()
                                 .WithContext<TGameContext>()
                                 .CreateSystem<SmellSourceDefinition, SenseSourceState<SmellSense>, EntityGridPosition>(ls.FindDirtySenseSources);
            context.AddInitializationStepHandler(system);
            context.AddFixedStepHandlers(system);
        }

        void RegisterCalculateSystem<TGameContext, TItemId>(IServiceResolver serviceResolver,
                                                                 IGameLoopSystemRegistration<TGameContext> context,
                                                                 EntityRegistry<TItemId> registry,
                                                                 ICommandHandlerRegistration<TGameContext, TItemId> handler)
            where TItemId : IEntityKey
        {
            if (!GetOrCreateLightSystem(serviceResolver, out var ls))
            {
                return;
            }

            var refreshLocalSenseState =
                registry.BuildSystem()
                        .WithContext<TGameContext>()
                        .CreateSystem<SmellSourceDefinition, SenseSourceState<SmellSense>, SenseDirtyFlag<SmellSense>>(ls.RefreshLocalSenseState);
            
            context.AddInitializationStepHandler(refreshLocalSenseState);
            context.AddFixedStepHandlers(refreshLocalSenseState);
        }

        void RegisterCleanUpSystem<TGameContext, TItemId>(IServiceResolver serviceResolver,
                                                          IGameLoopSystemRegistration<TGameContext> context,
                                                          EntityRegistry<TItemId> registry,
                                                          ICommandHandlerRegistration<TGameContext, TItemId> handler)
            where TItemId : IEntityKey
        {
            if (!GetOrCreateLightSystem(serviceResolver, out var ls))
            {
                return;
            }

            context.AddInitializationStepHandler(ls.EndSenseCalculation);
            context.AddFixedStepHandlers(ls.EndSenseCalculation);
            context.AddDisposeStepHandler(ls.ShutDown);
        }

        static bool GetOrCreateLightSystem(IServiceResolver serviceResolver, out SmellSystem ls)
        {
            if (!serviceResolver.TryResolve(out ISenseDataBlitter senseBlitterFactory) ||
                !serviceResolver.TryResolve(out ISmellPhysicsConfiguration physicsConfig))
            {
                ls = default;
                return false;
            }

            if (!serviceResolver.TryResolve(out ls))
            {
                ls = new SmellSystem(serviceResolver.ResolveToReference<ISensePropertiesSource>(),
                                    serviceResolver.ResolveToReference<ISenseStateCacheProvider>(),
                                    physicsConfig.CreateSmellPropagationAlgorithm(),
                                    physicsConfig.SmellPhysics,
                                    senseBlitterFactory);
            }

            return true;
        }

        void RegisterEntities<TItemId>(IServiceResolver serviceResolver, EntityRegistry<TItemId> registry)
            where TItemId : IEntityKey
        {
            registry.RegisterNonConstructable<SenseDirtyFlag<SmellSense>>();
            registry.RegisterNonConstructable<SmellSourceDefinition>();
            registry.RegisterNonConstructable<SenseSourceState<SmellSense>>();
        }
    }
}