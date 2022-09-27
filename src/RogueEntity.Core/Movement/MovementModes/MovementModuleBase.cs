using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using EnTTSharp.Entities;
using RogueEntity.Api.GameLoops;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Modules;
using RogueEntity.Api.Modules.Attributes;
using RogueEntity.Api.Modules.Helpers;
using RogueEntity.Api.Services;
using RogueEntity.Api.Utils;
using RogueEntity.Core.GridProcessing.LayerAggregation;
using RogueEntity.Core.Movement.Cost;
using RogueEntity.Core.Movement.CostModifier;
using RogueEntity.Core.Movement.CostModifier.Directions;
using RogueEntity.Core.Movement.CostModifier.Map;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Grid;
using Serilog;
using System.Linq;

namespace RogueEntity.Core.Movement.MovementModes
{
    public abstract class MovementModuleBase<TMovementMode> : ModuleBase
        where TMovementMode : IMovementMode
    {
        static readonly ILogger logger = SLog.ForContext<MovementModuleBase<TMovementMode>>();
        
        public static EntityRole MovementCostModifierSourceRole = MovementModules.GetCostModifierSourceRole<TMovementMode>();

        public static EntityRole MovableActorRole = MovementModules.GetMovableActorRole<TMovementMode>();
        public static EntityRole MovableActorWithPointsRole = MovementModules.GetMovableActorWithPointsRole<TMovementMode>();
        public static EntityRole MovableActorWithVelocityRole = MovementModules.GetMovableActorWithVelocityRole<TMovementMode>();

        public static readonly EntitySystemId RegisterActorEntitiesWithPointsId = MovementModules.CreateActorEntityId<TMovementMode>();
        public static readonly EntitySystemId RegisterActorEntitiesWithVelocityId = MovementModules.CreateActorEntityId<TMovementMode>();

        public static readonly EntitySystemId RegisterResistanceEntitiesId = MovementModules.CreateEntityId<TMovementMode>();
        public static readonly EntitySystemId InitializeResistanceSystem = MovementModules.CreateSystemId<TMovementMode>("Initialize");
        public static readonly EntitySystemId RegisterResistanceSystem = MovementModules.CreateSystemId<TMovementMode>("LifeCycle");
        public static readonly EntitySystemId ExecuteResistanceSystem = MovementModules.CreateSystemId<TMovementMode>("ProcessChanges");
        public static readonly EntitySystemId ExecuteInboundDirectionSystem = MovementModules.CreateSystemId<TMovementMode>("ProcessInboundDirection");
        public static readonly EntitySystemId ExecuteOutboundDirectionSystem = MovementModules.CreateSystemId<TMovementMode>("ProcessOutboundDirection");

        public static readonly EntitySystemId RegisterMovementDataCollectorConfigurationSystem = MovementModules.CreateSystemId<TMovementMode>("MovementDataCollectorRegistration");
        public static readonly EntitySystemId RegisterMovementDataLayerSystem = MovementModules.CreateSystemId<TMovementMode>("RegisterMovementDataLayer");

        public static readonly EntitySystemId EnsureMovementDataCollectorAvailableSystem = new EntitySystemId("System.Core.Movement.EnsureMovementDataCollectorAvailable");

        protected MovementModuleBase()
        {
            DeclareDependencies(ModuleDependency.Of(PositionModule.ModuleId));

            RequireRole(MovementCostModifierSourceRole).WithImpliedRole(MovementModules.GeneralCostModifierSourceRole);
            RequireRole(MovableActorRole).WithImpliedRole(MovementModules.GeneralMovableActorRole);
            ForRole(MovableActorWithPointsRole).WithImpliedRole(MovableActorRole);
            ForRole(MovableActorWithVelocityRole).WithImpliedRole(MovableActorRole);

            // RequireRelation(ProvidesMovementCostDataRelation);
        }

        [InitializerCollector(InitializerCollectorType.Roles)]
        public IEnumerable<ModuleEntityRoleInitializerInfo<TItemId>> CollectRoleInitializers<TItemId>(IServiceResolver serviceResolver,
                                                                                                      IModuleEntityInformation entityInformation,
                                                                                                      EntityRole role)
            where TItemId : struct, IEntityKey
        {
            if (role == MovementCostModifierSourceRole)
            {
                yield return ModuleEntityRoleInitializerInfo.CreateFor<TItemId>(MovementCostModifierSourceRole,
                                                                                InitializeResistanceRole,
                                                                                $"{GetType().Name}#{nameof(InitializeResistanceRole)}")
                                                            .WithRequiredRoles(GridPositionModule.GridPositionedRole)
                                                            .WithRequiredRolesAnywhereInSystem(MovableActorRole);
            }

            if (role == MovableActorWithPointsRole)
            {
                yield return ModuleEntityRoleInitializerInfo.CreateFor<TItemId>(MovableActorWithPointsRole,
                                                                                InitializeActorWithPointsRole,
                                                                                $"{GetType().Name}#{nameof(InitializeActorWithPointsRole)}");
            }

            if (role == MovableActorWithVelocityRole)
            {
                yield return ModuleEntityRoleInitializerInfo.CreateFor<TItemId>(MovableActorWithVelocityRole,
                                                                                InitializeActorWithVelocityRole,
                                                                                $"{GetType().Name}#{nameof(InitializeActorWithVelocityRole)}");
            }
        }

        [FinalizerCollector(InitializerCollectorType.Roles)]
        [SuppressMessage("ReSharper", "UnusedTypeParameter", Justification = "TSource is part of a delegate definition.")]
        public IEnumerable<ModuleEntityRoleInitializerInfo<TSource>> CollectRelationInitializers<TSource>(IServiceResolver serviceResolver,
                                                                                                          IModuleEntityInformation entityInformation,
                                                                                                          EntityRole role)
            where TSource : struct, IEntityKey
        {
            if (role != MovementCostModifierSourceRole)
            {
                yield break;
            }

            yield return ModuleEntityRoleInitializerInfo.CreateFor<TSource>(MovementCostModifierSourceRole,
                                                                            FinalizeMovementDataCollector,
                                                                            $"{GetType().Name}#{nameof(FinalizeMovementDataCollector)}"
            );
        }

        protected void InitializeActorWithPointsRole<TItemId>(in ModuleEntityInitializationParameter<TItemId> initParameter,
                                                              IModuleInitializer initializer,
                                                              EntityRole role)
            where TItemId : struct, IEntityKey
        {
            var ctx = initializer.DeclareEntityContext<TItemId>();
            ctx.Register(RegisterActorEntitiesWithPointsId, 0, RegisterActorEntitiesWithPoints);
            ctx.Register(EnsureMovementDataCollectorAvailableSystem, 1, EnsureMovementDataCollectorAvailable);
        }

        protected void InitializeActorWithVelocityRole<TItemId>(in ModuleEntityInitializationParameter<TItemId> initParameter,
                                                                IModuleInitializer initializer,
                                                                EntityRole role)
            where TItemId : struct, IEntityKey
        {
            var ctx = initializer.DeclareEntityContext<TItemId>();
            ctx.Register(RegisterActorEntitiesWithVelocityId, 0, RegisterActorEntitiesWithVelocity);
            ctx.Register(EnsureMovementDataCollectorAvailableSystem, 1, EnsureMovementDataCollectorAvailable);
        }

        void EnsureMovementDataCollectorAvailable<TItemId>(in ModuleEntityInitializationParameter<TItemId> initParameter, EntityRegistry<TItemId> registry)
            where TItemId : struct, IEntityKey
        {
            var sr = initParameter.ServiceResolver;
            if (!sr.TryResolve<IMovementDataCollector>(out _) &&
                !sr.TryResolve<IMovementDataProvider>(out _))
            {
                var x = new MovementDataCollector();
                sr.Store<IMovementDataCollector>(x);
                sr.Store<IMovementDataProvider>(x);
            }
        }

        protected void InitializeResistanceRole<TItemId>(in ModuleEntityInitializationParameter<TItemId> initParameter,
                                                         IModuleInitializer initializer,
                                                         EntityRole role)
            where TItemId : struct, IEntityKey
        {
            var ctx = initializer.DeclareEntityContext<TItemId>();
            ctx.Register(RegisterResistanceEntitiesId, 0, RegisterEntities);
            ctx.Register(ExecuteResistanceSystem, 51000, RegisterResistanceSystemExecution);
            ctx.Register(ExecuteInboundDirectionSystem, 52000, RegisterProcessInboundDirectionalitySystem);
            ctx.Register(ExecuteOutboundDirectionSystem, 52000, RegisterProcessOutboundDirectionalitySystem);
            ctx.Register(RegisterResistanceSystem, 500, RegisterResistanceSystemLifecycle);
            ctx.Register(InitializeResistanceSystem, 500, RegisterMovementDataLayer);
        }

        protected void FinalizeMovementDataCollector<TItemId>(in ModuleEntityInitializationParameter<TItemId> initParameter,
                                                                 IModuleInitializer initializer,
                                                                 EntityRole relation)
            where TItemId : struct, IEntityKey
        {
            initializer.Register(RegisterMovementDataCollectorConfigurationSystem, 500, RegisterMovementDataCollector);
        }

        void RegisterMovementDataLayer<TItemId>(in ModuleEntityInitializationParameter<TItemId> initParameter, 
                                                IGameLoopSystemRegistration context, 
                                                EntityRegistry<TItemId> registry)
            where TItemId : struct, IEntityKey
        {
            var layers = PositionModuleServices.CollectMapLayers<TItemId, RelativeMovementCostModifier<TMovementMode>>(initParameter);
            var mapLayers = layers.ToList();
            logger.Debug("{MovementMode} will use map layers {Layers} as relative movement resistance source for {EntityId}", typeof(TMovementMode), mapLayers, typeof(TItemId));

            var sr = initParameter.ServiceResolver;
            var sys = GetOrCreateMovementPropertiesSystem<TItemId>(sr);
            var itemContext = sr.Resolve<IItemResolver< TItemId>>();
            var mapContext = sr.Resolve<IGridMapContext<TItemId>>();
            
            context.AddInitializationStepHandler(() =>
            {
                foreach (var layer in layers)
                {
                    sys.AddLayer(mapContext, itemContext, layer);
                }

            });
        }


        void RegisterMovementDataCollector(in ModuleInitializationParameter initParameter,
                                           IGameLoopSystemRegistration context)
        {
            var serviceResolver = initParameter.ServiceResolver;
            GetOrCreateModeRegistry(serviceResolver).Register(GetMovementModeInstance());

            context.AddInitializationStepHandler(() =>
            {
                if (serviceResolver.TryResolve<IMovementDataCollector>(out var pathFinderSource))
                {
                    var movementCostMap = serviceResolver.Resolve<IRelativeMovementCostSystem<TMovementMode>>();
                    var inboundDirectionMap = serviceResolver.Resolve<IInboundMovementDirectionView<TMovementMode>>();
                    var outboundDirectionMap = serviceResolver.Resolve<IOutboundMovementDirectionView<TMovementMode>>();
                    pathFinderSource.RegisterMovementSource(GetMovementModeInstance(), movementCostMap.ResultView, inboundDirectionMap.ResultView, outboundDirectionMap.ResultView);
                }
            }, "RegisterMovementSourceData");
        }

        protected abstract TMovementMode GetMovementModeInstance();

        protected void RegisterResistanceSystemLifecycle<TItemId>(in ModuleEntityInitializationParameter<TItemId> initParameter,
                                                                  IGameLoopSystemRegistration context,
                                                                  EntityRegistry<TItemId> registry)
            where TItemId : struct, IEntityKey
        {
            var serviceResolver = initParameter.ServiceResolver;
            var system = GetOrCreateMovementPropertiesSystem<TItemId>(serviceResolver);
            context.AddInitializationStepHandler(system.Start);
            context.AddDisposeStepHandler(system.Stop);
        }

        protected void RegisterResistanceSystemExecution<TItemId>(in ModuleEntityInitializationParameter<TItemId> initParameter,
                                                                  IGameLoopSystemRegistration context,
                                                                  EntityRegistry<TItemId> registry)
            where TItemId : struct, IEntityKey
        {
            var serviceResolver = initParameter.ServiceResolver;
            var system = GetOrCreateMovementPropertiesSystem<TItemId>(serviceResolver);

            context.AddInitializationStepHandler(system.ProcessSenseProperties);
            context.AddFixedStepHandlers(system.ProcessSenseProperties);
        }

        protected void RegisterProcessInboundDirectionalitySystem<TItemId>(in ModuleEntityInitializationParameter<TItemId> initParameter,
                                                                           IGameLoopSystemRegistration context,
                                                                           EntityRegistry<TItemId> registry)
            where TItemId : struct, IEntityKey
        {
            var serviceResolver = initParameter.ServiceResolver;
            var system = GetOrCreateInboundDirectionalitySystem<TItemId>(serviceResolver);

            if (!serviceResolver.TryResolve(out InboundDirectionalitySystemRegisteredMarker _))
            {
                serviceResolver.Store(new InboundDirectionalitySystemRegisteredMarker());
                context.AddInitializationStepHandler(system.MarkGloballyDirty);
                context.AddInitializationStepHandler(system.ProcessSystem);
                context.AddInitializationStepHandler(system.MarkCleanSystem);
                context.AddFixedStepHandlers(system.ProcessSystem);
                context.AddFixedStepHandlers(system.MarkCleanSystem);
            }
        }

        protected void RegisterProcessOutboundDirectionalitySystem<TItemId>(in ModuleEntityInitializationParameter<TItemId> initParameter,
                                                                            IGameLoopSystemRegistration context,
                                                                            EntityRegistry<TItemId> registry)
            where TItemId : struct, IEntityKey
        {
            var serviceResolver = initParameter.ServiceResolver;
            var system = GetOrCreateOutboundDirectionalitySystem<TItemId>(serviceResolver);

            if (!serviceResolver.TryResolve(out OutboundDirectionalitySystemRegisteredMarker _))
            {
                
                serviceResolver.Store(new OutboundDirectionalitySystemRegisteredMarker());
                context.AddInitializationStepHandler(system.MarkGloballyDirty);
                context.AddInitializationStepHandler(system.ProcessSystem);
                context.AddInitializationStepHandler(system.MarkCleanSystem);
                context.AddFixedStepHandlers(system.ProcessSystem);
                context.AddFixedStepHandlers(system.MarkCleanSystem);
            }
        }


        protected virtual RelativeMovementCostSystem<TMovementMode> GetOrCreateMovementPropertiesSystem<TEntityId>(IServiceResolver serviceResolver)
        {
            if (serviceResolver.TryResolve<RelativeMovementCostSystem<TMovementMode>>(out var system))
            {
                return system;
            }

            var gridConfig = serviceResolver.Resolve<IGridMapConfiguration<TEntityId>>();
            system = new RelativeMovementCostSystem<TMovementMode>(gridConfig.OffsetX, gridConfig.OffsetY, gridConfig.TileSizeX, gridConfig.TileSizeY);

            serviceResolver.Store(system);
            serviceResolver.Store<IAggregationLayerSystemBackend<RelativeMovementCostModifier<TMovementMode>>>(system);
            serviceResolver.Store<IRelativeMovementCostSystem<TMovementMode>>(system);
            return system;
        }

        protected virtual InboundMovementDirectionalitySystem<TMovementMode> GetOrCreateInboundDirectionalitySystem<TItemId>(IServiceResolver serviceResolver)
        {
            if (serviceResolver.TryResolve<InboundMovementDirectionalitySystem<TMovementMode>>(out var system))
            {
                return system;
            }

            if (!serviceResolver.TryResolve<IRelativeMovementCostSystem<TMovementMode>>(out var data))
            {
                data = GetOrCreateMovementPropertiesSystem<TItemId>(serviceResolver);
            }

            system = new InboundMovementDirectionalitySystem<TMovementMode>(data.ResultView, data);
            serviceResolver.Store(system);
            serviceResolver.Store<IInboundMovementDirectionView<TMovementMode>>(system);
            return system;
        }

        protected virtual OutboundMovementDirectionalitySystem<TMovementMode> GetOrCreateOutboundDirectionalitySystem<TItemId>(IServiceResolver serviceResolver)
        {
            if (serviceResolver.TryResolve<OutboundMovementDirectionalitySystem<TMovementMode>>(out var system))
            {
                return system;
            }

            if (!serviceResolver.TryResolve<IRelativeMovementCostSystem<TMovementMode>>(out var data))
            {
                data = GetOrCreateMovementPropertiesSystem<TItemId>(serviceResolver);
            }

            system = new OutboundMovementDirectionalitySystem<TMovementMode>(data.ResultView, data);
            serviceResolver.Store(system);
            serviceResolver.Store<IOutboundMovementDirectionView<TMovementMode>>(system);
            return system;
        }

        protected MovementModeRegistry GetOrCreateModeRegistry(IServiceResolver r)
        {
            if (r.TryResolve<MovementModeRegistry>(out var reg))
            {
                return reg;
            }

            reg = new MovementModeRegistry();
            r.Store(reg);
            r.Store<IMovementModeRegistry>(reg);
            return reg;
        }

        protected void RegisterEntities<TItemId>(in ModuleEntityInitializationParameter<TItemId> initParameter,
                                                 EntityRegistry<TItemId> registry)
            where TItemId : struct, IEntityKey
        {
            registry.RegisterNonConstructable<RelativeMovementCostModifier<TMovementMode>>();
        }

        protected void RegisterActorEntitiesWithPoints<TItemId>(in ModuleEntityInitializationParameter<TItemId> initParameter,
                                                                EntityRegistry<TItemId> registry)
            where TItemId : struct, IEntityKey
        {
            registry.RegisterNonConstructable<MovementPointCost<TMovementMode>>();
        }

        protected void RegisterActorEntitiesWithVelocity<TItemId>(in ModuleEntityInitializationParameter<TItemId> initParameter,
                                                                  EntityRegistry<TItemId> registry)
            where TItemId : struct, IEntityKey
        {
            registry.RegisterNonConstructable<MovementVelocity<TMovementMode>>();
        }

        readonly struct InboundDirectionalitySystemRegisteredMarker
        { }

        readonly struct OutboundDirectionalitySystemRegisteredMarker
        { }
    }
}
