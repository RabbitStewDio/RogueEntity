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
using RogueEntity.Core.Sensing.Common.Blitter;
using RogueEntity.Core.Sensing.Common.Physics;
using RogueEntity.Core.Sensing.Resistance;
using RogueEntity.Core.Sensing.Resistance.Maps;
using RogueEntity.Core.Sensing.Sources;
using RogueEntity.Core.Sensing.Sources.Touch;

namespace RogueEntity.Core.Sensing.Receptors.Touch
{
    public class TouchSenseModule : ModuleBase
    {
        public const string ModuleId = "Core.Sense.Receptor.Touch";

        public static readonly EntitySystemId RegisterEntityId = "Entities.Core.Senses.Receptor.Touch";

        public static readonly EntitySystemId ReceptorPreparationSystemId = "Systems.Core.Senses.Receptor.Touch.Prepare";
        public static readonly EntitySystemId ReceptorCollectionGridSystemId = "Systems.Core.Senses.Receptor.Touch.Collect.Grid";
        public static readonly EntitySystemId ReceptorCollectionContinuousSystemId = "Systems.Core.Senses.Receptor.Touch.Collect.Continuous";
        public static readonly EntitySystemId SenseSourceCollectionContinuousSystemId = "Systems.Core.Senses.Receptor.Touch.CollectSources.Continuous";
        public static readonly EntitySystemId ReceptorComputeFoVSystemId = "Systems.Core.Senses.Receptor.Touch.ComputeFieldOfView";
        public static readonly EntitySystemId ReceptorComputeSystemId = "Systems.Core.Senses.Receptor.Touch.Compute";
        public static readonly EntitySystemId ReceptorFinalizeSystemId = "Systems.Core.Senses.Receptor.Touch.Finalize";

        public static readonly EntityRole SenseReceptorActorRole = new EntityRole("Role.Core.Senses.Receptor.Touch.ActorRole");

        public TouchSenseModule()
        {
            Id = ModuleId;
            Author = "RogueEntity.Core";
            Name = "RogueEntity Core Module - Senses - Receptors";
            Description = "Provides items and actors with a field of view for a omnidirectional sense of Touch.";
            IsFrameworkModule = true;

            DeclareDependencies(ModuleDependency.Of(PositionModule.ModuleId),
                                ModuleDependency.Of(SensoryResistanceModule.ModuleId),
                                ModuleDependency.Of(SensoryCacheModule.ModuleId),
                                ModuleDependency.Of(TouchSourceModule.ModuleId));

            RequireRole(SenseReceptorActorRole).WithImpliedRole(TouchSourceModule.TouchSourceRole);
        }

        [EntityRoleInitializer("Role.Core.Senses.Receptor.Touch.ActorRole")]
        protected void InitializeRole<TGameContext, TItemId>(IServiceResolver serviceResolver,
                                                             IModuleInitializer<TGameContext> initializer,
                                                             EntityRole role)
            where TItemId : IEntityKey
        {
            var ctx = initializer.DeclareEntityContext<TItemId>();
            ctx.Register(RegisterEntityId, 0, RegisterEntities);
            ctx.Register(ReceptorPreparationSystemId, 50000, RegisterPrepareSystem);
            ctx.Register(ReceptorComputeFoVSystemId, 56000, RegisterComputeReceptorFieldOfView);
            ctx.Register(ReceptorComputeSystemId, 58500, RegisterCalculateSystem);
            ctx.Register(ReceptorFinalizeSystemId, 59500, RegisterFinalizeSystem);
        }

        [EntityRoleInitializer("Role.Core.Senses.Receptor.Touch.ActorRole",
                               ConditionalRoles = new[] {"Role.Core.Position.GridPositioned"})]
        protected void InitializeCollectReceptorsGrid<TGameContext, TItemId>(IServiceResolver serviceResolver,
                                                                             IModuleInitializer<TGameContext> initializer,
                                                                             EntityRole role)
            where TItemId : IEntityKey
        {
            var ctx = initializer.DeclareEntityContext<TItemId>();
            ctx.Register(ReceptorCollectionGridSystemId, 55500, RegisterCollectReceptorsGridSystem);
        }

        [EntityRoleInitializer("Role.Core.Senses.Receptor.Touch.ActorRole",
                               ConditionalRoles = new[] {"Role.Core.Position.ContinuousPositioned"})]
        protected void InitializeCollectReceptorsContinuous<TGameContext, TItemId>(IServiceResolver serviceResolver,
                                                                                   IModuleInitializer<TGameContext> initializer,
                                                                                   EntityRole role)
            where TItemId : IEntityKey
        {
            var ctx = initializer.DeclareEntityContext<TItemId>();
            ctx.Register(ReceptorCollectionContinuousSystemId, 55500, RegisterCollectReceptorsContinuousSystem);
        }

        [EntityRoleInitializer("Role.Core.Senses.Source.Touch")]
        protected void InitializeSenseCollection<TGameContext, TItemId>(IServiceResolver serviceResolver,
                                                                        IModuleInitializer<TGameContext> initializer,
                                                                        EntityRole role)
            where TItemId : IEntityKey
        {
            var ctx = initializer.DeclareEntityContext<TItemId>();
            ctx.Register(SenseSourceCollectionContinuousSystemId, 57500, RegisterCollectSenseSourcesSystem);
        }

