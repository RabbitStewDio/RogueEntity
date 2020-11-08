using EnTTSharp.Entities;
using EnTTSharp.Entities.Systems;
using RogueEntity.Core.GridProcessing.LayerAggregation;
using RogueEntity.Core.Infrastructure.Commands;
using RogueEntity.Core.Infrastructure.GameLoops;
using RogueEntity.Core.Infrastructure.Modules;
using RogueEntity.Core.Infrastructure.Time;
using RogueEntity.Core.Positioning.Continuous;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Sensing.Cache;
using RogueEntity.Core.Sensing.Common;
using RogueEntity.Core.Sensing.Common.Physics;
using RogueEntity.Core.Sensing.Resistance;
using RogueEntity.Core.Sensing.Resistance.Directions;
using RogueEntity.Core.Sensing.Resistance.Maps;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.DataViews;
using Serilog;

namespace RogueEntity.Core.Sensing.Sources
{
    public abstract class SenseSourceModuleBase<TSense, TSenseSourceDefinition> : ModuleBase
        where TSense : ISense
        where TSenseSourceDefinition : ISenseDefinition
    {
        readonly static ILogger Logger = SLog.ForContext<SenseSourceModuleBase<TSense, TSenseSourceDefinition>>();

        protected void RegisterResistanceSystemLifecycle<TGameContext, TItemId>(IServiceResolver resolver,
                                                                                IGameLoopSystemRegistration<TGameContext> context,
                                                                                EntityRegistry<TItemId> registry,
                                                                                ICommandHandlerRegistration<TGameContext, TItemId> handler)
            where TItemId : IEntityKey
        {
            var system = GetOrCreateSensePropertiesSystem<TGameContext, TItemId>(resolver);
            context.AddInitializationStepHandler(system.Start);
            context.AddDisposeStepHandler(system.Stop);
        }


        protected void RegisterResistanceSystemExecution<TGameContext, TItemId>(IServiceResolver serviceResolver,
                                                                                IGameLoopSystemRegistration<TGameContext> context,
                                                                                EntityRegistry<TItemId> registry,
                                                                                ICommandHandlerRegistration<TGameContext, TItemId> handler)
            where TItemId : IEntityKey
        {
            var system = GetOrCreateSensePropertiesSystem<TGameContext, TItemId>(serviceResolver);

            context.AddInitializationStepHandler(system.ProcessSenseProperties);
            context.AddFixedStepHandlers(system.ProcessSenseProperties);
        }

        protected void RegisterPrepareSystem<TGameContext, TItemId>(IServiceResolver serviceResolver,
                                                                    IGameLoopSystemRegistration<TGameContext> context,
                                                                    EntityRegistry<TItemId> registry,
                                                                    ICommandHandlerRegistration<TGameContext, TItemId> handler)
            where TItemId : IEntityKey
        {
            var ls = GetOrCreateSenseSystem<TGameContext, TItemId>(serviceResolver);
            context.AddInitializationStepHandler(ls.EnsureSenseCacheAvailable);
            context.AddInitializationStepHandler(ls.BeginSenseCalculation);
            context.AddFixedStepHandlers(ls.BeginSenseCalculation);
        }

        protected void RegisterCollectLightsGridSystem<TGameContext, TItemId>(IServiceResolver resolver,
                                                                              IGameLoopSystemRegistration<TGameContext> context,
                                                                              EntityRegistry<TItemId> registry,
                                                                              ICommandHandlerRegistration<TGameContext, TItemId> handler)
            where TItemId : IEntityKey
        {
            var ls = GetOrCreateSenseSystem<TGameContext, TItemId>(resolver);
            var system = registry.BuildSystem().WithContext<TGameContext>().CreateSystem<TSenseSourceDefinition, SenseSourceState<TSense>, ContinuousMapPosition>(ls.FindDirtySenseSources);
            context.AddInitializationStepHandler(system);
            context.AddFixedStepHandlers(system);
        }

        protected void RegisterCollectLightsContinuousSystem<TGameContext, TItemId>(IServiceResolver resolver,
                                                                                    IGameLoopSystemRegistration<TGameContext> context,
                                                                                    EntityRegistry<TItemId> registry,
                                                                                    ICommandHandlerRegistration<TGameContext, TItemId> handler)
            where TItemId : IEntityKey
        {
            var ls = GetOrCreateSenseSystem<TGameContext, TItemId>(resolver);
            var system = registry.BuildSystem()
                                 .WithContext<TGameContext>()
                                 .CreateSystem<TSenseSourceDefinition, SenseSourceState<TSense>, EntityGridPosition>(ls.FindDirtySenseSources);
            context.AddInitializationStepHandler(system);
            context.AddFixedStepHandlers(system);
        }

        protected void RegisterCalculateSystem<TGameContext, TItemId>(IServiceResolver serviceResolver,
                                                                      IGameLoopSystemRegistration<TGameContext> context,
                                                                      EntityRegistry<TItemId> registry,
                                                                      ICommandHandlerRegistration<TGameContext, TItemId> handler)
            where TItemId : IEntityKey
        {
            var ls = GetOrCreateSenseSystem<TGameContext, TItemId>(serviceResolver);
            var refreshLocalSenseState =
                registry.BuildSystem()
                        .WithContext<TGameContext>()
                        .CreateSystem<TSenseSourceDefinition, SenseSourceState<TSense>, SenseDirtyFlag<TSense>, ObservedSenseSource<TSense>>(ls.RefreshLocalSenseState);

            context.AddInitializationStepHandler(refreshLocalSenseState);
            context.AddFixedStepHandlers(refreshLocalSenseState);
        }

        protected void RegisterCleanUpSystem<TGameContext, TItemId>(IServiceResolver serviceResolver,
                                                                    IGameLoopSystemRegistration<TGameContext> context,
                                                                    EntityRegistry<TItemId> registry,
                                                                    ICommandHandlerRegistration<TGameContext, TItemId> handler)
            where TItemId : IEntityKey
        {
            var ls = GetOrCreateSenseSystem<TGameContext, TItemId>(serviceResolver);
            context.AddInitializationStepHandler(ls.EndSenseCalculation);
            context.AddFixedStepHandlers(ls.EndSenseCalculation);
            context.AddDisposeStepHandler(ls.ShutDown);
        }

        protected void RegisterProcessSenseDirectionalitySystem<TGameContext, TItemId>(IServiceResolver serviceResolver,
                                                                                       IGameLoopSystemRegistration<TGameContext> context,
                                                                                       EntityRegistry<TItemId> registry,
                                                                                       ICommandHandlerRegistration<TGameContext, TItemId> handler)
            where TItemId : IEntityKey
        {
            var system = GetOrCreateDirectionalitySystem<TGameContext, TItemId>(serviceResolver);

            if (!serviceResolver.TryResolve(out DirectionalitySystemRegisteredMarker _))
            {
                serviceResolver.Store(new DirectionalitySystemRegisteredMarker());
                context.AddInitializationStepHandler(c => system.MarkGloballyDirty());
                context.AddInitializationStepHandler(system.ProcessSystem);
                context.AddInitializationStepHandler(system.MarkCleanSystem);
                context.AddFixedStepHandlers(system.ProcessSystem);
                context.AddFixedStepHandlers(system.MarkCleanSystem);
            }
        }

        protected void RegisterSenseResistanceCacheLifeCycle<TGameContext, TItemId, TSense>(IServiceResolver resolver,
                                                                                            IGameLoopSystemRegistration<TGameContext> context,
                                                                                            EntityRegistry<TItemId> registry,
                                                                                            ICommandHandlerRegistration<TGameContext, TItemId> handler)
            where TItemId : IEntityKey
        {
            // Ensure that the connection is only made once. The sense properties system will
            // forward all invalidation events to the sense cache.
            if (!resolver.TryResolve(out SensePropertiesConnectorSystem<TGameContext, TSense> system))
            {
                if (!resolver.TryResolve(out SensePropertiesSystem<TGameContext, TSense> sensePropertiesSystem))
                {
                    Logger.Verbose("Not registering SensePropertiesConnector. No Sense Properties System defined.");
                }

                if (!resolver.TryResolve(out SenseStateCache cacheProvider))
                {
                    Logger.Warning("Not registering SensePropertiesConnector. No SenseStateCache defined.");
                    return;
                }

                system = new SensePropertiesConnectorSystem<TGameContext, TSense>(sensePropertiesSystem, cacheProvider);
                resolver.Store(system);

                context.AddInitializationStepHandler(system.Start);
                context.AddDisposeStepHandler(system.Stop);
            }
        }

        readonly struct DirectionalitySystemRegisteredMarker
        {
        }

        protected virtual SensePropertiesSystem<TGameContext, TSense> GetOrCreateSensePropertiesSystem<TGameContext, TEntityId>(IServiceResolver serviceResolver)
        {
            if (serviceResolver.TryResolve(out SensePropertiesSystem<TGameContext, TSense> system))
            {
                return system;
            }

            var gridConfig = serviceResolver.Resolve<IGridMapConfiguration<TEntityId>>();
            system = new SensePropertiesSystem<TGameContext, TSense>(gridConfig.OffsetX, gridConfig.OffsetY, gridConfig.TileSizeX, gridConfig.TileSizeY);

            serviceResolver.Store(system);
            serviceResolver.Store<IAggregationLayerSystem<TGameContext, SensoryResistance<TSense>>>(system);
            serviceResolver.Store<IReadOnlyDynamicDataView3D<SensoryResistance<TSense>>>(system);
            return system;
        }

        protected virtual SensoryResistanceDirectionalitySystem<TSense> GetOrCreateDirectionalitySystem<TGameContext, TItemId>(IServiceResolver serviceResolver)
        {
            if (serviceResolver.TryResolve(out SensoryResistanceDirectionalitySystem<TSense> system))
            {
                return system;
            }

            if (!serviceResolver.TryResolve(out IReadOnlyDynamicDataView3D<SensoryResistance<TSense>> data))
            {
                data = GetOrCreateSensePropertiesSystem<TGameContext, TItemId>(serviceResolver);
            }

            system = new SensoryResistanceDirectionalitySystem<TSense>(data);
            serviceResolver.Store(system);
            serviceResolver.Store<ISensoryResistanceDirectionView<TSense>>(system);
            return system;
        }

        protected virtual SenseSourceSystem<TSense, TSenseSourceDefinition> GetOrCreateSenseSystem<TGameContext, TItemId>(IServiceResolver serviceResolver)
        {
            var physicsConfig = GetOrCreateSensePhysics(serviceResolver);
            if (serviceResolver.TryResolve(out SenseSourceSystem<TSense, TSenseSourceDefinition> ls))
            {
                return ls;
            }

            if (!serviceResolver.TryResolve(out ISensoryResistanceDirectionView<TSense> senseDirections))
            {
                senseDirections = GetOrCreateDirectionalitySystem<TGameContext, TItemId>(serviceResolver);
            }

            ls = new SenseSourceSystem<TSense, TSenseSourceDefinition>(serviceResolver.ResolveToReference<IReadOnlyDynamicDataView3D<SensoryResistance<TSense>>>(),
                                                                     serviceResolver.ResolveToReference<IGlobalSenseStateCacheProvider>(),
                                                                     serviceResolver.ResolveToReference<ITimeSource>(),
                                                                     senseDirections,
                                                                     serviceResolver.Resolve<ISenseStateCacheControl>(),
                                                                     physicsConfig.Item1,
                                                                     physicsConfig.Item2);
            serviceResolver.Store(ls);
            return ls;
        }

        protected abstract (ISensePropagationAlgorithm, ISensePhysics) GetOrCreateSensePhysics(IServiceResolver resolver);

        protected void RegisterEntities<TItemId>(IServiceResolver serviceResolver, EntityRegistry<TItemId> registry)
            where TItemId : IEntityKey
        {
            registry.RegisterNonConstructable<SensoryResistance<TSense>>();
            registry.RegisterNonConstructable<TSenseSourceDefinition>();
            registry.RegisterNonConstructable<SenseSourceState<TSense>>();
            registry.RegisterFlag<ObservedSenseSource<TSense>>();
            registry.RegisterFlag<SenseDirtyFlag<TSense>>();
        }
    }
}