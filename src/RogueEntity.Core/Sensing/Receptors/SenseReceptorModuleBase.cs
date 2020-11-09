using EnTTSharp.Entities;
using EnTTSharp.Entities.Systems;
using RogueEntity.Core.GridProcessing.LayerAggregation;
using RogueEntity.Core.Infrastructure.Commands;
using RogueEntity.Core.Infrastructure.GameLoops;
using RogueEntity.Core.Infrastructure.Modules;
using RogueEntity.Core.Infrastructure.Modules.Services;
using RogueEntity.Core.Infrastructure.Time;
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
        protected void RegisterPrepareSystem<TGameContext, TItemId>(IServiceResolver serviceResolver,
                                                                    IGameLoopSystemRegistration<TGameContext> context,
                                                                    EntityRegistry<TItemId> registry,
                                                                    ICommandHandlerRegistration<TGameContext, TItemId> handler)
            where TItemId : IEntityKey
        {
            var ls = GetOrCreateLightSystem<TGameContext, TItemId>(serviceResolver);
            context.AddInitializationStepHandler(ls.EnsureSenseCacheAvailable);
            context.AddInitializationStepHandler(ls.BeginSenseCalculation);
            context.AddFixedStepHandlers(ls.BeginSenseCalculation);
        }

        protected void RegisterCollectReceptorsGridSystem<TGameContext, TItemId>(IServiceResolver resolver,
                                                                                 IGameLoopSystemRegistration<TGameContext> context,
                                                                                 EntityRegistry<TItemId> registry,
                                                                                 ICommandHandlerRegistration<TGameContext, TItemId> handler)
            where TItemId : IEntityKey
        {
            var ls = GetOrCreateLightSystem<TGameContext, TItemId>(resolver);
            var system = registry.BuildSystem()
                                 .WithContext<TGameContext>()
                                 .CreateSystem<SensoryReceptorData<TReceptorSense, TSourceSense>, SensoryReceptorState<TReceptorSense, TSourceSense>, EntityGridPosition>(ls.CollectReceptor);
            context.AddInitializationStepHandler(system);
            context.AddFixedStepHandlers(system);
        }

        protected void RegisterCollectReceptorsContinuousSystem<TGameContext, TItemId>(IServiceResolver resolver,
                                                                                       IGameLoopSystemRegistration<TGameContext> context,
                                                                                       EntityRegistry<TItemId> registry,
                                                                                       ICommandHandlerRegistration<TGameContext, TItemId> handler)
            where TItemId : IEntityKey
        {
            var ls = GetOrCreateLightSystem<TGameContext, TItemId>(resolver);
            var system = registry.BuildSystem()
                                 .WithContext<TGameContext>()
                                 .CreateSystem<SensoryReceptorData<TReceptorSense, TSourceSense>, SensoryReceptorState<TReceptorSense, TSourceSense>, ContinuousMapPosition>(ls.CollectReceptor);
            context.AddInitializationStepHandler(system);
            context.AddFixedStepHandlers(system);
        }

        protected void RegisterCollectSenseSourcesSystem<TGameContext, TItemId>(IServiceResolver serviceResolver,
                                                                                IGameLoopSystemRegistration<TGameContext> context,
                                                                                EntityRegistry<TItemId> registry,
                                                                                ICommandHandlerRegistration<TGameContext, TItemId> handler)
            where TItemId : IEntityKey
        {
            var ls = GetOrCreateLightSystem<TGameContext, TItemId>(serviceResolver);
            var system = registry.BuildSystem()
                                 .WithContext<TGameContext>()
                                 .CreateSystem<TSenseSource, SenseSourceState<TSourceSense>>(ls.CollectObservedSenseSource);
            context.AddInitializationStepHandler(system);
            context.AddFixedStepHandlers(system);
        }

        protected void RegisterComputeReceptorFieldOfView<TGameContext, TItemId>(IServiceResolver serviceResolver,
                                                                                 IGameLoopSystemRegistration<TGameContext> context,
                                                                                 EntityRegistry<TItemId> registry,
                                                                                 ICommandHandlerRegistration<TGameContext, TItemId> handler)
            where TItemId : IEntityKey
        {
            var ls = GetOrCreateLightSystem<TGameContext, TItemId>(serviceResolver);

            var refreshLocalSenseState =
                registry.BuildSystem()
                        .WithContext<TGameContext>()
                        .CreateSystem<SensoryReceptorData<TReceptorSense, TSourceSense>, SensoryReceptorState<TReceptorSense, TSourceSense>, SenseReceptorDirtyFlag<TReceptorSense, TSourceSense>>(
                            ls.RefreshLocalReceptorState);
            context.AddInitializationStepHandler(refreshLocalSenseState);
            context.AddFixedStepHandlers(refreshLocalSenseState);
        }

        protected void RegisterCalculateOmniDirectionalSystem<TGameContext, TItemId>(IServiceResolver serviceResolver,
                                                                                     IGameLoopSystemRegistration<TGameContext> context,
                                                                                     EntityRegistry<TItemId> registry,
                                                                                     ICommandHandlerRegistration<TGameContext, TItemId> handler)
            where TItemId : IEntityKey
        {
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

        protected void RegisterCalculateUniDirectionalSystem<TGameContext, TItemId>(IServiceResolver serviceResolver,
                                                                                    IGameLoopSystemRegistration<TGameContext> context,
                                                                                    EntityRegistry<TItemId> registry,
                                                                                    ICommandHandlerRegistration<TGameContext, TItemId> handler)
            where TItemId : IEntityKey
        {
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


        protected void RegisterFinalizeSystem<TGameContext, TItemId>(IServiceResolver serviceResolver,
                                                                     IGameLoopSystemRegistration<TGameContext> context,
                                                                     EntityRegistry<TItemId> registry,
                                                                     ICommandHandlerRegistration<TGameContext, TItemId> handler)
            where TItemId : IEntityKey
        {
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

        protected void RegisterEntities<TItemId>(IServiceResolver serviceResolver, EntityRegistry<TItemId> registry)
            where TItemId : IEntityKey
        {
            registry.RegisterNonConstructable<SensoryReceptorData<TReceptorSense, TSourceSense>>();
            registry.RegisterNonConstructable<SensoryReceptorState<TReceptorSense, TSourceSense>>();
            registry.RegisterNonConstructable<SingleLevelSenseDirectionMapData<TReceptorSense, TSourceSense>>();
            registry.RegisterFlag<SenseReceptorDirtyFlag<TReceptorSense, TSourceSense>>();
        }
    }
}