        void RegisterPrepareSystem<TGameContext, TItemId>(IServiceResolver serviceResolver,
                                                          IGameLoopSystemRegistration<TGameContext> context,
                                                          EntityRegistry<TItemId> registry,
                                                          ICommandHandlerRegistration<TGameContext, TItemId> handler)
            where TItemId : IEntityKey
        {
            if (!GetOrCreateLightSystem(serviceResolver, out var ls))
            {
                return;
            }

            context.AddInitializationStepHandler(ls.EnsureSenseCacheAvailable);
            context.AddInitializationStepHandler(ls.BeginSenseCalculation);
            context.AddFixedStepHandlers(ls.BeginSenseCalculation);
        }

        void RegisterCollectReceptorsGridSystem<TGameContext, TItemId>(IServiceResolver resolver,
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
                                 .CreateSystem<SensoryReceptorData<TouchSense>, SensoryReceptorState<TouchSense>, ContinuousMapPosition>(ls.CollectReceptor);
            context.AddInitializationStepHandler(system);
            context.AddFixedStepHandlers(system);
        }

        void RegisterCollectReceptorsContinuousSystem<TGameContext, TItemId>(IServiceResolver resolver,
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
                                 .CreateSystem<SensoryReceptorData<TouchSense>, SensoryReceptorState<TouchSense>, EntityGridPosition>(ls.CollectReceptor);
            context.AddInitializationStepHandler(system);
            context.AddFixedStepHandlers(system);
        }

        void RegisterCollectSenseSourcesSystem<TGameContext, TItemId>(IServiceResolver resolver,
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
                                 .CreateSystem<TouchSourceDefinition, SenseSourceState<TouchSense>>(ls.CollectSenseSource);
            context.AddInitializationStepHandler(system);
            context.AddFixedStepHandlers(system);
        }

        void RegisterComputeReceptorFieldOfView<TGameContext, TItemId>(IServiceResolver resolver,
                                                                       IGameLoopSystemRegistration<TGameContext> context,
                                                                       EntityRegistry<TItemId> registry,
                                                                       ICommandHandlerRegistration<TGameContext, TItemId> handler)
            where TItemId : IEntityKey
        {
            if (!GetOrCreateLightSystem(resolver, out var ls))
            {
                return;
            }

            var refreshLocalSenseState =
                registry.BuildSystem()
                        .WithContext<TGameContext>()
                        .CreateSystem<SensoryReceptorData<TouchSense>, SensoryReceptorState<TouchSense>, SenseReceptorDirtyFlag<TouchSense>>(ls.RefreshLocalReceptorState);
            context.AddInitializationStepHandler(refreshLocalSenseState);
            context.AddFixedStepHandlers(refreshLocalSenseState);
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

            if (!serviceResolver.TryResolve(out ISenseDataBlitter senseBlitter))
            {
                senseBlitter = new DefaultSenseDataBlitter();
            }


            var uniSys = new OmnidirectionalSenseReceptorSystem<TouchSense, TouchSense>(ls, senseBlitter);
            var system = registry.BuildSystem()
                                 .WithContext<TGameContext>()
                                 .CreateSystem<SingleLevelSenseDirectionMapData<TouchSense, TouchSense>, SensoryReceptorState<TouchSense>>(uniSys.CopySenseSourcesToVisionField);


            context.AddInitializationStepHandler(system);
            context.AddFixedStepHandlers(system);
        }

        void RegisterFinalizeSystem<TGameContext, TItemId>(IServiceResolver serviceResolver,
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
        }

        static bool GetOrCreateLightSystem(IServiceResolver serviceResolver, out SenseReceptorSystem<TouchSense, TouchSense> ls)
        {
            if (!serviceResolver.TryResolve(out INoisePhysicsConfiguration physicsConfig))
            {
                ls = default;
                return false;
            }

            if (!serviceResolver.TryResolve(out ls))
            {
                ls = new TouchReceptorSystem(serviceResolver.ResolveToReference<ISensePropertiesSource>(),
                                             serviceResolver.ResolveToReference<ISenseStateCacheProvider>(),
                                             serviceResolver.ResolveToReference<IGlobalSenseStateCacheProvider>(),
                                             serviceResolver.ResolveToReference<ITimeSource>(),
                                             physicsConfig.NoisePhysics,
                                             physicsConfig.CreateNoisePropagationAlgorithm());
            }

            return true;
        }

        void RegisterEntities<TItemId>(IServiceResolver serviceResolver, EntityRegistry<TItemId> registry)
            where TItemId : IEntityKey
        {
            registry.RegisterNonConstructable<SensoryReceptorData<TouchSense>>();
            registry.RegisterNonConstructable<SensoryReceptorState<TouchSense>>();
            registry.RegisterNonConstructable<SingleLevelSenseDirectionMapData<TouchSense, TouchSense>>();
            registry.RegisterFlag<SenseReceptorDirtyFlag<TouchSense>>();
        }
    }
}