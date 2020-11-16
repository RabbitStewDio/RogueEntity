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
using RogueEntity.Core.Utils.DataViews;

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
        public IEnumerable<ModuleEntityRoleInitializerInfo<TGameContext, TItemId>> CollectRoleInitializers<TGameContext, TItemId>(IServiceResolver serviceResolver,
                                                                                                                         IModuleEntityInformation entityInformation,
                                                                                                                         EntityRole role)
            where TItemId : IEntityKey
        {
            if (role == SenseReceptorActorRole)
            {
                yield return ModuleEntityRoleInitializerInfo.CreateFor<TGameContext, TItemId>
                    (SenseReceptorActorRole, InitializeSenseReceptorRole);

                yield return ModuleEntityRoleInitializerInfo.CreateFor<TGameContext, TItemId>
                    (SenseReceptorActorRole, InitializeCollectReceptorsGrid).WithRequiredRoles(PositionModule.GridPositionedRole);

                yield return ModuleEntityRoleInitializerInfo.CreateFor<TGameContext, TItemId>
                    (SenseReceptorActorRole, InitializeCollectReceptorsContinuous).WithRequiredRoles(PositionModule.ContinuousPositionedRole);

                yield return ModuleEntityRoleInitializerInfo.CreateFor<TGameContext, TItemId>
                    (SenseReceptorActorRole, InitializeSenseCache).WithRequiredRoles(PositionModule.GridPositionedRole);
            }

            if (role == SenseSourceRole)
            {
                yield return ModuleEntityRoleInitializerInfo.CreateFor<TGameContext, TItemId>(SenseSourceRole, InitializeCollectSenseSources);
            }
        }

        [SuppressMessage("ReSharper", "UnusedTypeParameter")]
        protected void InitializeSenseCache<TGameContext, TItemId>(in ModuleEntityInitializationParameter<TGameContext, TItemId> initParameter,
                                                                   IModuleInitializer<TGameContext> initializer,
                                                                   EntityRole role)
            where TItemId : IEntityKey
        {
            var serviceResolver = initParameter.ServiceResolver;
            if (serviceResolver.TryResolve(out SenseCacheSetUpSystem<TGameContext> o))
            {
                o.RegisterSense<TReceptorSense>();
                o.RegisterSense<TSourceSense>();
            }
        }

        protected void InitializeSenseReceptorRole<TGameContext, TItemId>(in ModuleEntityInitializationParameter<TGameContext, TItemId> initParameter,
                                                             IModuleInitializer<TGameContext> initializer,
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

        protected void InitializeCollectReceptorsGrid<TGameContext, TItemId>(in ModuleEntityInitializationParameter<TGameContext, TItemId> initParameter,
                                                                             IModuleInitializer<TGameContext> initializer,
                                                                             EntityRole role)
            where TItemId : IEntityKey
        {
            var ctx = initializer.DeclareEntityContext<TItemId>();
            ctx.Register(ReceptorCollectionGridSystemId, 55500, RegisterCollectReceptorsGridSystem);
        }

        protected void InitializeCollectReceptorsContinuous<TGameContext, TItemId>(in ModuleEntityInitializationParameter<TGameContext, TItemId> initParameter,
                                                                                   IModuleInitializer<TGameContext> initializer,
                                                                                   EntityRole role)
            where TItemId : IEntityKey
        {
            var ctx = initializer.DeclareEntityContext<TItemId>();
            ctx.Register(ReceptorCollectionContinuousSystemId, 55500, RegisterCollectReceptorsContinuousSystem);
        }

        protected void InitializeCollectSenseSources<TGameContext, TItemId>(in ModuleEntityInitializationParameter<TGameContext, TItemId> initParameter,
                                                                            IModuleInitializer<TGameContext> initializer,
                                                                            EntityRole role)
            where TItemId : IEntityKey
        {
            var ctx = initializer.DeclareEntityContext<TItemId>();
            ctx.Register(SenseSourceCollectionSystemId, 57500, RegisterCollectSenseSourcesSystem);
        }

        protected void RegisterPrepareSystem<TGameContext, TItemId>(in ModuleInitializationParameter initParameter,
                                                                    IGameLoopSystemRegistration<TGameContext> context,
                                                                    EntityRegistry<TItemId> registry)
            where TItemId : IEntityKey
        {
            var serviceResolver = initParameter.ServiceResolver;
            var ls = GetOrCreateSenseReceptorSystem<TGameContext, TItemId>(serviceResolver);
            context.AddInitializationStepHandler(ls.EnsureSenseCacheAvailable, nameof(ls.EnsureSenseCacheAvailable));
            context.AddInitializationStepHandler(ls.BeginSenseCalculation, nameof(ls.BeginSenseCalculation));
            context.AddFixedStepHandlers(ls.BeginSenseCalculation, nameof(ls.BeginSenseCalculation));
        }

        protected void RegisterCollectReceptorsGridSystem<TGameContext, TItemId>(in ModuleInitializationParameter initParameter,
                                                                                 IGameLoopSystemRegistration<TGameContext> context,
                                                                                 EntityRegistry<TItemId> registry)
            where TItemId : IEntityKey
        {
            var serviceResolver = initParameter.ServiceResolver;
            var ls = GetOrCreateSenseReceptorSystem<TGameContext, TItemId>(serviceResolver);
            var system = registry.BuildSystem()
                                 .WithContext<TGameContext>()
                                 .CreateSystem<SensoryReceptorData<TReceptorSense, TSourceSense>, SensoryReceptorState<TReceptorSense, TSourceSense>, EntityGridPosition>(ls.CollectReceptor);
            context.AddInitializationStepHandler(system);
            context.AddFixedStepHandlers(system);
        }

        protected void RegisterCollectReceptorsContinuousSystem<TGameContext, TItemId>(in ModuleInitializationParameter initParameter,
                                                                                       IGameLoopSystemRegistration<TGameContext> context,
                                                                                       EntityRegistry<TItemId> registry)
            where TItemId : IEntityKey
        {
            var serviceResolver = initParameter.ServiceResolver;
            var ls = GetOrCreateSenseReceptorSystem<TGameContext, TItemId>(serviceResolver);
            var system = registry.BuildSystem()
                                 .WithContext<TGameContext>()
                                 .CreateSystem<SensoryReceptorData<TReceptorSense, TSourceSense>, SensoryReceptorState<TReceptorSense, TSourceSense>, ContinuousMapPosition>(ls.CollectReceptor);
            context.AddInitializationStepHandler(system);
            context.AddFixedStepHandlers(system);
        }

        protected void RegisterCollectSenseSourcesSystem<TGameContext, TItemId>(in ModuleInitializationParameter initParameter,
                                                                                IGameLoopSystemRegistration<TGameContext> context,
                                                                                EntityRegistry<TItemId> registry)
            where TItemId : IEntityKey
        {
            var serviceResolver = initParameter.ServiceResolver;
            var ls = GetOrCreateSenseReceptorSystem<TGameContext, TItemId>(serviceResolver);
            var system = registry.BuildSystem()
                                 .WithContext<TGameContext>()
                                 .CreateSystem<TSenseSource, SenseSourceState<TSourceSense>>(ls.CollectObservedSenseSource);
            context.AddInitializationStepHandler(system);
            context.AddFixedStepHandlers(system);
        }

        protected void RegisterComputeReceptorFieldOfView<TGameContext, TItemId>(in ModuleInitializationParameter initParameter,
                                                                                 IGameLoopSystemRegistration<TGameContext> context,
                                                                                 EntityRegistry<TItemId> registry)
            where TItemId : IEntityKey
        {
            var serviceResolver = initParameter.ServiceResolver;
            var ls = GetOrCreateSenseReceptorSystem<TGameContext, TItemId>(serviceResolver);

            var refreshLocalSenseState =
                registry.BuildSystem()
                        .WithContext<TGameContext>()
                        .CreateSystem<SensoryReceptorData<TReceptorSense, TSourceSense>, SensoryReceptorState<TReceptorSense, TSourceSense>, SenseReceptorDirtyFlag<TReceptorSense, TSourceSense>>(
                            ls.RefreshLocalReceptorState);
            context.AddInitializationStepHandler(refreshLocalSenseState);
            context.AddFixedStepHandlers(refreshLocalSenseState);
        }

        protected abstract void RegisterCalculateDirectionalSystem<TGameContext, TItemId>(in ModuleInitializationParameter initParameter,
                                                                                          IGameLoopSystemRegistration<TGameContext> context,
                                                                                          EntityRegistry<TItemId> registry)
            where TItemId : IEntityKey;

        protected void RegisterCalculateOmniDirectionalSystem<TGameContext, TItemId>(in ModuleInitializationParameter initParameter,
                                                                                     IGameLoopSystemRegistration<TGameContext> context,
                                                                                     EntityRegistry<TItemId> registry)
            where TItemId : IEntityKey
        {
            var serviceResolver = initParameter.ServiceResolver;
            var ls = GetOrCreateSenseReceptorSystem<TGameContext, TItemId>(serviceResolver);

            if (!serviceResolver.TryResolve(out IRadiationSenseReceptorBlitter senseBlitter))
            {
                senseBlitter = new DefaultRadiationSenseReceptorBlitter();
            }

            var omni = new SenseReceptorBlitterSystem<TReceptorSense, TSourceSense>(ls, senseBlitter);
            var system = registry.BuildSystem()
                                 .WithContext<TGameContext>()
                                 .CreateSystem<SingleLevelSenseDirectionMapData<TReceptorSense, TSourceSense>, SensoryReceptorState<TReceptorSense, TSourceSense>>(omni.CopySenseSourcesToVisionField);


            context.AddInitializationStepHandler(system);
            context.AddFixedStepHandlers(system);
        }

        protected void RegisterCalculateUniDirectionalSystem<TGameContext, TItemId>(in ModuleInitializationParameter initParameter,
                                                                                    IGameLoopSystemRegistration<TGameContext> context,
                                                                                    EntityRegistry<TItemId> registry)
            where TItemId : IEntityKey
        {
            var serviceResolver = initParameter.ServiceResolver;
            var ls = GetOrCreateSenseReceptorSystem<TGameContext, TItemId>(serviceResolver);

            if (!serviceResolver.TryResolve(out IDirectionalSenseReceptorBlitter senseBlitter))
            {
                senseBlitter = new DefaultDirectionalSenseReceptorBlitter();
            }


            var uniSys = new SenseReceptorBlitterSystem<TReceptorSense, TSourceSense>(ls, senseBlitter);
            var system = registry.BuildSystem()
                                 .WithContext<TGameContext>()
                                 .CreateSystem<SingleLevelSenseDirectionMapData<TReceptorSense, TSourceSense>, SensoryReceptorState<TReceptorSense, TSourceSense>>(
                                     uniSys.CopySenseSourcesToVisionField);


            context.AddInitializationStepHandler(system);
            context.AddFixedStepHandlers(system);
        }


        protected void RegisterFinalizeSystem<TGameContext, TItemId>(in ModuleInitializationParameter initParameter,
                                                                     IGameLoopSystemRegistration<TGameContext> context,
                                                                     EntityRegistry<TItemId> registry)
            where TItemId : IEntityKey
        {
            var serviceResolver = initParameter.ServiceResolver;
            var ls = GetOrCreateSenseReceptorSystem<TGameContext, TItemId>(serviceResolver);

            var clearReceptorStateSystem =
                registry.BuildSystem()
                        .WithContext<TGameContext>()
                        .CreateSystem<SensoryReceptorData<TReceptorSense, TSourceSense>, SensoryReceptorState<TReceptorSense, TSourceSense>, SenseReceptorDirtyFlag<TReceptorSense, TSourceSense>>(
                            ls.ResetReceptorCacheState);

            context.AddInitializationStepHandler(clearReceptorStateSystem, nameof(ls.ResetReceptorCacheState));
            context.AddFixedStepHandlers(clearReceptorStateSystem, nameof(ls.ResetReceptorCacheState));

            var clearObservedStateSystem =
                registry.BuildSystem()
                        .WithContext<TGameContext>()
                        .CreateSystem<ObservedSenseSource<TSourceSense>>(ls.ResetSenseSourceObservedState);

            context.AddInitializationStepHandler(clearObservedStateSystem, nameof(ls.ResetSenseSourceObservedState));
            context.AddFixedStepHandlers(clearObservedStateSystem, nameof(ls.ResetSenseSourceObservedState));


            context.AddInitializationStepHandler(ls.EndSenseCalculation, nameof(ls.EndSenseCalculation));
            context.AddFixedStepHandlers(ls.EndSenseCalculation, nameof(ls.EndSenseCalculation));
        }

        protected virtual SensePropertiesSystem<TGameContext, TReceptorSense> GetOrCreateSensePropertiesSystem<TGameContext, TEntityId>(IServiceResolver serviceResolver)
        {
            if (serviceResolver.TryResolve(out SensePropertiesSystem<TGameContext, TReceptorSense> system))
            {
                return system;
            }

            var gridConfig = serviceResolver.Resolve<IGridMapConfiguration<TEntityId>>();
            system = new SensePropertiesSystem<TGameContext, TReceptorSense>(gridConfig.OffsetX, gridConfig.OffsetY, gridConfig.TileSizeX, gridConfig.TileSizeY);

            serviceResolver.Store(system);
            serviceResolver.Store<IAggregationLayerSystem<TGameContext, SensoryResistance<TReceptorSense>>>(system);
            serviceResolver.Store<IReadOnlyDynamicDataView3D<SensoryResistance<TReceptorSense>>>(system);
            return system;
        }

        protected virtual SensoryResistanceDirectionalitySystem<TReceptorSense> GetOrCreateDirectionalitySystem<TGameContext, TItemId>(IServiceResolver serviceResolver)
        {
            if (serviceResolver.TryResolve(out SensoryResistanceDirectionalitySystem<TReceptorSense> system))
            {
                return system;
            }

            if (!serviceResolver.TryResolve(out IReadOnlyDynamicDataView3D<SensoryResistance<TReceptorSense>> data))
            {
                data = GetOrCreateSensePropertiesSystem<TGameContext, TItemId>(serviceResolver);
            }

            system = new SensoryResistanceDirectionalitySystem<TReceptorSense>(data);
            serviceResolver.Store(system);
            serviceResolver.Store<ISensoryResistanceDirectionView<TReceptorSense>>(system);
            return system;
        }


        protected abstract (ISensePropagationAlgorithm propagationAlgorithm, ISensePhysics sensePhysics) GetOrCreatePhysics(IServiceResolver serviceResolver);

        protected virtual SenseReceptorSystem<TReceptorSense, TSourceSense> GetOrCreateSenseReceptorSystem<TGameContext, TItemId>(IServiceResolver serviceResolver)
        {
            if (!serviceResolver.TryResolve(out SenseReceptorSystem<TReceptorSense, TSourceSense> ls))
            {
                var physics = GetOrCreatePhysics(serviceResolver);
                ls = new SenseReceptorSystem<TReceptorSense, TSourceSense>(serviceResolver.ResolveToReference<IReadOnlyDynamicDataView3D<SensoryResistance<TReceptorSense>>>(),
                                                                           serviceResolver.ResolveToReference<ISenseStateCacheProvider>(),
                                                                           serviceResolver.ResolveToReference<IGlobalSenseStateCacheProvider>(),
                                                                           serviceResolver.ResolveToReference<ITimeSource>(),
                                                                           GetOrCreateDirectionalitySystem<TGameContext, TItemId>(serviceResolver),
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