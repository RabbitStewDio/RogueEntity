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
using RogueEntity.Core.Movement.Pathfinding;
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
            ctx.Register(ExecuteResistanceSystem, 52000, RegisterProcessSenseDirectionalitySystem);
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
            context.AddInitializationStepHandler(c =>
            {
                if (serviceResolver.TryResolve(out IPathFinderSourceBackend pathFinderSource))
                {
                    var movementCostMap = serviceResolver.Resolve<IRelativeMovementCostSystem<TMovementMode>>();
                    var directionMap = serviceResolver.Resolve<IMovementResistanceDirectionView<TMovementMode>>();
                    pathFinderSource.RegisterMovementSource(GetMovementModeInstance(), movementCostMap.ResultView, directionMap.ResultView);
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

        protected void RegisterProcessSenseDirectionalitySystem<TGameContext, TItemId>(in ModuleInitializationParameter initParameter,
                                                                                       IGameLoopSystemRegistration<TGameContext> context,
                                                                                       EntityRegistry<TItemId> registry)
            where TItemId : IEntityKey
        {
            var serviceResolver = initParameter.ServiceResolver;
            var system = GetOrCreateDirectionalitySystem<TGameContext, TItemId>(serviceResolver);

            if (!serviceResolver.TryResolve(out MovementDirectionalitySystemRegisteredMarker _))
            {
                serviceResolver.Store(new MovementDirectionalitySystemRegisteredMarker());
                context.AddInitializationStepHandler(c => system.MarkGloballyDirty(), nameof(system.MarkGloballyDirty));
                context.AddInitializationStepHandler(system.ProcessSystem, nameof(system.ProcessSystem));
                context.AddInitializationStepHandler(system.MarkCleanSystem, nameof(system.MarkCleanSystem));
                context.AddFixedStepHandlers(system.ProcessSystem, nameof(system.ProcessSystem));
                context.AddFixedStepHandlers(system.MarkCleanSystem, nameof(system.MarkCleanSystem));
            }
        }


        protected virtual RelativeMovementCostSystem<TGameContext, TMovementMode> GetOrCreateSensePropertiesSystem<TGameContext, TEntityId>(IServiceResolver serviceResolver)
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

        protected virtual MovementResistanceDirectionalitySystem<TMovementMode> GetOrCreateDirectionalitySystem<TGameContext, TItemId>(IServiceResolver serviceResolver)
        {
            if (serviceResolver.TryResolve(out MovementResistanceDirectionalitySystem<TMovementMode> system))
            {
                return system;
            }

            if (!serviceResolver.TryResolve(out IRelativeMovementCostSystem<TMovementMode> data))
            {
                data = GetOrCreateSensePropertiesSystem<TGameContext, TItemId>(serviceResolver);
            }

            system = new MovementResistanceDirectionalitySystem<TMovementMode>(data.ResultView);
            serviceResolver.Store(system);
            serviceResolver.Store<IMovementResistanceDirectionView<TMovementMode>>(system);
            return system;
        }

        protected void RegisterEntities<TItemId>(in ModuleInitializationParameter initParameter,
                                                 EntityRegistry<TItemId> registry)
            where TItemId : IEntityKey
        {
            registry.RegisterNonConstructable<RelativeMovementCostModifier<TMovementMode>>();
        }

        readonly struct MovementDirectionalitySystemRegisteredMarker
        {
        }
    }
}