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

namespace RogueEntity.Core.Sensing.Sources.Touch
{
    public class TouchSourceModule: ModuleBase
    {
       public static readonly string ModuleId = "Core.Senses.Source.Touch";
        public static readonly EntityRole SmellSourceRole = new EntityRole("Role.Core.Senses.Source.Touch");

        public static readonly EntitySystemId PreparationSystemId = "Systems.Core.Senses.Source.Touch.Prepare";
        public static readonly EntitySystemId CollectionGridSystemId = "Systems.Core.Senses.Source.Touch.Collect.Grid";
        public static readonly EntitySystemId CollectionContinuousSystemId = "Systems.Core.Senses.Touch.Smell.Collect.Continuous";
        public static readonly EntitySystemId ComputeSystemId = "Systems.Core.Senses.Source.Touch.Compute";
        public static readonly EntitySystemId FinalizeSystemId = "Systems.Core.Senses.Source.Touch.Finalize";

        public static readonly EntitySystemId RegisterEntityId = "Entities.Core.Senses.Source.Touch";

        public TouchSourceModule()
        {
            Id = ModuleId;

            DeclareDependencies(ModuleDependency.Of(SensoryResistanceModule.ModuleId),
                                ModuleDependency.Of(SensoryCacheModule.ModuleId),
                                ModuleDependency.Of(PositionModule.ModuleId));

            RequireRole(SmellSourceRole).WithImpliedRole(SenseSources.SenseSourceRole).WithImpliedRole(SensoryCacheModule.SenseCacheSourceRole);;
        }

        [EntityRoleInitializer("Role.Core.Senses.Source.Touch")]
        protected void InitializeRole<TGameContext, TItemId>(IServiceResolver serviceResolver,
                                                             IModuleInitializer<TGameContext> initializer,
                                                             EntityRole role)
            where TItemId : IEntityKey
            where TGameContext : ITimeContext
        {
            var ctx = initializer.DeclareEntityContext<TItemId>();
            ctx.Register(RegisterEntityId, 0, RegisterEntities);
            ctx.Register(PreparationSystemId, 50000, RegisterPrepareSystem);
            ctx.Register(ComputeSystemId, 58000, RegisterCalculateSystem);
            ctx.Register(FinalizeSystemId, 59000, RegisterCleanUpSystem);
        }

        [EntityRoleInitializer("Role.Core.Senses.Source.Touch",
                               ConditionalRoles = new[] {"Role.Core.Position.GridPositioned"})]
        protected void InitializeLightCollectionGrid<TGameContext, TItemId>(IServiceResolver serviceResolver,
                                                                            IModuleInitializer<TGameContext> initializer,
                                                                            EntityRole role)
            where TItemId : IEntityKey
        {
            var ctx = initializer.DeclareEntityContext<TItemId>();
            ctx.Register(CollectionGridSystemId, 57000, RegisterCollectLightsGridSystem);
        }

        [EntityRoleInitializer("Role.Core.Senses.Source.Touch",
                               ConditionalRoles = new[] {"Role.Core.Position.ContinuousPositioned"})]
        protected void InitializeLightCollectionContinuous<TGameContext, TItemId>(IServiceResolver serviceResolver,
                                                                                  IModuleInitializer<TGameContext> initializer,
                                                                                  EntityRole role)
            where TItemId : IEntityKey
        {
            var ctx = initializer.DeclareEntityContext<TItemId>();
            ctx.Register(CollectionGridSystemId, 57000, RegisterCollectLightsContinuousSystem);
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

            var system = registry.BuildSystem().WithContext<TGameContext>().CreateSystem<TouchSourceDefinition, SenseSourceState<TouchSense>, ContinuousMapPosition>(ls.FindDirtySenseSources);
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
                                 .CreateSystem<TouchSourceDefinition, SenseSourceState<TouchSense>, EntityGridPosition>(ls.FindDirtySenseSources);
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
                        .CreateSystem<TouchSourceDefinition, SenseSourceState<TouchSense>, SenseDirtyFlag<TouchSense>, ObservedSenseSource<TouchSense>>(ls.RefreshLocalSenseState);
            
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

        static bool GetOrCreateLightSystem(IServiceResolver serviceResolver, out TouchSystem ls)
        {
            if (!serviceResolver.TryResolve(out ISenseDataBlitter senseBlitterFactory) ||
                !serviceResolver.TryResolve(out ITouchPhysicsConfiguration physicsConfig))
            {
                ls = default;
                return false;
            }

            if (!serviceResolver.TryResolve(out ls))
            {
                ls = new TouchSystem(serviceResolver.ResolveToReference<ISensePropertiesSource>(),
                                     serviceResolver.ResolveToReference<ISenseStateCacheProvider>(),
                                     physicsConfig.CreateTouchPropagationAlgorithm(),
                                     physicsConfig.TouchPhysics,
                                     senseBlitterFactory);
            }

            return true;
        }

        void RegisterEntities<TItemId>(IServiceResolver serviceResolver, EntityRegistry<TItemId> registry)
            where TItemId : IEntityKey
        {
            registry.RegisterNonConstructable<TouchSourceDefinition>();
            registry.RegisterNonConstructable<SenseSourceState<TouchSense>>();
            registry.RegisterFlag<ObservedSenseSource<TouchSense>>();
            registry.RegisterFlag<SenseDirtyFlag<TouchSense>>();
        }
    }
}