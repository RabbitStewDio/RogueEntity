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
using RogueEntity.Core.Sensing.Receptors.Light;
using RogueEntity.Core.Sensing.Resistance;
using RogueEntity.Core.Sensing.Resistance.Maps;
using RogueEntity.Core.Sensing.Sources;
using RogueEntity.Core.Sensing.Sources.Heat;

namespace RogueEntity.Core.Sensing.Receptors.InfraVision
{
    public class InfraVisionSenseModule : ModuleBase
    {
        public const string ModuleId = "Core.Sense.Receptor.InfraVision";

        public static readonly EntitySystemId RegisterEntityId = "Entities.Core.Senses.Receptor.InfraVision";

        public static readonly EntitySystemId ReceptorPreparationSystemId = "Systems.Core.Senses.Receptor.InfraVision.Prepare";
        public static readonly EntitySystemId ReceptorCollectionGridSystemId = "Systems.Core.Senses.Receptor.InfraVision.Collect.Grid";
        public static readonly EntitySystemId ReceptorCollectionContinuousSystemId = "Systems.Core.Senses.Receptor.InfraVision.Collect.Continuous";
        public static readonly EntitySystemId SenseSourceCollectionContinuousSystemId = "Systems.Core.Senses.Receptor.InfraVision.CollectSources.Continuous";
        public static readonly EntitySystemId ReceptorComputeSystemId = "Systems.Core.Senses.Receptor.InfraVision.Compute";

        public static readonly EntityRole SenseReceptorActorRole = new EntityRole("Role.Core.Senses.Receptor.InfraVision.ActorRole");

        public InfraVisionSenseModule()
        {
            Id = ModuleId;
            Author = "RogueEntity.Core";
            Name = "RogueEntity Core Module - Senses - Receptors";
            Description = "Provides items and actors with a field of view for heat/infrared vision.";
            IsFrameworkModule = true;

            DeclareDependencies(ModuleDependency.Of(PositionModule.ModuleId),
                                ModuleDependency.Of(SensoryResistanceModule.ModuleId),
                                ModuleDependency.Of(HeatSourceModule.ModuleId),
                                ModuleDependency.Of(SensoryCacheModule.ModuleId));

            RequireRole(SenseReceptorActorRole);
        }

        [EntityRoleInitializer("Role.Core.Senses.Receptor.Temperature.ActorRole")]
        protected void InitializeRole<TGameContext, TItemId>(IServiceResolver serviceResolver,
                                                             IModuleInitializer<TGameContext> initializer,
                                                             EntityRole role)
            where TItemId : IEntityKey
            where TGameContext : ITimeContext
        {
            var ctx = initializer.DeclareEntityContext<TItemId>();
            ctx.Register(RegisterEntityId, 0, RegisterEntities);
            ctx.Register(ReceptorPreparationSystemId, 5000, RegisterPrepareSystem);
            ctx.Register(ReceptorComputeSystemId, 5850, RegisterCalculateSystem);
        }

        [EntityRoleInitializer("Role.Core.Senses.Receptor.Temperature.ActorRole",
                               ConditionalRoles = new[] {"Role.Core.Position.GridPositioned"})]
        protected void InitializeCollectReceptorsGrid<TGameContext, TItemId>(IServiceResolver serviceResolver,
                                                                             IModuleInitializer<TGameContext> initializer,
                                                                             EntityRole role)
            where TItemId : IEntityKey
        {
            var ctx = initializer.DeclareEntityContext<TItemId>();
            ctx.Register(ReceptorCollectionGridSystemId, 5500, RegisterCollectReceptorsGridSystem);
        }

        [EntityRoleInitializer("Role.Core.Senses.Receptor.Temperature.ActorRole",
                               ConditionalRoles = new[] {"Role.Core.Position.ContinuousPositioned"})]
        protected void InitializeCollectReceptorsContinuous<TGameContext, TItemId>(IServiceResolver serviceResolver,
                                                                                   IModuleInitializer<TGameContext> initializer,
                                                                                   EntityRole role)
            where TItemId : IEntityKey
        {
            var ctx = initializer.DeclareEntityContext<TItemId>();
            ctx.Register(ReceptorCollectionContinuousSystemId, 5500, RegisterCollectReceptorsContinuousSystem);
        }

