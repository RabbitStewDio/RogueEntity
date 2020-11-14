using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using EnTTSharp.Entities;
using EnTTSharp.Entities.Systems;
using RogueEntity.Core.GridProcessing.LayerAggregation;
using RogueEntity.Core.Infrastructure.GameLoops;
using RogueEntity.Core.Infrastructure.ItemTraits;
using RogueEntity.Core.Infrastructure.Modules;
using RogueEntity.Core.Infrastructure.Modules.Attributes;
using RogueEntity.Core.Infrastructure.Modules.Helpers;
using RogueEntity.Core.Infrastructure.Modules.Services;
using RogueEntity.Core.Infrastructure.Time;
using RogueEntity.Core.Meta.Items;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Continuous;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Positioning.MapLayers;
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
        static readonly ILogger Logger = SLog.ForContext<SenseSourceModuleBase<TSense, TSenseSourceDefinition>>();

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

        protected SenseSourceModuleBase()
        {
            DeclareDependencies(ModuleDependency.Of(SensoryCacheModule.ModuleId),
                                ModuleDependency.Of(PositionModule.ModuleId));

            RequireRole(SenseSourceRole).WithImpliedRole(SenseSources.SenseSourceRole).WithImpliedRole(SensoryCacheModule.SenseCacheSourceRole);
            ForRole(ResistanceDataProviderRole).WithImpliedRole(SensoryCacheModule.SenseCacheSourceRole);
        }

        [InitializerCollectorAttribute(InitializerCollectorType.Roles)]
        public IEnumerable<ModuleEntityRoleInitializerInfo<TGameContext, TItemId>> CollectRoleInitializers<TGameContext, TItemId>(IServiceResolver serviceResolver,
                                                                                                                                  IModuleEntityInformation entityInformation,
                                                                                                                                  EntityRole role)
            where TItemId : IEntityKey
        {
            if (role == SenseSourceRole)
            {
                yield return ModuleEntityRoleInitializerInfo.CreateFor<TGameContext, TItemId>
                    (SenseSourceRole, InitializeSenseSourceRole);

                // activate for any sense source that is also grid positioned
                yield return ModuleEntityRoleInitializerInfo.CreateFor<TGameContext, TItemId>(SenseSourceRole, InitializeSenseSourceCollectionGrid)
                                                            .WithRequiredRoles(PositionModule.GridPositionedRole);

                // activate for any sense source that is also continuously positioned
                yield return ModuleEntityRoleInitializerInfo.CreateFor<TGameContext, TItemId>(SenseSourceRole, InitializeSenseSourceCollectionContinuous)
                                                            .WithRequiredRoles(PositionModule.ContinuousPositionedRole);

                // all sense sources make use of the sense-cache.
                yield return ModuleEntityRoleInitializerInfo.CreateFor<TGameContext, TItemId>(SenseSourceRole, InitializeSenseCache)
                                                            .WithRequiredRoles(PositionModule.GridPositionedRole);
            }

            if (role == ResistanceDataProviderRole)
            {
                yield return ModuleEntityRoleInitializerInfo.CreateFor<TGameContext, TItemId>(role, InitializeResistanceRole);
            }
        }

        public override void ProcessDeclaredSystems<TGameContext, TEntityId>(in ModuleInitializationParameter p,
                                                                             IModuleInitializationData<TGameContext, TEntityId> moduleContext,
                                                                             IModuleInitializer<TGameContext> initializer)
        {
            foreach (var bi in moduleContext.DeclaredBulkItems)
            {
                if (bi.itemDeclaration.TryQuery(out IItemComponentDesignTimeInformationTrait<MapLayerPreference> layerPref) &&
                    layerPref.TryQuery(out var layers) &&
                    bi.itemDeclaration.HasItemComponent<TGameContext, TEntityId, SensoryResistance<VisionSense>>())
                {
                    var ctx = initializer.DeclareEntityContext<TEntityId>();

                    var systemId = SenseSourceModules.CreateResistanceSourceSystemId<VisionSense>();
                    ctx.Register(systemId, 1100, SenseSourceModules.RegisterSenseResistanceSourceLayer<TGameContext, TEntityId, VisionSense>(layers));
                }
            }
        }

        protected void InitializeSenseSourceRole<TGameContext, TItemId>(in ModuleEntityInitializationParameter<TGameContext, TItemId> initParameter,
                                                                        IModuleInitializer<TGameContext> initializer,
                                                                        EntityRole role)
            where TItemId : IEntityKey
        {
            var ctx = initializer.DeclareEntityContext<TItemId>();
            ctx.Register(RegisterEntityId, 0, RegisterSenseSourceEntities);
            ctx.Register(PreparationSystemId, 50000, RegisterPrepareSenseSourceSystem);
            ctx.Register(ComputeSystemId, 58000, RegisterCalculateSenseSourceStateSystem);
            ctx.Register(FinalizeSystemId, 59000, RegisterCleanUpSystem);

            // The sense resistance system is only needed if there is at least one sense source of this type active. 
            ctx.Register(RegisterResistanceSystem, 500, RegisterResistanceSystemLifecycle);
            ctx.Register(ExecuteResistanceSystem, 51000, RegisterResistanceSystemExecution);
            ctx.Register(ExecuteResistanceSystem, 52000, RegisterProcessSenseDirectionalitySystem);

            // Sense caching is only required if there is at least one sense source active. 
            ctx.Register(SenseCacheLifecycleId, 500, RegisterSenseResistanceCacheLifeCycle);
        }

        protected void InitializeSenseSourceCollectionGrid<TGameContext, TItemId>(in ModuleEntityInitializationParameter<TGameContext, TItemId> initParameter,
                                                                                  IModuleInitializer<TGameContext> initializer,
                                                                                  EntityRole role)
            where TItemId : IEntityKey
        {
            var ctx = initializer.DeclareEntityContext<TItemId>();
            ctx.Register(CollectionGridSystemId, 55000, RegisterCollectSenseSourceGridSystem);
        }

        protected void InitializeSenseSourceCollectionContinuous<TGameContext, TItemId>(in ModuleEntityInitializationParameter<TGameContext, TItemId> initParameter,
                                                                                        IModuleInitializer<TGameContext> initializer,
                                                                                        EntityRole role)
            where TItemId : IEntityKey
        {
            var ctx = initializer.DeclareEntityContext<TItemId>();
            ctx.Register(CollectionGridSystemId, 55000, RegisterCollectSenseSourceContinuousSystem);
        }

        [SuppressMessage("ReSharper", "UnusedTypeParameter")]
        protected void InitializeSenseCache<TGameContext, TItemId>(in ModuleEntityInitializationParameter<TGameContext, TItemId> initParameter,
                                                                   IModuleInitializer<TGameContext> initializer,
                                                                   EntityRole role)
            where TItemId : IEntityKey
        {
            if (initParameter.ServiceResolver.TryResolve(out SenseCacheSetUpSystem<TGameContext> o))
            {
                o.RegisterSense<TSense>();
            }
        }

        protected void InitializeResistanceRole<TGameContext, TItemId>(in ModuleEntityInitializationParameter<TGameContext, TItemId> initParameter,
                                                                       IModuleInitializer<TGameContext> initializer,
                                                                       EntityRole role)
            where TItemId : IEntityKey
        {
            var ctx = initializer.DeclareEntityContext<TItemId>();
            ctx.Register(RegisterResistanceEntitiesId, 0, RegisterResistanceEntities);
        }

        protected void RegisterResistanceSystemLifecycle<TGameContext, TItemId>(in ModuleInitializationParameter initParameter,
                                                                                IGameLoopSystemRegistration<TGameContext> context,
                                                                                EntityRegistry<TItemId> registry)
            where TItemId : IEntityKey
        {
            var serviceResolver = initParameter.ServiceResolver;
            var system = GetOrCreateSensePropertiesSystem<TGameContext, TItemId>(serviceResolver);
            context.AddInitializationStepHandler(system.Start);
            context.AddDisposeStepHandler(system.Stop);
        }


        protected void RegisterResistanceSystemExecution<TGameContext, TItemId>(in ModuleInitializationParameter initParameter,
                                                                                IGameLoopSystemRegistration<TGameContext> context,
                                                                                EntityRegistry<TItemId> registry)
            where TItemId : IEntityKey
        {
            var serviceResolver = initParameter.ServiceResolver;
            var system = GetOrCreateSensePropertiesSystem<TGameContext, TItemId>(serviceResolver);

            context.AddInitializationStepHandler(system.ProcessSenseProperties);
            context.AddFixedStepHandlers(system.ProcessSenseProperties);
        }

        protected void RegisterPrepareSenseSourceSystem<TGameContext, TItemId>(in ModuleInitializationParameter initParameter,
                                                                               IGameLoopSystemRegistration<TGameContext> context,
                                                                               EntityRegistry<TItemId> registry)
            where TItemId : IEntityKey
        {
            var serviceResolver = initParameter.ServiceResolver;
            var ls = GetOrCreateSenseSourceSystem<TGameContext, TItemId>(serviceResolver);
            context.AddInitializationStepHandler(ls.EnsureSenseCacheAvailable, nameof(ls.EnsureSenseCacheAvailable));
            context.AddInitializationStepHandler(ls.BeginSenseCalculation, nameof(ls.BeginSenseCalculation));
            context.AddFixedStepHandlers(ls.BeginSenseCalculation, nameof(ls.BeginSenseCalculation));
        }

        protected void RegisterCollectSenseSourceGridSystem<TGameContext, TItemId>(in ModuleInitializationParameter initParameter,
                                                                                   IGameLoopSystemRegistration<TGameContext> context,
                                                                                   EntityRegistry<TItemId> registry)
            where TItemId : IEntityKey
        {
            var serviceResolver = initParameter.ServiceResolver;
            var ls = GetOrCreateSenseSourceSystem<TGameContext, TItemId>(serviceResolver);
            var system = registry.BuildSystem().WithContext<TGameContext>().CreateSystem<TSenseSourceDefinition, SenseSourceState<TSense>, EntityGridPosition>(ls.FindDirtySenseSources);
            context.AddInitializationStepHandler(system);
            context.AddFixedStepHandlers(system);
        }

        protected void RegisterCollectSenseSourceContinuousSystem<TGameContext, TItemId>(in ModuleInitializationParameter initParameter,
                                                                                         IGameLoopSystemRegistration<TGameContext> context,
                                                                                         EntityRegistry<TItemId> registry)
            where TItemId : IEntityKey
        {
            var serviceResolver = initParameter.ServiceResolver;
            var ls = GetOrCreateSenseSourceSystem<TGameContext, TItemId>(serviceResolver);
            var system = registry.BuildSystem()
                                 .WithContext<TGameContext>()
                                 .CreateSystem<TSenseSourceDefinition, SenseSourceState<TSense>, ContinuousMapPosition>(ls.FindDirtySenseSources);
            context.AddInitializationStepHandler(system);
            context.AddFixedStepHandlers(system);
        }

        protected void RegisterCalculateSenseSourceStateSystem<TGameContext, TItemId>(in ModuleInitializationParameter initParameter,
                                                                                      IGameLoopSystemRegistration<TGameContext> context,
                                                                                      EntityRegistry<TItemId> registry)
            where TItemId : IEntityKey
        {
            var serviceResolver = initParameter.ServiceResolver;
            var ls = GetOrCreateSenseSourceSystem<TGameContext, TItemId>(serviceResolver);
            var refreshLocalSenseState =
                registry.BuildSystem()
                        .WithContext<TGameContext>()
                        .CreateSystem<TSenseSourceDefinition, SenseSourceState<TSense>, SenseDirtyFlag<TSense>, ObservedSenseSource<TSense>>(ls.RefreshLocalSenseState);

            context.AddInitializationStepHandler(refreshLocalSenseState);
            context.AddFixedStepHandlers(refreshLocalSenseState);
        }

        protected void RegisterCleanUpSystem<TGameContext, TItemId>(in ModuleInitializationParameter initParameter,
                                                                    IGameLoopSystemRegistration<TGameContext> context,
                                                                    EntityRegistry<TItemId> registry)
            where TItemId : IEntityKey
        {
            var serviceResolver = initParameter.ServiceResolver;
            var ls = GetOrCreateSenseSourceSystem<TGameContext, TItemId>(serviceResolver);
            context.AddInitializationStepHandler(ls.EndSenseCalculation, nameof(ls.EndSenseCalculation));
            context.AddFixedStepHandlers(ls.EndSenseCalculation, nameof(ls.EndSenseCalculation));
            context.AddDisposeStepHandler(ls.ShutDown, nameof(ls.ShutDown));
        }

        protected void RegisterProcessSenseDirectionalitySystem<TGameContext, TItemId>(in ModuleInitializationParameter initParameter,
                                                                                       IGameLoopSystemRegistration<TGameContext> context,
                                                                                       EntityRegistry<TItemId> registry)
            where TItemId : IEntityKey
        {
            var serviceResolver = initParameter.ServiceResolver;
            var system = GetOrCreateDirectionalitySystem<TGameContext, TItemId>(serviceResolver);

            if (!serviceResolver.TryResolve(out SenseDirectionalitySystemRegisteredMarker<TSense> _))
            {
                serviceResolver.Store(new SenseDirectionalitySystemRegisteredMarker<TSense>());
                context.AddInitializationStepHandler(c => system.MarkGloballyDirty(), nameof(system.MarkGloballyDirty));
                context.AddInitializationStepHandler(system.ProcessSystem, nameof(system.ProcessSystem));
                context.AddInitializationStepHandler(system.MarkCleanSystem, nameof(system.MarkCleanSystem));
                context.AddFixedStepHandlers(system.ProcessSystem, nameof(system.ProcessSystem));
                context.AddFixedStepHandlers(system.MarkCleanSystem, nameof(system.MarkCleanSystem));
            }
        }

        protected void RegisterSenseResistanceCacheLifeCycle<TGameContext, TItemId>(in ModuleInitializationParameter initParameter,
                                                                                    IGameLoopSystemRegistration<TGameContext> context,
                                                                                    EntityRegistry<TItemId> registry)
            where TItemId : IEntityKey
        {
            var resolver = initParameter.ServiceResolver;
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

                context.AddInitializationStepHandler(system.Start, nameof(system.Start));
                context.AddDisposeStepHandler(system.Stop, nameof(system.Stop));
            }
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

        protected virtual SenseSourceSystem<TSense, TSenseSourceDefinition> GetOrCreateSenseSourceSystem<TGameContext, TItemId>(IServiceResolver serviceResolver)
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

        protected void RegisterSenseSourceEntities<TItemId>(in ModuleInitializationParameter initParameter,
                                                            EntityRegistry<TItemId> registry)
            where TItemId : IEntityKey
        {
            registry.RegisterNonConstructable<TSenseSourceDefinition>();
            registry.RegisterNonConstructable<SenseSourceState<TSense>>();
            registry.RegisterFlag<ObservedSenseSource<TSense>>();
            registry.RegisterFlag<SenseDirtyFlag<TSense>>();
        }

        protected void RegisterResistanceEntities<TItemId>(in ModuleInitializationParameter initParameter,
                                                           EntityRegistry<TItemId> registry)
            where TItemId : IEntityKey
        {
            registry.RegisterNonConstructable<SensoryResistance<TSense>>();
        }
    }

    [SuppressMessage("ReSharper", "UnusedTypeParameter")]
    readonly struct SenseDirectionalitySystemRegisteredMarker<TSense>
    {
    }
}