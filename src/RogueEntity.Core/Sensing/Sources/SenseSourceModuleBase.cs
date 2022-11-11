using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using EnTTSharp.Entities;
using EnTTSharp.Entities.Systems;
using RogueEntity.Api.GameLoops;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Modules;
using RogueEntity.Api.Modules.Attributes;
using RogueEntity.Api.Modules.Helpers;
using RogueEntity.Api.Services;
using RogueEntity.Api.Time;
using RogueEntity.Api.Utils;
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
using Serilog;

namespace RogueEntity.Core.Sensing.Sources
{
    /// <summary>
    ///   Registers light source calculation entities.
    /// </summary>
    /// <remarks>
    ///   Defines the following systems:
    ///
    ///   5000 - preparation: Clear collected sources, fetch current time, etc.
    ///   5700 - collect all sense sources that are dirty. Specialized handling for grid and continuous positions.
    ///   5800 - recompute those collected sense sources.
    ///   5900 - clean up, mark all processed sources as clean.
    /// </remarks>
    public abstract class SenseSourceModuleBase<TSense, TSenseSourceDefinition> : ModuleBase
        where TSense : ISense
        where TSenseSourceDefinition : ISenseDefinition
    {
        static readonly ILogger logger = SLog.ForContext<SenseSourceModuleBase<TSense, TSenseSourceDefinition>>();

        public static readonly EntityRole SenseSourceRole = SenseSourceModules.GetSourceRole<TSense>();
        public static readonly EntityRole ResistanceDataProviderRole = SenseSourceModules.GetResistanceRole<TSense>();

        public static readonly EntitySystemId PreparationSystemId = SenseSourceModules.CreateSystemId<TSense>("Sources.Prepare");
        public static readonly EntitySystemId CollectionGridSystemId = SenseSourceModules.CreateSystemId<TSense>("Sources.Collect.Grid");
        public static readonly EntitySystemId CollectionContinuousSystemId = SenseSourceModules.CreateSystemId<TSense>("Sources.Collect.Continuous");
        public static readonly EntitySystemId ComputeSystemId = SenseSourceModules.CreateSystemId<TSense>("Sources.Compute");
        public static readonly EntitySystemId FinalizeSystemId = SenseSourceModules.CreateSystemId<TSense>("Sources.Finalize");

        public static readonly EntitySystemId RegisterResistanceEntitiesId = SenseSourceModules.CreateEntityId<TSense>("Resistance");
        public static readonly EntitySystemId RegisterResistanceSystem = SenseSourceModules.CreateSystemId<TSense>("Resistance.LifeCycle");
        public static readonly EntitySystemId ExecuteResistanceSystem = SenseSourceModules.CreateSystemId<TSense>("Resistance.Run");

        public static readonly EntitySystemId SenseCacheLifecycleId = SenseSourceModules.CreateSystemId<TSense>("CacheLifeCycle");

        public static readonly EntitySystemId RegisterEntityId = SenseSourceModules.CreateEntityId<TSense>("Sources");

        public static readonly EntityRelation NeedSenseResistanceRelation = SenseSourceModules.GetResistanceRelation<TSense>();

        protected SenseSourceModuleBase()
        {
            DeclareDependencies(ModuleDependency.Of(SensoryCacheModule.ModuleId),
                                ModuleDependency.Of(PositionModule.ModuleId));

            RequireRole(SenseSourceRole).WithImpliedRole(SenseSources.SenseSourceRole).WithImpliedRole(SensoryCacheModule.SenseCacheSourceRole);
            ForRole(ResistanceDataProviderRole).WithImpliedRole(SensoryCacheModule.SenseCacheSourceRole);

            RequireRelation(NeedSenseResistanceRelation);
        }

        [InitializerCollector(InitializerCollectorType.Roles)]
        public IEnumerable<ModuleEntityRoleInitializerInfo<TItemId>> CollectRoleInitializers<TItemId>(IServiceResolver serviceResolver,
                                                                                                      IModuleEntityInformation entityInformation,
                                                                                                      EntityRole role)
            where TItemId : struct, IEntityKey
        {
            if (role == SenseSourceRole)
            {
                yield return ModuleEntityRoleInitializerInfo.CreateFor<TItemId>
                    (SenseSourceRole, InitializeSenseSourceRole);

                // activate for any sense source that is also grid positioned
                yield return ModuleEntityRoleInitializerInfo.CreateFor<TItemId>(SenseSourceRole, InitializeSenseSourceCollectionGrid)
                                                            .WithRequiredRoles(GridPositionModule.GridPositionedRole);

                // activate for any sense source that is also continuously positioned
                yield return ModuleEntityRoleInitializerInfo.CreateFor<TItemId>(SenseSourceRole, InitializeSenseSourceCollectionContinuous)
                                                            .WithRequiredRoles(ContinuousPositionModule.ContinuousPositionedRole);

                // all sense sources make use of the sense-cache.
                yield return ModuleEntityRoleInitializerInfo.CreateFor<TItemId>(SenseSourceRole, InitializeSenseCache)
                                                            .WithRequiredRoles(GridPositionModule.GridPositionedRole);
            }

            if (role == ResistanceDataProviderRole)
            {
                yield return ModuleEntityRoleInitializerInfo.CreateFor<TItemId>(ResistanceDataProviderRole, InitializeResistanceEntities);
                yield return ModuleEntityRoleInitializerInfo.CreateFor<TItemId>(ResistanceDataProviderRole, InitializeResistanceRole)
                                                            .WithRequiredRoles(GridPositionModule.GridPositionedRole)
                                                            .WithRequiredRelations(NeedSenseResistanceRelation);
            }
        }

        [FinalizerCollector(InitializerCollectorType.Roles)]
        public IEnumerable<ModuleEntityRoleInitializerInfo<TItemId>> CollectRoleFinalizers<TItemId>(IServiceResolver serviceResolver,
                                                                                                    IModuleEntityInformation entityInformation,
                                                                                                    EntityRole role)
            where TItemId : struct, IEntityKey
        {
            if (role != ResistanceDataProviderRole)
            {
                yield break;
            }

            if (entityInformation.HasRelation(ResistanceDataProviderRole, NeedSenseResistanceRelation))
            {
                yield return ModuleEntityRoleInitializerInfo.CreateFor<TItemId>(ResistanceDataProviderRole, InitializeResistanceRelation)
                                                            .WithRequiredRoles(GridPositionModule.GridPositionedRole);
            }
        }

        void InitializeResistanceRelation<TItemId>(in ModuleEntityInitializationParameter<TItemId> initParameter,
                                                   IModuleInitializer initializer,
                                                   EntityRole role)
            where TItemId : struct, IEntityKey
        {
            var layers = PositionModuleServices.CollectMapLayers<TItemId, SensoryResistance<TSense>>(initParameter);
            var mapLayers = layers.ToList();
            logger.Debug("{Sense} will use map layers {Layers} as resistance source for {EntityId}", typeof(TSense), mapLayers, typeof(TItemId));
            
            var systemId = SenseSourceModules.CreateResistanceSourceSystemId<TSense>();
            var ctx = initializer.DeclareEntityContext<TItemId>();
            ctx.Register(systemId, 1100, SenseSourceModules.RegisterSenseResistanceSourceLayer<TItemId, TSense>(mapLayers));
        }


        protected void InitializeSenseSourceRole<TItemId>(in ModuleEntityInitializationParameter<TItemId> initParameter,
                                                          IModuleInitializer initializer,
                                                          EntityRole role)
            where TItemId : struct, IEntityKey
        {
            var ctx = initializer.DeclareEntityContext<TItemId>();
            ctx.Register(RegisterEntityId, 0, RegisterSenseSourceEntities);
            ctx.Register(PreparationSystemId, 50000, RegisterPrepareSenseSourceSystem);
            ctx.Register(ComputeSystemId, 58000, RegisterCalculateSenseSourceStateSystem);
            ctx.Register(FinalizeSystemId, 59000, RegisterCleanUpSystem);

            // Sense caching is only required if there is at least one sense source active. 
            ctx.Register(SenseCacheLifecycleId, 500, RegisterSenseResistanceCacheLifeCycle);
        }

        protected void InitializeSenseSourceCollectionGrid<TItemId>(in ModuleEntityInitializationParameter<TItemId> initParameter,
                                                                    IModuleInitializer initializer,
                                                                    EntityRole role)
            where TItemId : struct, IEntityKey
        {
            var ctx = initializer.DeclareEntityContext<TItemId>();
            ctx.Register(CollectionGridSystemId, 55000, RegisterCollectSenseSourceGridSystem);
        }

        protected void InitializeSenseSourceCollectionContinuous<TItemId>(in ModuleEntityInitializationParameter<TItemId> initParameter,
                                                                          IModuleInitializer initializer,
                                                                          EntityRole role)
            where TItemId : struct, IEntityKey
        {
            var ctx = initializer.DeclareEntityContext<TItemId>();
            ctx.Register(CollectionGridSystemId, 55000, RegisterCollectSenseSourceContinuousSystem);
        }

        [SuppressMessage("ReSharper", "UnusedTypeParameter")]
        protected void InitializeSenseCache<TItemId>(in ModuleEntityInitializationParameter<TItemId> initParameter,
                                                     IModuleInitializer initializer,
                                                     EntityRole role)
            where TItemId : struct, IEntityKey
        {
            if (initParameter.ServiceResolver.TryResolve<SenseCacheSetUpSystem>(out var o))
            {
                o.RegisterSense<TSense>();
            }
        }

        protected void InitializeResistanceEntities<TItemId>(in ModuleEntityInitializationParameter<TItemId> initParameter,
                                                             IModuleInitializer initializer,
                                                             EntityRole role)
            where TItemId : struct, IEntityKey
        {
            var ctx = initializer.DeclareEntityContext<TItemId>();
            ctx.Register(RegisterResistanceEntitiesId, 0, RegisterResistanceEntities);
        }

        protected void InitializeResistanceRole<TItemId>(in ModuleEntityInitializationParameter<TItemId> initParameter,
                                                         IModuleInitializer initializer,
                                                         EntityRole role)
            where TItemId : struct, IEntityKey
        {
            // The sense resistance system is only needed if there is at least one sense source of this type active.

            var ctx = initializer.DeclareEntityContext<TItemId>();
            ctx.Register(RegisterResistanceSystem, 500, RegisterResistanceSystemLifecycle);
            ctx.Register(ExecuteResistanceSystem, 51000, RegisterResistanceSystemExecution);
            ctx.Register(ExecuteResistanceSystem, 52000, RegisterProcessSenseDirectionalitySystem);
        }

        protected void RegisterResistanceSystemLifecycle<TItemId>(in ModuleEntityInitializationParameter<TItemId> initParameter,
                                                                  IGameLoopSystemRegistration context,
                                                                  EntityRegistry<TItemId> registry)
            where TItemId : struct, IEntityKey
        {
            var serviceResolver = initParameter.ServiceResolver;
            var system = GetOrCreateSensePropertiesSystem<TItemId>(serviceResolver);
            context.AddInitializationStepHandler(system.Start);
            context.AddDisposeStepHandler(system.Stop);
        }


        protected void RegisterResistanceSystemExecution<TItemId>(in ModuleEntityInitializationParameter<TItemId> initParameter,
                                                                  IGameLoopSystemRegistration context,
                                                                  EntityRegistry<TItemId> registry)
            where TItemId : struct, IEntityKey
        {
            var serviceResolver = initParameter.ServiceResolver;
            var system = GetOrCreateSensePropertiesSystem<TItemId>(serviceResolver);

            context.AddInitializationStepHandler(system.ProcessLayerData);
            context.AddFixedStepHandlers(system.ProcessLayerData);
        }

        protected void RegisterPrepareSenseSourceSystem<TItemId>(in ModuleEntityInitializationParameter<TItemId> initParameter,
                                                                 IGameLoopSystemRegistration context,
                                                                 EntityRegistry<TItemId> registry)
            where TItemId : struct, IEntityKey
        {
            var serviceResolver = initParameter.ServiceResolver;
            var ls = GetOrCreateSenseSourceSystem<TItemId>(serviceResolver);
            context.AddInitializationStepHandler(ls.EnsureSenseCacheAvailable);
            context.AddInitializationStepHandler(ls.BeginSenseCalculation);
            context.AddFixedStepHandlers(ls.BeginSenseCalculation);
        }

        protected void RegisterCollectSenseSourceGridSystem<TItemId>(in ModuleEntityInitializationParameter<TItemId> initParameter,
                                                                     IGameLoopSystemRegistration context,
                                                                     EntityRegistry<TItemId> registry)
            where TItemId : struct, IEntityKey
        {
            var serviceResolver = initParameter.ServiceResolver;
            var ls = GetOrCreateSenseSourceSystem<TItemId>(serviceResolver);
            var system = registry.BuildSystem()
                                 .WithoutContext()
                                 .WithInputParameter<TSenseSourceDefinition, EntityGridPosition>()
                                 .WithOutputParameter<SenseSourceState<TSense>>()
                                 .CreateSystem(ls.FindDirtySenseSources);
            context.AddInitializationStepHandlerSystem(system);
            context.AddFixedStepHandlerSystem(system);
        }

        protected void RegisterCollectSenseSourceContinuousSystem<TItemId>(in ModuleEntityInitializationParameter<TItemId> initParameter,
                                                                           IGameLoopSystemRegistration context,
                                                                           EntityRegistry<TItemId> registry)
            where TItemId : struct, IEntityKey
        {
            var serviceResolver = initParameter.ServiceResolver;
            var ls = GetOrCreateSenseSourceSystem<TItemId>(serviceResolver);
            var system = registry.BuildSystem()
                                 .WithoutContext()
                                 .WithInputParameter<TSenseSourceDefinition, ContinuousMapPosition>()
                                 .WithOutputParameter<SenseSourceState<TSense>>()
                                 .CreateSystem(ls.FindDirtySenseSources);
            context.AddInitializationStepHandlerSystem(system);
            context.AddFixedStepHandlerSystem(system);
        }

        protected void RegisterCalculateSenseSourceStateSystem<TItemId>(in ModuleEntityInitializationParameter<TItemId> initParameter,
                                                                        IGameLoopSystemRegistration context,
                                                                        EntityRegistry<TItemId> registry)
            where TItemId : struct, IEntityKey
        {
            var serviceResolver = initParameter.ServiceResolver;
            var ls = GetOrCreateSenseSourceSystem<TItemId>(serviceResolver);
            var refreshLocalSenseState =
                registry.BuildSystem()
                        .WithoutContext()
                        .WithInputParameter<TSenseSourceDefinition, SenseDirtyFlag<TSense>, ObservedSenseSource<TSense>>()
                        .WithOutputParameter<SenseSourceState<TSense>>()
                        .CreateSystem(ls.RefreshLocalSenseState);

            context.AddInitializationStepHandlerSystem(refreshLocalSenseState);
            context.AddFixedStepHandlerSystem(refreshLocalSenseState);
        }

        protected void RegisterCleanUpSystem<TItemId>(in ModuleEntityInitializationParameter<TItemId> initParameter,
                                                      IGameLoopSystemRegistration context,
                                                      EntityRegistry<TItemId> registry)
            where TItemId : struct, IEntityKey
        {
            var serviceResolver = initParameter.ServiceResolver;
            var ls = GetOrCreateSenseSourceSystem<TItemId>(serviceResolver);

            var resetSystem = registry.BuildSystem()
                                      .WithoutContext()
                                      .WithInputParameter<TSenseSourceDefinition, SenseDirtyFlag<TSense>>()
                                      .WithOutputParameter<SenseSourceState<TSense>>()
                                      .CreateSystem(ls.ResetSenseSourceCacheState);

            context.AddInitializationStepHandlerSystem(resetSystem);
            context.AddFixedStepHandlerSystem(resetSystem);

            context.AddInitializationStepHandler(ls.EndSenseCalculation);
            context.AddFixedStepHandlers(ls.EndSenseCalculation);

            context.AddDisposeStepHandler(ls.ShutDown);
        }

        protected void RegisterProcessSenseDirectionalitySystem<TItemId>(in ModuleEntityInitializationParameter<TItemId> initParameter,
                                                                         IGameLoopSystemRegistration context,
                                                                         EntityRegistry<TItemId> registry)
            where TItemId : struct, IEntityKey
        {
            var serviceResolver = initParameter.ServiceResolver;
            var system = GetOrCreateDirectionalitySystem<TItemId>(serviceResolver);

            if (!serviceResolver.TryResolve(out SenseDirectionalitySystemRegisteredMarker<TSense> _))
            {
                serviceResolver.Store(new SenseDirectionalitySystemRegisteredMarker<TSense>());
                context.AddInitializationStepHandler(system.MarkGloballyDirty);
                context.AddInitializationStepHandler(system.ProcessSystem);
                context.AddInitializationStepHandler(system.MarkCleanSystem);
                context.AddFixedStepHandlers(system.ProcessSystem);
                context.AddFixedStepHandlers(system.MarkCleanSystem);
            }
        }

        protected void RegisterSenseResistanceCacheLifeCycle<TItemId>(in ModuleEntityInitializationParameter<TItemId> initParameter,
                                                                      IGameLoopSystemRegistration context,
                                                                      EntityRegistry<TItemId> registry)
            where TItemId : struct, IEntityKey
        {
            var resolver = initParameter.ServiceResolver;
            // Ensure that the connection is only made once. The sense properties system will
            // forward all invalidation events to the sense cache.
            if (!resolver.TryResolve<SensePropertiesConnectorSystem<TSense>>(out var system))
            {
                if (!resolver.TryResolve<SensePropertiesSystem<TSense>>(out var sensePropertiesSystem))
                {
                    logger.Verbose("Not registering SensePropertiesConnector. No Sense Properties System defined");
                    return;
                }

                if (!resolver.TryResolve<SenseStateCache>(out var cacheProvider))
                {
                    logger.Warning("Not registering SensePropertiesConnector. No SenseStateCache defined");
                    return;
                }

                system = new SensePropertiesConnectorSystem<TSense>(sensePropertiesSystem, cacheProvider);
                resolver.Store(system);

                context.AddInitializationStepHandler(system.Start, nameof(system.Start));
                context.AddDisposeStepHandler(system.Stop, nameof(system.Stop));
            }
        }

        protected virtual SensePropertiesSystem<TSense> GetOrCreateSensePropertiesSystem<TEntityId>(IServiceResolver serviceResolver)
        {
            if (serviceResolver.TryResolve<SensePropertiesSystem<TSense>>(out var system))
            {
                return system;
            }

            var gridConfig = PositionModuleServices.LookupDefaultConfiguration<TEntityId>(serviceResolver);
            system = new SensePropertiesSystem<TSense>(gridConfig.OffsetX, gridConfig.OffsetY, gridConfig.TileSizeX, gridConfig.TileSizeY);

            serviceResolver.Store(system);
            serviceResolver.Store<IAggregationLayerSystemBackend<SensoryResistance<TSense>>>(system);
            serviceResolver.Store<ISensePropertiesDataView<TSense>>(system);
            return system;
        }

        protected virtual SensoryResistanceDirectionalitySystem<TSense> GetOrCreateDirectionalitySystem<TItemId>(IServiceResolver serviceResolver)
        {
            if (serviceResolver.TryResolve<SensoryResistanceDirectionalitySystem<TSense>>(out var system))
            {
                return system;
            }

            if (!serviceResolver.TryResolve<ISensePropertiesDataView<TSense>>(out var data))
            {
                data = GetOrCreateSensePropertiesSystem<TItemId>(serviceResolver);
            }

            system = new SensoryResistanceDirectionalitySystem<TSense>(data.ResultView);
            serviceResolver.Store(system);
            serviceResolver.Store<ISensoryResistanceDirectionView<TSense>>(system);
            return system;
        }

        protected virtual SenseSourceSystem<TSense, TSenseSourceDefinition> GetOrCreateSenseSourceSystem<TItemId>(IServiceResolver serviceResolver)
        {
            var physicsConfig = GetOrCreateSensePhysics(serviceResolver);
            if (serviceResolver.TryResolve<SenseSourceSystem<TSense, TSenseSourceDefinition>>(out var ls))
            {
                return ls;
            }

            if (!serviceResolver.TryResolve<ISensoryResistanceDirectionView<TSense>>(out var senseDirections))
            {
                senseDirections = GetOrCreateDirectionalitySystem<TItemId>(serviceResolver);
            }

            var senseProperties = serviceResolver.ResolveToReference<ISensePropertiesDataView<TSense>>().Map(l => l.ResultView);
            ls = new SenseSourceSystem<TSense, TSenseSourceDefinition>(senseProperties,
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

        protected void RegisterSenseSourceEntities<TItemId>(in ModuleEntityInitializationParameter<TItemId> initParameter,
                                                            EntityRegistry<TItemId> registry)
            where TItemId : struct, IEntityKey
        {
            registry.RegisterNonConstructable<TSenseSourceDefinition>();
            registry.RegisterNonConstructable<SenseSourceState<TSense>>();
            registry.RegisterFlag<ObservedSenseSource<TSense>>();
            registry.RegisterFlag<SenseDirtyFlag<TSense>>();
        }

        protected void RegisterResistanceEntities<TItemId>(in ModuleEntityInitializationParameter<TItemId> initParameter,
                                                           EntityRegistry<TItemId> registry)
            where TItemId : struct, IEntityKey
        {
            registry.RegisterNonConstructable<SensoryResistance<TSense>>();
        }
    }

    [SuppressMessage("ReSharper", "UnusedTypeParameter")]
    readonly struct SenseDirectionalitySystemRegisteredMarker<TSense>
    { }
}
