using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using EnTTSharp.Entities;
using EnTTSharp.Entities.Systems;
using RogueEntity.Core.GridProcessing.LayerAggregation;
using RogueEntity.Core.Infrastructure.Commands;
using RogueEntity.Core.Infrastructure.GameLoops;
using RogueEntity.Core.Infrastructure.ItemTraits;
using RogueEntity.Core.Infrastructure.Modules;
using RogueEntity.Core.Infrastructure.Modules.Attributes;
using RogueEntity.Core.Infrastructure.Modules.Services;
using RogueEntity.Core.Infrastructure.Time;
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
        public static readonly EntitySystemId RegisterEntityId = SenseReceptorModules.CreateEntityId<TReceptorSense, TSenseSource>("Core");

        public static readonly EntitySystemId ReceptorPreparationSystemId = SenseReceptorModules.CreateSystemId<TReceptorSense, TSenseSource>("Prepare");
        public static readonly EntitySystemId ReceptorCollectionGridSystemId = SenseReceptorModules.CreateSystemId<TReceptorSense, TSenseSource>("Collect.Grid");
        public static readonly EntitySystemId ReceptorCollectionContinuousSystemId = SenseReceptorModules.CreateSystemId<TReceptorSense, TSenseSource>("Collect.Continuous");
        public static readonly EntitySystemId SenseSourceCollectionSystemId = SenseReceptorModules.CreateSystemId<TReceptorSense, TSenseSource>("CollectSources");
        public static readonly EntitySystemId ReceptorComputeFoVSystemId = SenseReceptorModules.CreateSystemId<TReceptorSense, TSenseSource>("ComputeFieldOfView");
        public static readonly EntitySystemId ReceptorComputeSystemId = SenseReceptorModules.CreateSystemId<TReceptorSense, TSenseSource>("ComputeSenseData");
        public static readonly EntitySystemId ReceptorFinalizeSystemId = SenseReceptorModules.CreateSystemId<TReceptorSense, TSenseSource>("Finalize");

        public static readonly EntityRole SenseReceptorActorRole = SenseReceptorModules.GetReceptorRole<TReceptorSense, TSourceSense>();


        protected SenseReceptorModuleBase()
        {
            RequireRole(SenseReceptorActorRole).WithImpliedRole(SenseReceptors.SenseReceptorRole)
                                               .WithImpliedRole(SensoryCacheModule.SenseCacheSourceRole);

            DeclareDependencies(ModuleDependency.Of(PositionModule.ModuleId),
                                ModuleDependency.Of(SensoryCacheModule.ModuleId));
        }

        [InitializerCollectorAttribute(InitializerCollectorType.Roles)]
        public IEnumerable<ModuleEntityRoleInitializerInfo<TGameContext>> CollectRoleInitializers<TGameContext, TItemId>(IServiceResolver serviceResolver,
                                                                                                                         IModuleEntityInformation entityInformation,
                                                                                                                         EntityRole role)
            where TItemId : IEntityKey
        {
            if (role == SenseReceptorActorRole)
            {
                yield return new ModuleEntityRoleInitializerInfo<TGameContext>
                    (role, InitializeRole<TGameContext, TItemId>);

                yield return new ModuleEntityRoleInitializerInfo<TGameContext>
                    (role, InitializeCollectReceptorsGrid<TGameContext, TItemId>).WithRequiredRoles(PositionModule.GridPositionedRole);

                yield return new ModuleEntityRoleInitializerInfo<TGameContext>
                    (role, InitializeCollectReceptorsContinuous<TGameContext, TItemId>).WithRequiredRoles(PositionModule.ContinuousPositionedRole);

                yield return new ModuleEntityRoleInitializerInfo<TGameContext>
                    (role, InitializeSenseCache<TGameContext, TItemId>).WithRequiredRoles(PositionModule.GridPositionedRole, SensoryCacheModule.SenseCacheSourceRole);
            }

            if (role == SenseSourceModules.GetSourceRole<TSourceSense>())
            {
                yield return new ModuleEntityRoleInitializerInfo<TGameContext>
                    (role, InitializeCollectSenseSources<TGameContext, TItemId>);
            }
        }

        [SuppressMessage("ReSharper", "UnusedTypeParameter")]
        protected void InitializeSenseCache<TGameContext, TItemId>(in ModuleInitializationParameter initParameter,
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

        protected void InitializeRole<TGameContext, TItemId>(in ModuleInitializationParameter initParameter,
                                                             IModuleInitializer<TGameContext> initializer,
                                                             EntityRole role)
            where TItemId : IEntityKey
        {
            var ctx = initializer.DeclareEntityContext<TItemId>();
            ctx.Register(RegisterEntityId, 0, RegisterEntities);
            ctx.Register(ReceptorPreparationSystemId, 50000, RegisterPrepareSystem);
            ctx.Register(ReceptorComputeFoVSystemId, 56000, RegisterComputeReceptorFieldOfView);
            ctx.Register(ReceptorComputeSystemId, 58500, RegisterCalculateOmniDirectionalSystem);
            ctx.Register(ReceptorFinalizeSystemId, 59500, RegisterFinalizeSystem);
        }

        protected void InitializeCollectReceptorsGrid<TGameContext, TItemId>(in ModuleInitializationParameter initParameter,
                                                                             IModuleInitializer<TGameContext> initializer,
                                                                             EntityRole role)
            where TItemId : IEntityKey
        {
            var ctx = initializer.DeclareEntityContext<TItemId>();
            ctx.Register(ReceptorCollectionGridSystemId, 55500, RegisterCollectReceptorsGridSystem);
        }

        protected void InitializeCollectReceptorsContinuous<TGameContext, TItemId>(in ModuleInitializationParameter initParameter,
                                                                                   IModuleInitializer<TGameContext> initializer,
                                                                                   EntityRole role)
            where TItemId : IEntityKey
        {
            var ctx = initializer.DeclareEntityContext<TItemId>();
            ctx.Register(ReceptorCollectionContinuousSystemId, 55500, RegisterCollectReceptorsContinuousSystem);
        }

        protected void InitializeCollectSenseSources<TGameContext, TItemId>(in ModuleInitializationParameter initParameter,
                                                                            IModuleInitializer<TGameContext> initializer,
                                                                            EntityRole role)
            where TItemId : IEntityKey
        {
            var ctx = initializer.DeclareEntityContext<TItemId>();
            ctx.Register(SenseSourceCollectionSystemId, 57500, RegisterCollectSenseSourcesSystem);
        }

        protected void RegisterPrepareSystem<TGameContext, TItemId>(in ModuleInitializationParameter initParameter,
                                                                    IGameLoopSystemRegistration<TGameContext> context,
                                                                    EntityRegistry<TItemId> registry,
                                                                    ICommandHandlerRegistration<TGameContext, TItemId> handler)
            where TItemId : IEntityKey
        {
            var serviceResolver = initParameter.ServiceResolver;
            var ls = GetOrCreateLightSystem<TGameContext, TItemId>(serviceResolver);
            context.AddInitializationStepHandler(ls.EnsureSenseCacheAvailable);
            context.AddInitializationStepHandler(ls.BeginSenseCalculation);
            context.AddFixedStepHandlers(ls.BeginSenseCalculation);
        }

        protected void RegisterCollectReceptorsGridSystem<TGameContext, TItemId>(in ModuleInitializationParameter initParameter,
                                                                                 IGameLoopSystemRegistration<TGameContext> context,
                                                                                 EntityRegistry<TItemId> registry,
                                                                                 ICommandHandlerRegistration<TGameContext, TItemId> handler)
            where TItemId : IEntityKey
        {
            var serviceResolver = initParameter.ServiceResolver;
            var ls = GetOrCreateLightSystem<TGameContext, TItemId>(serviceResolver);
            var system = registry.BuildSystem()
                                 .WithContext<TGameContext>()
                                 .CreateSystem<SensoryReceptorData<TReceptorSense, TSourceSense>, SensoryReceptorState<TReceptorSense, TSourceSense>, EntityGridPosition>(ls.CollectReceptor);
            context.AddInitializationStepHandler(system);
            context.AddFixedStepHandlers(system);
        }

        protected void RegisterCollectReceptorsContinuousSystem<TGameContext, TItemId>(in ModuleInitializationParameter initParameter,
                                                                                       IGameLoopSystemRegistration<TGameContext> context,
                                                                                       EntityRegistry<TItemId> registry,
                                                                                       ICommandHandlerRegistration<TGameContext, TItemId> handler)
            where TItemId : IEntityKey
        {
            var serviceResolver = initParameter.ServiceResolver;
            var ls = GetOrCreateLightSystem<TGameContext, TItemId>(serviceResolver);
            var system = registry.BuildSystem()
                                 .WithContext<TGameContext>()
                                 .CreateSystem<SensoryReceptorData<TReceptorSense, TSourceSense>, SensoryReceptorState<TReceptorSense, TSourceSense>, ContinuousMapPosition>(ls.CollectReceptor);
            context.AddInitializationStepHandler(system);
            context.AddFixedStepHandlers(system);
        }

        protected void RegisterCollectSenseSourcesSystem<TGameContext, TItemId>(in ModuleInitializationParameter initParameter,
                                                                                IGameLoopSystemRegistration<TGameContext> context,
                                                                                EntityRegistry<TItemId> registry,
                                                                                ICommandHandlerRegistration<TGameContext, TItemId> handler)
            where TItemId : IEntityKey
        {
            var serviceResolver = initParameter.ServiceResolver;
            var ls = GetOrCreateLightSystem<TGameContext, TItemId>(serviceResolver);
            var system = registry.BuildSystem()
                                 .WithContext<TGameContext>()
                                 .CreateSystem<TSenseSource, SenseSourceState<TSourceSense>>(ls.CollectObservedSenseSource);
            context.AddInitializationStepHandler(system);
            context.AddFixedStepHandlers(system);
        }

        protected void RegisterComputeReceptorFieldOfView<TGameContext, TItemId>(in ModuleInitializationParameter initParameter,
                                                                                 IGameLoopSystemRegistration<TGameContext> context,
                                                                                 EntityRegistry<TItemId> registry,
                                                                                 ICommandHandlerRegistration<TGameContext, TItemId> handler)
            where TItemId : IEntityKey
        {
            var serviceResolver = initParameter.ServiceResolver;
            var ls = GetOrCreateLightSystem<TGameContext, TItemId>(serviceResolver);

            var refreshLocalSenseState =
                registry.BuildSystem()
                        .WithContext<TGameContext>()
                        .CreateSystem<SensoryReceptorData<TReceptorSense, TSourceSense>, SensoryReceptorState<TReceptorSense, TSourceSense>, SenseReceptorDirtyFlag<TReceptorSense, TSourceSense>>(
                            ls.RefreshLocalReceptorState);
            context.AddInitializationStepHandler(refreshLocalSenseState);
            context.AddFixedStepHandlers(refreshLocalSenseState);
        }

        protected void RegisterCalculateOmniDirectionalSystem<TGameContext, TItemId>(in ModuleInitializationParameter initParameter,
                                                                                     IGameLoopSystemRegistration<TGameContext> context,
                                                                                     EntityRegistry<TItemId> registry,
                                                                                     ICommandHandlerRegistration<TGameContext, TItemId> handler)
            where TItemId : IEntityKey
        {
            var serviceResolver = initParameter.ServiceResolver;
            var ls = GetOrCreateLightSystem<TGameContext, TItemId>(serviceResolver);

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
                                                                                    EntityRegistry<TItemId> registry,
                                                                                    ICommandHandlerRegistration<TGameContext, TItemId> handler)
            where TItemId : IEntityKey
        {
            var serviceResolver = initParameter.ServiceResolver;
            var ls = GetOrCreateLightSystem<TGameContext, TItemId>(serviceResolver);

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
                                                                     EntityRegistry<TItemId> registry,
                                                                     ICommandHandlerRegistration<TGameContext, TItemId> handler)
            where TItemId : IEntityKey
        {
            var serviceResolver = initParameter.ServiceResolver;
            var ls = GetOrCreateLightSystem<TGameContext, TItemId>(serviceResolver);

            var clearReceptorStateSystem =
                registry.BuildSystem()
                        .WithContext<TGameContext>()
                        .CreateSystem<SensoryReceptorData<TReceptorSense, TSourceSense>, SensoryReceptorState<TReceptorSense, TSourceSense>, SenseReceptorDirtyFlag<TReceptorSense, TSourceSense>>(
                            ls.ResetReceptorCacheState);

            context.AddInitializationStepHandler(clearReceptorStateSystem);
            context.AddFixedStepHandlers(clearReceptorStateSystem);

            var clearObservedStateSystem =
                registry.BuildSystem()
                        .WithContext<TGameContext>()
                        .CreateSystem<ObservedSenseSource<TSourceSense>>(ls.ResetSenseSourceObservedState);

            context.AddInitializationStepHandler(clearObservedStateSystem);
            context.AddFixedStepHandlers(clearObservedStateSystem);


            context.AddInitializationStepHandler(ls.EndSenseCalculation);
            context.AddFixedStepHandlers(ls.EndSenseCalculation);
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


        protected abstract (ISensePropagationAlgorithm, ISensePhysics) GetOrCreatePhysics(IServiceResolver serviceResolver);

        protected virtual SenseReceptorSystem<TReceptorSense, TSourceSense> GetOrCreateLightSystem<TGameContext, TItemId>(IServiceResolver serviceResolver)
        {
            if (!serviceResolver.TryResolve(out SenseReceptorSystem<TReceptorSense, TSourceSense> ls))
            {
                var physics = GetOrCreatePhysics(serviceResolver);
                ls = new SenseReceptorSystem<TReceptorSense, TSourceSense>(serviceResolver.ResolveToReference<IReadOnlyDynamicDataView3D<SensoryResistance<TReceptorSense>>>(),
                                                                           serviceResolver.ResolveToReference<ISenseStateCacheProvider>(),
                                                                           serviceResolver.ResolveToReference<IGlobalSenseStateCacheProvider>(),
                                                                           serviceResolver.ResolveToReference<ITimeSource>(),
                                                                           GetOrCreateDirectionalitySystem<TGameContext, TItemId>(serviceResolver),
                                                                           physics.Item2,
                                                                           physics.Item1);
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
        }
    }
}