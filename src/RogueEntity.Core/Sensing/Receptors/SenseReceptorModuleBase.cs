using EnTTSharp.Entities;
using EnTTSharp.Entities.Systems;
using RogueEntity.Core.Infrastructure.Commands;
using RogueEntity.Core.Infrastructure.GameLoops;
using RogueEntity.Core.Infrastructure.Modules;
using RogueEntity.Core.Positioning.Continuous;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Sensing.Common.Blitter;
using RogueEntity.Core.Sensing.Sources;

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
            if (!GetOrCreateLightSystem(serviceResolver, out var ls))
            {
                return;
            }

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
            if (!GetOrCreateLightSystem(resolver, out var ls))
            {
                return;
            }

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
            if (!GetOrCreateLightSystem(resolver, out var ls))
            {
                return;
            }

            var system = registry.BuildSystem()
                                 .WithContext<TGameContext>()
                                 .CreateSystem<SensoryReceptorData<TReceptorSense, TSourceSense>, SensoryReceptorState<TReceptorSense, TSourceSense>, ContinuousMapPosition>(ls.CollectReceptor);
            context.AddInitializationStepHandler(system);
            context.AddFixedStepHandlers(system);
        }

        protected void RegisterCollectSenseSourcesSystem<TGameContext, TItemId>(IServiceResolver resolver,
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
                                 .CreateSystem<TSenseSource, SenseSourceState<TSourceSense>>(ls.CollectObservedSenseSource);
            context.AddInitializationStepHandler(system);
            context.AddFixedStepHandlers(system);
        }

        protected void RegisterComputeReceptorFieldOfView<TGameContext, TItemId>(IServiceResolver resolver,
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
            if (!GetOrCreateLightSystem(serviceResolver, out var ls))
            {
                return;
            }

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
            if (!GetOrCreateLightSystem(serviceResolver, out var ls))
            {
                return;
            }

            if (!serviceResolver.TryResolve(out IDirectionalSenseReceptorBlitter senseBlitter))
            {
                senseBlitter = new DefaultDirectionalSenseReceptorBlitter();
            }


            var uniSys = new SenseReceptorBlitterSystem<TReceptorSense, TSourceSense>(ls, senseBlitter);
            var system = registry.BuildSystem()
                                 .WithContext<TGameContext>()
                                 .CreateSystem<SingleLevelSenseDirectionMapData<TReceptorSense,TSourceSense>, SensoryReceptorState<TReceptorSense, TSourceSense>>(uniSys.CopySenseSourcesToVisionField);


            context.AddInitializationStepHandler(system);
            context.AddFixedStepHandlers(system);
        }


        protected void RegisterFinalizeSystem<TGameContext, TItemId>(IServiceResolver serviceResolver,
                                                                     IGameLoopSystemRegistration<TGameContext> context,
                                                                     EntityRegistry<TItemId> registry,
                                                                     ICommandHandlerRegistration<TGameContext, TItemId> handler)
            where TItemId : IEntityKey
        {
            if (!GetOrCreateLightSystem(serviceResolver, out var ls))
            {
                return;
            }

            var clearReceptorStateSystem =
                registry.BuildSystem()
                        .WithContext<TGameContext>()
                        .CreateSystem<SensoryReceptorData<TReceptorSense, TSourceSense>, SensoryReceptorState<TReceptorSense, TSourceSense>, SenseReceptorDirtyFlag<TReceptorSense, TSourceSense>>(ls.ResetReceptorCacheState);

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

        protected abstract bool GetOrCreateLightSystem(IServiceResolver serviceResolver, out SenseReceptorSystemBase<TReceptorSense, TSourceSense> ls);

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