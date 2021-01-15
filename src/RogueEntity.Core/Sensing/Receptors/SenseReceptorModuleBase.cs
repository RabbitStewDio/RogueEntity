using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using EnTTSharp.Entities;
using EnTTSharp.Entities.Systems;
using RogueEntity.Api.GameLoops;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Modules;
using RogueEntity.Api.Modules.Attributes;
using RogueEntity.Api.Modules.Helpers;
using RogueEntity.Api.Services;
using RogueEntity.Api.Time;
using RogueEntity.Core.GridProcessing.LayerAggregation;
using RogueEntity.Core.Infrastructure.Services;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Continuous;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Sensing.Cache;
using RogueEntity.Core.Sensing.Common;
using RogueEntity.Core.Sensing.Common.Physics;
using RogueEntity.Core.Sensing.Resistance;
using RogueEntity.Core.Sensing.Resistance.Directions;
using RogueEntity.Core.Sensing.Resistance.Maps;
using RogueEntity.Core.Sensing.Sources;

namespace RogueEntity.Core.Sensing.Receptors
{
    public abstract class SenseReceptorModuleBase<TReceptorSense, TSourceSense, TSenseSource> : ModuleBase
        where TReceptorSense : ISense
        where TSourceSense : ISense
        where TSenseSource : ISenseDefinition
    {
        public static readonly EntitySystemId RegisterEntityId = SenseReceptorModules.CreateEntityId<TReceptorSense, TSourceSense>("Core");

        public static readonly EntitySystemId ReceptorPreparationSystemId = SenseReceptorModules.CreateSystemId<TReceptorSense, TSourceSense>("Prepare");
        public static readonly EntitySystemId ReceptorCollectionGridSystemId = SenseReceptorModules.CreateSystemId<TReceptorSense, TSourceSense>("Collect.Grid");
        public static readonly EntitySystemId ReceptorCollectionContinuousSystemId = SenseReceptorModules.CreateSystemId<TReceptorSense, TSourceSense>("Collect.Continuous");
        public static readonly EntitySystemId SenseSourceCollectionSystemId = SenseReceptorModules.CreateSystemId<TReceptorSense, TSourceSense>("CollectSources");
        public static readonly EntitySystemId ReceptorComputeFoVSystemId = SenseReceptorModules.CreateSystemId<TReceptorSense, TSourceSense>("ComputeFieldOfView");
        public static readonly EntitySystemId ReceptorComputeSystemId = SenseReceptorModules.CreateSystemId<TReceptorSense, TSourceSense>("ComputeSenseData");
        public static readonly EntitySystemId ReceptorFinalizeSystemId = SenseReceptorModules.CreateSystemId<TReceptorSense, TSourceSense>("Finalize");

        public static readonly EntityRole SenseReceptorActorRole = SenseReceptorModules.GetReceptorRole<TReceptorSense, TSourceSense>();
        static readonly EntityRole SenseSourceRole = SenseSourceModules.GetSourceRole<TSourceSense>();

        protected SenseReceptorModuleBase()
        {
            RequireRole(SenseReceptorActorRole)
                .WithImpliedRole(SenseReceptors.SenseReceptorRole)
                .WithImpliedRole(SensoryCacheModule.SenseCacheSourceRole);

            DeclareDependencies(ModuleDependency.Of(PositionModule.ModuleId),
                                ModuleDependency.Of(SensoryCacheModule.ModuleId));
        }

        [InitializerCollector(InitializerCollectorType.Roles)]
        public IEnumerable<ModuleEntityRoleInitializerInfo<TItemId>> CollectRoleInitializers<TItemId>(IServiceResolver serviceResolver,
                                                                                                      IModuleEntityInformation entityInformation,
                                                                                                      EntityRole role)
            where TItemId : IEntityKey
        {
            if (role == SenseReceptorActorRole)
            {
                yield return ModuleEntityRoleInitializerInfo.CreateFor<TItemId>
                    (SenseReceptorActorRole, InitializeSenseReceptorRole);

                yield return ModuleEntityRoleInitializerInfo.CreateFor<TItemId>
                                                                (SenseReceptorActorRole, InitializeCollectReceptorsGrid)
                                                            .WithRequiredRoles(PositionModule.GridPositionedRole);

                yield return ModuleEntityRoleInitializerInfo.CreateFor<TItemId>
                                                                (SenseReceptorActorRole, InitializeCollectReceptorsContinuous)
                                                            .WithRequiredRoles(PositionModule.ContinuousPositionedRole);

                yield return ModuleEntityRoleInitializerInfo.CreateFor<TItemId>
                                                                (SenseReceptorActorRole, InitializeSenseCache)
                                                            .WithRequiredRoles(PositionModule.GridPositionedRole);
            }

            if (role == SenseSourceRole)
            {
                yield return ModuleEntityRoleInitializerInfo.CreateFor<TItemId>(SenseSourceRole, InitializeCollectSenseSources);
            }
        }

        [SuppressMessage("ReSharper", "UnusedTypeParameter")]
        protected void InitializeSenseCache<TItemId>(in ModuleEntityInitializationParameter<TItemId> initParameter,
                                                     IModuleInitializer initializer,
                                                     EntityRole role)
            where TItemId : IEntityKey
        {
            var serviceResolver = initParameter.ServiceResolver;
            if (serviceResolver.TryResolve(out SenseCacheSetUpSystem o))
            {
                o.RegisterSense<TReceptorSense>();
                o.RegisterSense<TSourceSense>();
            }
        }

        protected void InitializeSenseReceptorRole<TItemId>(in ModuleEntityInitializationParameter<TItemId> initParameter,
                                                            IModuleInitializer initializer,
                                                            EntityRole role)
            where TItemId : IEntityKey
        {
            var ctx = initializer.DeclareEntityContext<TItemId>();
            ctx.Register(RegisterEntityId, 0, RegisterEntities);
            ctx.Register(ReceptorPreparationSystemId, 50000, RegisterPrepareSystem);
            ctx.Register(ReceptorComputeFoVSystemId, 56000, RegisterComputeReceptorFieldOfView);
            ctx.Register(ReceptorComputeSystemId, 58500, RegisterCalculateDirectionalSystem);
            ctx.Register(ReceptorFinalizeSystemId, 59500, RegisterFinalizeSystem);
        }

        protected void InitializeCollectReceptorsGrid<TItemId>(in ModuleEntityInitializationParameter<TItemId> initParameter,
                                                               IModuleInitializer initializer,
                                                               EntityRole role)
            where TItemId : IEntityKey
        {
            var ctx = initializer.DeclareEntityContext<TItemId>();
            ctx.Register(ReceptorCollectionGridSystemId, 55500, RegisterCollectReceptorsGridSystem);
        }

        protected void InitializeCollectReceptorsContinuous<TItemId>(in ModuleEntityInitializationParameter<TItemId> initParameter,
                                                                     IModuleInitializer initializer,
                                                                     EntityRole role)
            where TItemId : IEntityKey
        {
            var ctx = initializer.DeclareEntityContext<TItemId>();
            ctx.Register(ReceptorCollectionContinuousSystemId, 55500, RegisterCollectReceptorsContinuousSystem);
        }

        protected void InitializeCollectSenseSources<TItemId>(in ModuleEntityInitializationParameter<TItemId> initParameter,
                                                              IModuleInitializer initializer,
                                                              EntityRole role)
            where TItemId : IEntityKey
        {
            var ctx = initializer.DeclareEntityContext<TItemId>();
            ctx.Register(SenseSourceCollectionSystemId, 57500, RegisterCollectSenseSourcesSystem);
        }

        protected void RegisterPrepareSystem<TItemId>(in ModuleInitializationParameter initParameter,
                                                      IGameLoopSystemRegistration context,
                                                      EntityRegistry<TItemId> registry)
            where TItemId : IEntityKey
        {
            var serviceResolver = initParameter.ServiceResolver;
            var ls = GetOrCreateSenseReceptorSystem<TItemId>(serviceResolver);
            context.AddInitializationStepHandler(ls.EnsureSenseCacheAvailable, nameof(ls.EnsureSenseCacheAvailable));
            context.AddInitializationStepHandler(ls.BeginSenseCalculation, nameof(ls.BeginSenseCalculation));
            context.AddFixedStepHandlers(ls.BeginSenseCalculation, nameof(ls.BeginSenseCalculation));
        }

        protected void RegisterCollectReceptorsGridSystem<TItemId>(in ModuleInitializationParameter initParameter,
                                                                   IGameLoopSystemRegistration context,
                                                                   EntityRegistry<TItemId> registry)
            where TItemId : IEntityKey
        {
            var serviceResolver = initParameter.ServiceResolver;
            var ls = GetOrCreateSenseReceptorSystem<TItemId>(serviceResolver);
            var system = registry.BuildSystem()
                                 .WithoutContext()
                                 .WithInputParameter<
                                     SensoryReceptorData<TReceptorSense, TSourceSense>,
                                     EntityGridPosition>()
                                 .WithOutputParameter<SensoryReceptorState<TReceptorSense, TSourceSense>>()
                                 .CreateSystem(ls.CollectReceptor);
            context.AddInitializationStepHandler(system);
            context.AddFixedStepHandlers(system);
        }

        protected void RegisterCollectReceptorsContinuousSystem<TItemId>(in ModuleInitializationParameter initParameter,
                                                                         IGameLoopSystemRegistration context,
                                                                         EntityRegistry<TItemId> registry)
            where TItemId : IEntityKey
        {
            var serviceResolver = initParameter.ServiceResolver;
            var ls = GetOrCreateSenseReceptorSystem<TItemId>(serviceResolver);
            var system = registry.BuildSystem()
                                 .WithoutContext()
                                 .WithInputParameter<
                                     SensoryReceptorData<TReceptorSense, TSourceSense>,
                                     ContinuousMapPosition>()
                                 .WithOutputParameter<SensoryReceptorState<TReceptorSense, TSourceSense>>()
                                 .CreateSystem(ls.CollectReceptor);
            context.AddInitializationStepHandler(system);
            context.AddFixedStepHandlers(system);
        }

        protected void RegisterCollectSenseSourcesSystem<TItemId>(in ModuleInitializationParameter initParameter,
                                                                  IGameLoopSystemRegistration context,
                                                                  EntityRegistry<TItemId> registry)
            where TItemId : IEntityKey
        {
            var serviceResolver = initParameter.ServiceResolver;
            var ls = GetOrCreateSenseReceptorSystem<TItemId>(serviceResolver);
            var system = registry.BuildSystem()
                                 .WithoutContext()
                                 .WithInputParameter<TSenseSource, SenseSourceState<TSourceSense>>()
                                 .CreateSystem(ls.CollectObservedSenseSource);
            context.AddInitializationStepHandler(system);
            context.AddFixedStepHandlers(system);
        }

        protected void RegisterComputeReceptorFieldOfView<TItemId>(in ModuleInitializationParameter initParameter,
                                                                   IGameLoopSystemRegistration context,
                                                                   EntityRegistry<TItemId> registry)
            where TItemId : IEntityKey
        {
            var serviceResolver = initParameter.ServiceResolver;
            var ls = GetOrCreateSenseReceptorSystem<TItemId>(serviceResolver);

            var refreshLocalSenseState =
                registry.BuildSystem()
                        .WithoutContext()
                        .WithInputParameter<
                            SensoryReceptorData<TReceptorSense, TSourceSense>,
                            SenseReceptorDirtyFlag<TReceptorSense, TSourceSense>>()
                        .WithOutputParameter<SensoryReceptorState<TReceptorSense, TSourceSense>>()
                        .CreateSystem(ls.RefreshLocalReceptorState);
            context.AddInitializationStepHandler(refreshLocalSenseState);
            context.AddFixedStepHandlers(refreshLocalSenseState);
        }

        protected abstract void RegisterCalculateDirectionalSystem<TItemId>(in ModuleInitializationParameter initParameter,
                                                                            IGameLoopSystemRegistration context,
                                                                            EntityRegistry<TItemId> registry)
            where TItemId : IEntityKey;

        protected void RegisterCalculateOmniDirectionalSystem<TItemId>(in ModuleInitializationParameter initParameter,
                                                                       IGameLoopSystemRegistration context,
                                                                       EntityRegistry<TItemId> registry)
            where TItemId : IEntityKey
        {
            var serviceResolver = initParameter.ServiceResolver;
            var ls = GetOrCreateSenseReceptorSystem<TItemId>(serviceResolver);

            if (!serviceResolver.TryResolve(out IRadiationSenseReceptorBlitter senseBlitter))
            {
                senseBlitter = new DefaultRadiationSenseReceptorBlitter();
            }

            var omni = new SenseReceptorBlitterSystem<TReceptorSense, TSourceSense>(ls, senseBlitter);
            var system = registry.BuildSystem()
                                 .WithoutContext()
                                 .WithInputParameter<SensoryReceptorState<TReceptorSense, TSourceSense>>()
                                 .WithOutputParameter<SingleLevelSenseDirectionMapData<TReceptorSense, TSourceSense>>()
                                 .CreateSystem(omni.CopySenseSourcesToVisionField);


            context.AddInitializationStepHandler(system);
            context.AddFixedStepHandlers(system);
        }

        protected void RegisterCalculateUniDirectionalSystem<TItemId>(in ModuleInitializationParameter initParameter,
                                                                      IGameLoopSystemRegistration context,
                                                                      EntityRegistry<TItemId> registry)
            where TItemId : IEntityKey
        {
            var serviceResolver = initParameter.ServiceResolver;
            var ls = GetOrCreateSenseReceptorSystem<TItemId>(serviceResolver);

            if (!serviceResolver.TryResolve(out IDirectionalSenseReceptorBlitter senseBlitter))
            {
                senseBlitter = new DefaultDirectionalSenseReceptorBlitter();
            }


            var uniSys = new SenseReceptorBlitterSystem<TReceptorSense, TSourceSense>(ls, senseBlitter);
            var system = registry.BuildSystem()
                                 .WithoutContext()
                                 .WithInputParameter<SensoryReceptorState<TReceptorSense, TSourceSense>>()
                                 .WithOutputParameter<SingleLevelSenseDirectionMapData<TReceptorSense, TSourceSense>>()
                                 .CreateSystem(uniSys.CopySenseSourcesToVisionField);


            context.AddInitializationStepHandler(system);
            context.AddFixedStepHandlers(system);
        }


        protected void RegisterFinalizeSystem<TItemId>(in ModuleInitializationParameter initParameter,
                                                       IGameLoopSystemRegistration context,
                                                       EntityRegistry<TItemId> registry)
            where TItemId : IEntityKey
        {
            var serviceResolver = initParameter.ServiceResolver;
            var ls = GetOrCreateSenseReceptorSystem<TItemId>(serviceResolver);

            var clearReceptorStateSystem =
                registry.BuildSystem()
                        .WithoutContext()
                        .WithInputParameter<
                            SensoryReceptorData<TReceptorSense, TSourceSense>,
                            SenseReceptorDirtyFlag<TReceptorSense, TSourceSense>>()
                        .WithOutputParameter<SensoryReceptorState<TReceptorSense, TSourceSense>>()
                        .CreateSystem(ls.ResetReceptorCacheState);

            context.AddInitializationStepHandler(clearReceptorStateSystem, nameof(ls.ResetReceptorCacheState));
            context.AddFixedStepHandlers(clearReceptorStateSystem, nameof(ls.ResetReceptorCacheState));

            var clearObservedStateSystem =
                registry.BuildSystem()
                        .WithoutContext()
                        .WithInputParameter<ObservedSenseSource<TSourceSense>>()
                        .CreateSystem(ls.ResetSenseSourceObservedState);

            context.AddInitializationStepHandler(clearObservedStateSystem, nameof(ls.ResetSenseSourceObservedState));
            context.AddFixedStepHandlers(clearObservedStateSystem, nameof(ls.ResetSenseSourceObservedState));


            context.AddInitializationStepHandler(ls.EndSenseCalculation, nameof(ls.EndSenseCalculation));
            context.AddFixedStepHandlers(ls.EndSenseCalculation, nameof(ls.EndSenseCalculation));
        }

        protected virtual SensePropertiesSystem<TReceptorSense> GetOrCreateSensePropertiesSystem<TEntityId>(IServiceResolver serviceResolver)
        {
            if (serviceResolver.TryResolve(out SensePropertiesSystem<TReceptorSense> system))
            {
                return system;
            }

            var gridConfig = serviceResolver.Resolve<IGridMapConfiguration<TEntityId>>();
            system = new SensePropertiesSystem<TReceptorSense>(gridConfig.OffsetX, gridConfig.OffsetY, gridConfig.TileSizeX, gridConfig.TileSizeY);

            serviceResolver.Store(system);
            serviceResolver.Store<IAggregationLayerSystemBackend<SensoryResistance<TReceptorSense>>>(system);
            serviceResolver.Store<ISensePropertiesDataView<TReceptorSense>>(system);
            return system;
        }

        protected virtual SensoryResistanceDirectionalitySystem<TReceptorSense> GetOrCreateDirectionalitySystem<TItemId>(IServiceResolver serviceResolver)
        {
            if (serviceResolver.TryResolve(out SensoryResistanceDirectionalitySystem<TReceptorSense> system))
            {
                return system;
            }

            if (!serviceResolver.TryResolve(out ISensePropertiesDataView<TReceptorSense> data))
            {
                data = GetOrCreateSensePropertiesSystem<TItemId>(serviceResolver);
            }

            system = new SensoryResistanceDirectionalitySystem<TReceptorSense>(data.ResultView);
            serviceResolver.Store(system);
            serviceResolver.Store<ISensoryResistanceDirectionView<TReceptorSense>>(system);
            return system;
        }


        protected abstract (ISensePropagationAlgorithm propagationAlgorithm, ISensePhysics sensePhysics) GetOrCreatePhysics(IServiceResolver serviceResolver);

        protected virtual SenseReceptorSystem<TReceptorSense, TSourceSense> GetOrCreateSenseReceptorSystem<TItemId>(IServiceResolver serviceResolver)
        {
            if (!serviceResolver.TryResolve(out SenseReceptorSystem<TReceptorSense, TSourceSense> ls))
            {
                var physics = GetOrCreatePhysics(serviceResolver);
                var senseProperties = serviceResolver.ResolveToReference<ISensePropertiesDataView<TReceptorSense>>().Map(l => l.ResultView);
                ls = new SenseReceptorSystem<TReceptorSense, TSourceSense>(senseProperties,
                                                                           serviceResolver.ResolveToReference<ISenseStateCacheProvider>(),
                                                                           serviceResolver.ResolveToReference<IGlobalSenseStateCacheProvider>(),
                                                                           serviceResolver.ResolveToReference<ITimeSource>(),
                                                                           GetOrCreateDirectionalitySystem<TItemId>(serviceResolver),
                                                                           physics.sensePhysics,
                                                                           physics.propagationAlgorithm);
            }

            return ls;
        }

        protected void RegisterEntities<TItemId>(in ModuleInitializationParameter initParameter,
                                                 EntityRegistry<TItemId> registry)
            where TItemId : IEntityKey
        {
            registry.RegisterNonConstructable<SensoryReceptorData<TReceptorSense, TSourceSense>>();
            registry.RegisterNonConstructable<SensoryReceptorState<TReceptorSense, TSourceSense>>();
            registry.RegisterNonConstructable<SingleLevelSenseDirectionMapData<TReceptorSense, TSourceSense>>();
            registry.RegisterFlag<SenseReceptorDirtyFlag<TReceptorSense, TSourceSense>>();
            if (!registry.IsManaged<ObservedSenseSource<TSourceSense>>())
            {
                // might be requested by more than one system.
                registry.RegisterFlag<ObservedSenseSource<TSourceSense>>();
            }
        }
    }
}