        [EntityRoleInitializer("Role.Core.Senses.Source.Temperature")]
        protected void InitializeSenseCollection<TGameContext, TItemId>(IServiceResolver serviceResolver,
                                                                        IModuleInitializer<TGameContext> initializer,
                                                                        EntityRole role)
            where TItemId : IEntityKey
        {
            var ctx = initializer.DeclareEntityContext<TItemId>();
            ctx.Register(SenseSourceCollectionContinuousSystemId, 5750, RegisterCollectSenseSourcesSystem);
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
                                 .CreateSystem<SensoryReceptorData<TemperatureSense>, SensoryReceptorState<TemperatureSense>, ContinuousMapPosition>(ls.CollectReceptor);
            context.AddInitializationStepHandler(system);
            context.AddFixedStepHandlers(system);

            var refreshLocalSenseState =
                registry.BuildSystem()
                        .WithContext<TGameContext>()
                        .CreateSystem<SensoryReceptorData<TemperatureSense>, SensoryReceptorState<TemperatureSense>, SenseReceptorDirtyFlag<TemperatureSense>, ContinuousMapPosition>(
                            ls.RefreshLocalReceptorState);
            context.AddInitializationStepHandler(refreshLocalSenseState);
            context.AddFixedStepHandlers(refreshLocalSenseState);
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
                                 .CreateSystem<HeatSourceDefinition, SenseSourceState<TemperatureSense>>(ls.CollectSenseSource);
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
                                 .CreateSystem<SensoryReceptorData<TemperatureSense>, SensoryReceptorState<TemperatureSense>, EntityGridPosition>(ls.CollectReceptor);
            context.AddInitializationStepHandler(system);
            context.AddFixedStepHandlers(system);

            var refreshLocalSenseState =
                registry.BuildSystem()
                        .WithContext<TGameContext>()
                        .CreateSystem<SensoryReceptorData<TemperatureSense>, SensoryReceptorState<TemperatureSense>, SenseReceptorDirtyFlag<TemperatureSense>, EntityGridPosition>(
                            ls.RefreshLocalReceptorState);

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

            var system = registry.BuildSystem()
                                 .WithContext<TGameContext>()
                                 .CreateSystem<SingleLevelBrightnessMap, SensoryReceptorState<TemperatureSense>>(ls.CopySenseSourcesToVisionField);
            

            context.AddInitializationStepHandler(system);
            context.AddFixedStepHandlers(system);

            context.AddInitializationStepHandler(ls.EndSenseCalculation);
            context.AddFixedStepHandlers(ls.EndSenseCalculation);
        }

        static bool GetOrCreateLightSystem(IServiceResolver serviceResolver, out InfraVisionSenseSystem ls)
        {
            if (!serviceResolver.TryResolve(out IHeatPhysicsConfiguration physicsConfig))
            {
                ls = default;
                return false;
            }

            if (!serviceResolver.TryResolve(out ISenseDataBlitter senseBlitter))
            {
                senseBlitter = new DefaultSenseDataBlitter();
            }

            if (!serviceResolver.TryResolve(out ls))
            {
                ls = new InfraVisionSenseSystem(serviceResolver.ResolveToReference<ISensePropertiesSource>(),
                                                serviceResolver.ResolveToReference<ISenseStateCacheProvider>(),
                                                physicsConfig.HeatPhysics,
                                                physicsConfig.CreateHeatPropagationAlgorithm(),
                                                senseBlitter);
            }

            return true;
        }

        void RegisterEntities<TItemId>(IServiceResolver serviceResolver, EntityRegistry<TItemId> registry)
            where TItemId : IEntityKey
        {
            registry.RegisterNonConstructable<SensoryReceptorData<TemperatureSense>>();
            registry.RegisterNonConstructable<SensoryReceptorState<TemperatureSense>>();
            registry.RegisterNonConstructable<SingleLevelHeatMapData>();
            registry.RegisterFlag<SenseReceptorDirtyFlag<TemperatureSense>>();
        }
    }
}