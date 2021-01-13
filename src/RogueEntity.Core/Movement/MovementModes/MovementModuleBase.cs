using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using EnTTSharp.Entities;
using RogueEntity.Api.GameLoops;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Modules;
using RogueEntity.Api.Modules.Attributes;
using RogueEntity.Api.Modules.Helpers;
using RogueEntity.Api.Services;
using RogueEntity.Core.GridProcessing.LayerAggregation;
using RogueEntity.Core.Movement.CostModifier;
using RogueEntity.Core.Movement.CostModifier.Directions;
using RogueEntity.Core.Movement.CostModifier.Map;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Grid;

namespace RogueEntity.Core.Movement.MovementModes
{
    public abstract class MovementModuleBase<TMovementMode> : ModuleBase
        where TMovementMode : IMovementMode
    {
        public static EntityRole MovementCostModifierSourceRole = MovementModules.GetMovableActorRole<TMovementMode>();
        public static EntityRole MovableActorRole = MovementModules.GetCostModifierSourceRole<TMovementMode>();

        public static EntityRelation ProvidesMovementCostDataRelation = MovementModules.GetCostModifierRelation<TMovementMode>();

        public static readonly EntitySystemId RegisterResistanceEntitiesId = MovementModules.CreateEntityId<TMovementMode>();
        public static readonly EntitySystemId RegisterResistanceSystem = MovementModules.CreateSystemId<TMovementMode>("LifeCycle");
        public static readonly EntitySystemId ExecuteResistanceSystem = MovementModules.CreateSystemId<TMovementMode>("ProcessChanges");
        public static readonly EntitySystemId ExecuteInboundDirectionSystem = MovementModules.CreateSystemId<TMovementMode>("ProcessInboundDirection");
        public static readonly EntitySystemId ExecuteOutboundDirectionSystem = MovementModules.CreateSystemId<TMovementMode>("ProcessOutboundDirection");

        public static readonly EntitySystemId RegisterPathfinderSourceConfigurationSystem = MovementModules.CreateSystemId<TMovementMode>("PathFindingSource");
        public static readonly EntitySystemId RegisterGoalSourceConfigurationSystem = MovementModules.CreateSystemId<TMovementMode>("GoalFindingSource");

        protected MovementModuleBase()
        {
            DeclareDependencies(ModuleDependency.Of(PositionModule.ModuleId));

            RequireRole(MovableActorRole).WithImpliedRole(MovementModules.GeneralMovableActorRole);
            RequireRole(MovementCostModifierSourceRole).WithImpliedRole(MovementModules.GeneralCostModifierSourceRole);

            RequireRelation(ProvidesMovementCostDataRelation);
        }

        [InitializerCollector(InitializerCollectorType.Roles)]
        public IEnumerable<ModuleEntityRoleInitializerInfo<TGameContext, TItemId>> CollectRoleInitializers<TGameContext, TItemId>(IServiceResolver serviceResolver,
                                                                                                                                  IModuleEntityInformation entityInformation,
                                                                                                                                  EntityRole role)
            where TItemId : IEntityKey
        {
            if (role != MovementCostModifierSourceRole)
            {
                yield break;
            }

            yield return ModuleEntityRoleInitializerInfo.CreateFor<TGameContext, TItemId>(MovementCostModifierSourceRole,
                                                                                          InitializeResistanceRole,
                                                                                          $"{GetType().Name}#{nameof(InitializeResistanceRole)}")
                                                        .WithRequiredRoles(PositionModule.GridPositionedRole)
                                                        .WithRequiredRolesAnywhereInSystem(MovableActorRole);
        }
        
        [FinalizerCollector(InitializerCollectorType.Relations)]
        [SuppressMessage("ReSharper", "UnusedTypeParameter", Justification = "TObject is part of a delegate definition.")]
        public IEnumerable<ModuleEntityRelationInitializerInfo<TGameContext, TSource>> CollectRoleInitializers<TGameContext, TSource, TObject>(IServiceResolver serviceResolver,
                                                                                                                                               IModuleEntityInformation entityInformation,
                                                                                                                                               EntityRelation relation)
            where TSource : IEntityKey
            where TObject : IEntityKey
        {
            if (relation != ProvidesMovementCostDataRelation)
            {
                yield break;
            }

            yield return ModuleEntityRelationInitializerInfo.CreateFor<TGameContext, TSource>(ProvidesMovementCostDataRelation,
                                                                                              FinalizePathfinderFromSourcesRelation,
                                                                                              $"{GetType().Name}#{nameof(FinalizePathfinderFromSourcesRelation)}"
            );
        }

        protected void InitializeResistanceRole<TGameContext, TItemId>(in ModuleEntityInitializationParameter<TGameContext, TItemId> initParameter,
                                                                       IModuleInitializer<TGameContext> initializer,
                                                                       EntityRole role)
            where TItemId : IEntityKey
        {
            var ctx = initializer.DeclareEntityContext<TItemId>();
            ctx.Register(RegisterResistanceEntitiesId, 0, RegisterEntities);
            ctx.Register(ExecuteResistanceSystem, 51000, RegisterResistanceSystemExecution);
            ctx.Register(ExecuteInboundDirectionSystem, 52000, RegisterProcessInboundDirectionalitySystem);
            ctx.Register(ExecuteOutboundDirectionSystem, 52000, RegisterProcessOutboundDirectionalitySystem);
            ctx.Register(RegisterResistanceSystem, 500, RegisterResistanceSystemLifecycle);
        }

        protected void FinalizePathfinderFromSourcesRelation<TGameContext, TItemId>(in ModuleEntityInitializationParameter<TGameContext, TItemId> initParameter,
                                                                                      IModuleInitializer<TGameContext> initializer,
                                                                                      EntityRelation relation)
            where TItemId : IEntityKey
        {
            initializer.Register(RegisterPathfinderSourceConfigurationSystem, 500, RegisterPathfinderSources);
        }

        void RegisterPathfinderSources<TGameContext>(in ModuleInitializationParameter initparameter,
                                                     IGameLoopSystemRegistration<TGameContext> context)
        {
            var serviceResolver = initparameter.ServiceResolver;
            GetOrCreateModeRegistry(serviceResolver).Register(GetMovementModeInstance());
            
            context.AddInitializationStepHandler(c =>
            {
                if (serviceResolver.TryResolve(out IMovementDataCollector pathFinderSource))
                {
                    var movementCostMap = serviceResolver.Resolve<IRelativeMovementCostSystem<TMovementMode>>();
                    var inboundDirectionMap = serviceResolver.Resolve<IInboundMovementDirectionView<TMovementMode>>();
                    var outboundDirectionMap = serviceResolver.Resolve<IOutboundMovementDirectionView<TMovementMode>>();
                    pathFinderSource.RegisterMovementSource(GetMovementModeInstance(), movementCostMap.ResultView, inboundDirectionMap.ResultView, outboundDirectionMap.ResultView);
                }
            });
        }

        protected abstract TMovementMode GetMovementModeInstance();

        protected void RegisterResistanceSystemLifecycle<TGameContext, TItemId>(in ModuleInitializationParameter initParameter,
                                                                                IGameLoopSystemRegistration<TGameContext> context,
                                                                                EntityRegistry<TItemId> registry)
            where TItemId : IEntityKey
        {
            var serviceResolver = initParameter.ServiceResolver;
            var system = GetOrCreateMovementPropertiesSystem<TGameContext, TItemId>(serviceResolver);
            context.AddInitializationStepHandler(system.Start);
            context.AddDisposeStepHandler(system.Stop);
        }

        protected void RegisterResistanceSystemExecution<TGameContext, TItemId>(in ModuleInitializationParameter initParameter,
                                                                                IGameLoopSystemRegistration<TGameContext> context,
                                                                                EntityRegistry<TItemId> registry)
            where TItemId : IEntityKey
        {
            var serviceResolver = initParameter.ServiceResolver;
            var system = GetOrCreateMovementPropertiesSystem<TGameContext, TItemId>(serviceResolver);

            context.AddInitializationStepHandler(system.ProcessSenseProperties);
            context.AddFixedStepHandlers(system.ProcessSenseProperties);
        }

        protected void RegisterProcessInboundDirectionalitySystem<TGameContext, TItemId>(in ModuleInitializationParameter initParameter,
                                                                                       IGameLoopSystemRegistration<TGameContext> context,
                                                                                       EntityRegistry<TItemId> registry)
            where TItemId : IEntityKey
        {
            var serviceResolver = initParameter.ServiceResolver;
            var system = GetOrCreateInboundDirectionalitySystem<TGameContext, TItemId>(serviceResolver);

            if (!serviceResolver.TryResolve(out InboundDirectionalitySystemRegisteredMarker _))
            {
                serviceResolver.Store(new InboundDirectionalitySystemRegisteredMarker());
                context.AddInitializationStepHandler(c => system.MarkGloballyDirty(), nameof(system.MarkGloballyDirty));
                context.AddInitializationStepHandler(system.ProcessSystem, nameof(system.ProcessSystem));
                context.AddInitializationStepHandler(system.MarkCleanSystem, nameof(system.MarkCleanSystem));
                context.AddFixedStepHandlers(system.ProcessSystem, nameof(system.ProcessSystem));
                context.AddFixedStepHandlers(system.MarkCleanSystem, nameof(system.MarkCleanSystem));
            }
        }

        protected void RegisterProcessOutboundDirectionalitySystem<TGameContext, TItemId>(in ModuleInitializationParameter initParameter,
                                                                                       IGameLoopSystemRegistration<TGameContext> context,
                                                                                       EntityRegistry<TItemId> registry)
            where TItemId : IEntityKey
        {
            var serviceResolver = initParameter.ServiceResolver;
            var system = GetOrCreateOutboundDirectionalitySystem<TGameContext, TItemId>(serviceResolver);

            if (!serviceResolver.TryResolve(out OutboundDirectionalitySystemRegisteredMarker _))
            {
                serviceResolver.Store(new OutboundDirectionalitySystemRegisteredMarker());
                context.AddInitializationStepHandler(c => system.MarkGloballyDirty(), nameof(system.MarkGloballyDirty));
                context.AddInitializationStepHandler(system.ProcessSystem, nameof(system.ProcessSystem));
                context.AddInitializationStepHandler(system.MarkCleanSystem, nameof(system.MarkCleanSystem));
                context.AddFixedStepHandlers(system.ProcessSystem, nameof(system.ProcessSystem));
                context.AddFixedStepHandlers(system.MarkCleanSystem, nameof(system.MarkCleanSystem));
            }
        }


        protected virtual RelativeMovementCostSystem<TGameContext, TMovementMode> GetOrCreateMovementPropertiesSystem<TGameContext, TEntityId>(IServiceResolver serviceResolver)
        {
            if (serviceResolver.TryResolve(out RelativeMovementCostSystem<TGameContext, TMovementMode> system))
            {
                return system;
            }

            var gridConfig = serviceResolver.Resolve<IGridMapConfiguration<TEntityId>>();
            system = new RelativeMovementCostSystem<TGameContext, TMovementMode>(gridConfig.OffsetX, gridConfig.OffsetY, gridConfig.TileSizeX, gridConfig.TileSizeY);

            serviceResolver.Store(system);
            serviceResolver.Store<IAggregationLayerSystemBackend<TGameContext, RelativeMovementCostModifier<TMovementMode>>>(system);
            serviceResolver.Store<IRelativeMovementCostSystem<TMovementMode>>(system);
            return system;
        }

        protected virtual InboundMovementDirectionalitySystem<TMovementMode> GetOrCreateInboundDirectionalitySystem<TGameContext, TItemId>(IServiceResolver serviceResolver)
        {
            if (serviceResolver.TryResolve(out InboundMovementDirectionalitySystem<TMovementMode> system))
            {
                return system;
            }

            if (!serviceResolver.TryResolve(out IRelativeMovementCostSystem<TMovementMode> data))
            {
                data = GetOrCreateMovementPropertiesSystem<TGameContext, TItemId>(serviceResolver);
            }

            system = new InboundMovementDirectionalitySystem<TMovementMode>(data.ResultView);
            serviceResolver.Store(system);
            serviceResolver.Store<IInboundMovementDirectionView<TMovementMode>>(system);
            return system;
        }

        protected virtual OutboundMovementDirectionalitySystem<TMovementMode> GetOrCreateOutboundDirectionalitySystem<TGameContext, TItemId>(IServiceResolver serviceResolver)
        {
            if (serviceResolver.TryResolve(out OutboundMovementDirectionalitySystem<TMovementMode> system))
            {
                return system;
            }

            if (!serviceResolver.TryResolve(out IRelativeMovementCostSystem<TMovementMode> data))
            {
                data = GetOrCreateMovementPropertiesSystem<TGameContext, TItemId>(serviceResolver);
            }

            system = new OutboundMovementDirectionalitySystem<TMovementMode>(data.ResultView);
            serviceResolver.Store(system);
            serviceResolver.Store<IOutboundMovementDirectionView<TMovementMode>>(system);
            return system;
        }

        protected MovementModeRegistry GetOrCreateModeRegistry(IServiceResolver r)
        {
            if (r.TryResolve(out MovementModeRegistry reg))
            {
                return reg;
            }

            reg = new MovementModeRegistry();
            r.Store(reg);
            r.Store<IMovementModeRegistry>(reg);
            return reg;
        }
        
        protected void RegisterEntities<TItemId>(in ModuleInitializationParameter initParameter,
                                                 EntityRegistry<TItemId> registry)
            where TItemId : IEntityKey
        {
            registry.RegisterNonConstructable<RelativeMovementCostModifier<TMovementMode>>();
        }

        readonly struct InboundDirectionalitySystemRegisteredMarker
        {
        }
        
        readonly struct OutboundDirectionalitySystemRegisteredMarker
        {
        }
    }
}