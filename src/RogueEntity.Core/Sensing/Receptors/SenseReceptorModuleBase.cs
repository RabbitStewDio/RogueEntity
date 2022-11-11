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
        static readonly EntityRole senseSourceRole = SenseSourceModules.GetSourceRole<TSourceSense>();

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
            where TItemId : struct, IEntityKey
        {
            if (role == SenseReceptorActorRole)
            {
                yield return ModuleEntityRoleInitializerInfo.CreateFor<TItemId>
                    (SenseReceptorActorRole, InitializeSenseReceptorRole);

                yield return ModuleEntityRoleInitializerInfo.CreateFor<TItemId>(SenseReceptorActorRole, InitializeCollectReceptorsGrid)
                                                            .WithRequiredRoles(GridPositionModule.GridPositionedRole);

                yield return ModuleEntityRoleInitializerInfo.CreateFor<TItemId>(SenseReceptorActorRole, InitializeCollectReceptorsContinuous)
                                                            .WithRequiredRoles(ContinuousPositionModule.ContinuousPositionedRole);

                yield return ModuleEntityRoleInitializerInfo.CreateFor<TItemId>(SenseReceptorActorRole, InitializeSenseCache)
                                                            .WithRequiredRoles(GridPositionModule.GridPositionedRole);
            }

            if (role == senseSourceRole)
            {
                yield return ModuleEntityRoleInitializerInfo.CreateFor<TItemId>(senseSourceRole, InitializeCollectSenseSources);
            }
        }

        [SuppressMessage("ReSharper", "UnusedTypeParameter")]
        protected void InitializeSenseCache<TItemId>(in ModuleEntityInitializationParameter<TItemId> initParameter,
                                                     IModuleInitializer initializer,
                                                     EntityRole role)
            where TItemId : struct, IEntityKey
        {
            var serviceResolver = initParameter.ServiceResolver;
            if (serviceResolver.TryResolve<SenseCacheSetUpSystem>(out var o))
            {
                o.RegisterSense<TReceptorSense>();
                o.RegisterSense<TSourceSense>();
            }
        }

        protected void InitializeSenseReceptorRole<TItemId>(in ModuleEntityInitializationParameter<TItemId> initParameter,
                                                            IModuleInitializer initializer,
                                                            EntityRole role)
            where TItemId : struct, IEntityKey
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
            where TItemId : struct, IEntityKey
        {
            var ctx = initializer.DeclareEntityContext<TItemId>();
            ctx.Register(ReceptorCollectionGridSystemId, 55500, RegisterCollectReceptorsGridSystem);
        }

        protected void InitializeCollectReceptorsContinuous<TItemId>(in ModuleEntityInitializationParameter<TItemId> initParameter,
                                                                     IModuleInitializer initializer,
                                                                     EntityRole role)
            where TItemId : struct, IEntityKey
        {
            var ctx = initializer.DeclareEntityContext<TItemId>();
            ctx.Register(ReceptorCollectionContinuousSystemId, 55500, RegisterCollectReceptorsContinuousSystem);
        }

        protected void InitializeCollectSenseSources<TItemId>(in ModuleEntityInitializationParameter<TItemId> initParameter,
                                                              IModuleInitializer initializer,
                                                              EntityRole role)
            where TItemId : struct, IEntityKey
        {
            var ctx = initializer.DeclareEntityContext<TItemId>();
            ctx.Register(SenseSourceCollectionSystemId, 57500, RegisterCollectSenseSourcesSystem);
        }

        protected void RegisterPrepareSystem<TItemId>(in ModuleEntityInitializationParameter<TItemId> initParameter,
                                                      IGameLoopSystemRegistration context,
                                                      EntityRegistry<TItemId> registry)
            where TItemId : struct, IEntityKey
        {
            var serviceResolver = initParameter.ServiceResolver;
            var ls = GetOrCreateSenseReceptorSystem<TItemId>(serviceResolver);
            context.AddInitializationStepHandler(ls.EnsureSenseCacheAvailable);
            context.AddInitializationStepHandler(ls.BeginSenseCalculation);
            context.AddFixedStepHandlers(ls.BeginSenseCalculation);
        }

        protected void RegisterCollectReceptorsGridSystem<TItemId>(in ModuleEntityInitializationParameter<TItemId> initParameter,
                                                                   IGameLoopSystemRegistration context,
                                                                   EntityRegistry<TItemId> registry)
            where TItemId : struct, IEntityKey
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
            context.AddInitializationStepHandlerSystem(system);
            context.AddFixedStepHandlerSystem(system);
        }

        protected void RegisterCollectReceptorsContinuousSystem<TItemId>(in ModuleEntityInitializationParameter<TItemId> initParameter,
                                                                         IGameLoopSystemRegistration context,
                                                                         EntityRegistry<TItemId> registry)
            where TItemId : struct, IEntityKey
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
            context.AddInitializationStepHandlerSystem(system);
            context.AddFixedStepHandlerSystem(system);
        }

        protected void RegisterCollectSenseSourcesSystem<TItemId>(in ModuleEntityInitializationParameter<TItemId> initParameter,
                                                                  IGameLoopSystemRegistration context,
                                                                  EntityRegistry<TItemId> registry)
            where TItemId : struct, IEntityKey
        {
            var serviceResolver = initParameter.ServiceResolver;
            var ls = GetOrCreateSenseReceptorSystem<TItemId>(serviceResolver);
            var system = registry.BuildSystem()
                                 .WithoutContext()
                                 .WithInputParameter<TSenseSource, SenseSourceState<TSourceSense>>()
                                 .CreateSystem(ls.CollectObservedSenseSource);
            context.AddInitializationStepHandlerSystem(system);
            context.AddFixedStepHandlerSystem(system);
        }

        protected void RegisterComputeReceptorFieldOfView<TItemId>(in ModuleEntityInitializationParameter<TItemId> initParameter,
                                                                   IGameLoopSystemRegistration context,
                                                                   EntityRegistry<TItemId> registry)
            where TItemId : struct, IEntityKey
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
            context.AddInitializationStepHandlerSystem(refreshLocalSenseState);
            context.AddFixedStepHandlerSystem(refreshLocalSenseState);
        }

        protected abstract void RegisterCalculateDirectionalSystem<TItemId>(in ModuleEntityInitializationParameter<TItemId> initParameter,
                                                                            IGameLoopSystemRegistration context,
                                                                            EntityRegistry<TItemId> registry)
            where TItemId : struct, IEntityKey;

        protected void RegisterCalculateOmniDirectionalSystem<TItemId>(in ModuleEntityInitializationParameter<TItemId> initParameter,
                                                                       IGameLoopSystemRegistration context,
                                                                       EntityRegistry<TItemId> registry)
            where TItemId : struct, IEntityKey
        {
            var serviceResolver = initParameter.ServiceResolver;
            var ls = GetOrCreateSenseReceptorSystem<TItemId>(serviceResolver);

            if (!serviceResolver.TryResolve<IRadiationSenseReceptorBlitter>(out var senseBlitter))
            {
                senseBlitter = new DefaultRadiationSenseReceptorBlitter();
            }

            var omni = new SenseReceptorBlitterSystem<TReceptorSense, TSourceSense>(ls, senseBlitter);
            var system = registry.BuildSystem()
                                 .WithoutContext()
                                 .WithInputParameter<SensoryReceptorState<TReceptorSense, TSourceSense>>()
                                 .WithOutputParameter<SingleLevelSenseDirectionMapData<TReceptorSense, TSourceSense>>()
                                 .CreateSystem(omni.CopySenseSourcesToVisionField);


            context.AddInitializationStepHandlerSystem(system);
            context.AddFixedStepHandlerSystem(system);
        }

        protected void RegisterCalculateUniDirectionalSystem<TItemId>(in ModuleEntityInitializationParameter<TItemId> initParameter,
                                                                      IGameLoopSystemRegistration context,
                                                                      EntityRegistry<TItemId> registry)
            where TItemId : struct, IEntityKey
        {
            var serviceResolver = initParameter.ServiceResolver;
            var ls = GetOrCreateSenseReceptorSystem<TItemId>(serviceResolver);

            if (!serviceResolver.TryResolve<IDirectionalSenseReceptorBlitter>(out var senseBlitter))
            {
                senseBlitter = new DefaultDirectionalSenseReceptorBlitter();
            }


            var uniSys = new SenseReceptorBlitterSystem<TReceptorSense, TSourceSense>(ls, senseBlitter);
            var system = registry.BuildSystem()
                                 .WithoutContext()
                                 .WithInputParameter<SensoryReceptorState<TReceptorSense, TSourceSense>>()
                                 .WithOutputParameter<SingleLevelSenseDirectionMapData<TReceptorSense, TSourceSense>>()
                                 .CreateSystem(uniSys.CopySenseSourcesToVisionField);


            context.AddInitializationStepHandlerSystem(system);
            context.AddFixedStepHandlerSystem(system);
        }


        protected void RegisterFinalizeSystem<TItemId>(in ModuleEntityInitializationParameter<TItemId> initParameter,
                                                       IGameLoopSystemRegistration context,
                                                       EntityRegistry<TItemId> registry)
            where TItemId : struct, IEntityKey
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

            context.AddInitializationStepHandlerSystem(clearObservedStateSystem);
            context.AddFixedStepHandlerSystem(clearObservedStateSystem);


            context.AddInitializationStepHandler(ls.EndSenseCalculation);
            context.AddFixedStepHandlers(ls.EndSenseCalculation);
        }

        protected virtual SensePropertiesSystem<TReceptorSense> GetOrCreateSensePropertiesSystem<TEntityId>(IServiceResolver serviceResolver)
        {
            if (serviceResolver.TryResolve<SensePropertiesSystem<TReceptorSense>>(out var system))
            {
                return system;
            }

            var gridConfig = PositionModuleServices.LookupDefaultConfiguration<TEntityId>(serviceResolver);
            system = new SensePropertiesSystem<TReceptorSense>(gridConfig.OffsetX, gridConfig.OffsetY, gridConfig.TileSizeX, gridConfig.TileSizeY);

            serviceResolver.Store(system);
            serviceResolver.Store<IAggregationLayerSystemBackend<SensoryResistance<TReceptorSense>>>(system);
            serviceResolver.Store<ISensePropertiesDataView<TReceptorSense>>(system);
            return system;
        }

        protected virtual SensoryResistanceDirectionalitySystem<TReceptorSense> GetOrCreateDirectionalitySystem<TItemId>(IServiceResolver serviceResolver)
        {
            if (serviceResolver.TryResolve<SensoryResistanceDirectionalitySystem<TReceptorSense>>(out var system))
            {
                return system;
            }

            if (!serviceResolver.TryResolve<ISensePropertiesDataView<TReceptorSense>>(out var data))
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
            if (!serviceResolver.TryResolve<SenseReceptorSystem<TReceptorSense, TSourceSense>>(out var ls))
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
                serviceResolver.Store(ls);
            }

            return ls;
        }

        protected void RegisterEntities<TItemId>(in ModuleEntityInitializationParameter<TItemId> initParameter,
                                                 EntityRegistry<TItemId> registry)
            where TItemId : struct, IEntityKey
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
