using System.Collections.Generic;
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
    public static class MovementModules
    {
        public static EntityRole GetMovableActorRole<TMovementMode>() => new EntityRole($"Role.Core.Movement.{typeof(TMovementMode).Name}.MovableActor");
        public static EntityRole GetResistanceRole<TMovementMode>() => new EntityRole($"Role.Core.Movement.{typeof(TMovementMode).Name}.CostModifierSource");

        public static EntityRelation GetCostModifierRelation<TMovementMode>() => new EntityRelation($"Relation.Core.Movement.Resistance.{typeof(TMovementMode).Name}.ProvidesCostData",
                                                                                             GetResistanceRole<TMovementMode>(), GetMovableActorRole<TMovementMode>());

        public static EntitySystemId CreateSystemId<TMovementMode>(string job) => new EntitySystemId($"Core.Systems.Movement.CostModifier.{typeof(TMovementMode).Name}.{job}");
        public static EntitySystemId CreateEntityId<TMovementMode>() => new EntitySystemId($"Entities.Systems.Movement.CostModifier.{typeof(TMovementMode).Name}");
    }

    public abstract class MovementModuleBase<TMovementMode> : ModuleBase
    {
        public static EntityRole MovementCostModifierSourceRole = MovementModules.GetMovableActorRole<TMovementMode>();
        public static EntityRole MovableActorRole = MovementModules.GetResistanceRole<TMovementMode>();

        public static EntityRelation MovementRelation = MovementModules.GetCostModifierRelation<TMovementMode>();

        public static readonly EntitySystemId RegisterResistanceEntitiesId = MovementModules.CreateEntityId<TMovementMode>();
        public static readonly EntitySystemId RegisterResistanceSystem = MovementModules.CreateSystemId<TMovementMode>("LifeCycle");
        public static readonly EntitySystemId ExecuteResistanceSystem = MovementModules.CreateSystemId<TMovementMode>("ProcessChanges");

        protected MovementModuleBase()
        {
            DeclareDependencies(ModuleDependency.Of(PositionModule.ModuleId));

            RequireRole(MovementCostModifierSourceRole);
        }

        [InitializerCollector(InitializerCollectorType.Roles)]
        public IEnumerable<ModuleEntityRoleInitializerInfo<TGameContext, TItemId>> CollectRoleInitializers<TGameContext, TItemId>(IServiceResolver serviceResolver,
                                                                                                                                  IModuleEntityInformation entityInformation,
                                                                                                                                  EntityRole role)
            where TItemId : IEntityKey
        {
            if (role == MovementCostModifierSourceRole)
            {
                yield return ModuleEntityRoleInitializerInfo.CreateFor<TGameContext, TItemId>(MovementCostModifierSourceRole,
                                                                                              InitializeResistanceRole,
                                                                                              GetType().Name + "#" + nameof(InitializeResistanceRole))
                                                            .WithRequiredRolesAnywhereInSystem(); //todo 
            }
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
            serviceResolver.Store<IRelativeMovementCostSystem<TGameContext, TMovementMode>>(system);
            return system;
        }

        protected virtual MovementResistanceDirectionalitySystem<TMovementMode> GetOrCreateDirectionalitySystem<TGameContext, TItemId>(IServiceResolver serviceResolver)
        {
            if (serviceResolver.TryResolve(out MovementResistanceDirectionalitySystem<TMovementMode> system))
            {
                return system;
            }

            if (!serviceResolver.TryResolve(out IRelativeMovementCostSystem<TGameContext, TMovementMode> data))
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