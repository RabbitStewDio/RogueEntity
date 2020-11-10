using System.Collections.Generic;
using EnTTSharp.Entities;
using RogueEntity.Core.GridProcessing.LayerAggregation;
using RogueEntity.Core.Infrastructure.Commands;
using RogueEntity.Core.Infrastructure.GameLoops;
using RogueEntity.Core.Infrastructure.ItemTraits;
using RogueEntity.Core.Infrastructure.Modules;
using RogueEntity.Core.Infrastructure.Modules.Attributes;
using RogueEntity.Core.Infrastructure.Modules.Services;
using RogueEntity.Core.Movement.CostModifier.Directions;
using RogueEntity.Core.Movement.CostModifier.Map;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Utils.DataViews;

namespace RogueEntity.Core.Movement.CostModifier
{
    public static class MovementResistanceModules
    {
        public static EntityRole GetResistanceRole<TSense>() => new EntityRole($"Role.Core.Movement.CostModifier.{typeof(TSense).Name}.Source");
        public static EntitySystemId CreateSystemId<TSense>(string job) => new EntitySystemId($"Core.Systems.Movement.CostModifier.{typeof(TSense).Name}.{job}");
        public static EntitySystemId CreateEntityId<TSense>() => new EntitySystemId($"Entities.Systems.Movement.CostModifier.{typeof(TSense).Name}");
    }

    public abstract class MovementCostModifierModuleBase<TMovementMode> : ModuleBase
    {
        public static EntityRole MovementCostModifierSourceRole = MovementResistanceModules.GetResistanceRole<TMovementMode>();
        public static readonly EntitySystemId RegisterResistanceEntitiesId = MovementResistanceModules.CreateEntityId<TMovementMode>();
        public static readonly EntitySystemId RegisterResistanceSystem = MovementResistanceModules.CreateSystemId<TMovementMode>("LifeCycle");
        public static readonly EntitySystemId ExecuteResistanceSystem = MovementResistanceModules.CreateSystemId<TMovementMode>("ProcessChanges");

        protected MovementCostModifierModuleBase()
        {
            DeclareDependencies(ModuleDependency.Of(PositionModule.ModuleId));

            RequireRole(MovementCostModifierSourceRole);
        }

        [InitializerCollector(InitializerCollectorType.Roles)]
        public IEnumerable<ModuleEntityRoleInitializerInfo<TGameContext>> CollectRoleInitializers<TGameContext, TItemId>(IServiceResolver serviceResolver,
                                                                                                                         IModuleEntityInformation entityInformation,
                                                                                                                         EntityRole role)
            where TItemId : IEntityKey
        {
            if (role == MovementCostModifierSourceRole)
            {
                yield return new ModuleEntityRoleInitializerInfo<TGameContext>(role, InitializeResistanceRole<TGameContext, TItemId>); 
            }
        }

        protected void InitializeResistanceRole<TGameContext, TItemId>(IServiceResolver serviceResolver,
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

        protected void RegisterProcessSenseDirectionalitySystem<TGameContext, TItemId>(IServiceResolver serviceResolver,
                                                                                       IGameLoopSystemRegistration<TGameContext> context,
                                                                                       EntityRegistry<TItemId> registry,
                                                                                       ICommandHandlerRegistration<TGameContext, TItemId> handler)
            where TItemId : IEntityKey
        {
            var system = GetOrCreateDirectionalitySystem<TGameContext, TItemId>(serviceResolver);

            if (!serviceResolver.TryResolve(out MovementDirectionalitySystemRegisteredMarker _))
            {
                serviceResolver.Store(new MovementDirectionalitySystemRegisteredMarker());
                context.AddInitializationStepHandler(c => system.MarkGloballyDirty());
                context.AddInitializationStepHandler(system.ProcessSystem);
                context.AddInitializationStepHandler(system.MarkCleanSystem);
                context.AddFixedStepHandlers(system.ProcessSystem);
                context.AddFixedStepHandlers(system.MarkCleanSystem);
            }
        }

        protected virtual MovementPropertiesSystem<TGameContext, TMovementMode> GetOrCreateSensePropertiesSystem<TGameContext, TEntityId>(IServiceResolver serviceResolver)
        {
            if (serviceResolver.TryResolve(out MovementPropertiesSystem<TGameContext, TMovementMode> system))
            {
                return system;
            }

            var gridConfig = serviceResolver.Resolve<IGridMapConfiguration<TEntityId>>();
            system = new MovementPropertiesSystem<TGameContext, TMovementMode>(gridConfig.OffsetX, gridConfig.OffsetY, gridConfig.TileSizeX, gridConfig.TileSizeY);

            serviceResolver.Store(system);
            serviceResolver.Store<IAggregationLayerSystem<TGameContext, MovementCostModifier<TMovementMode>>>(system);
            serviceResolver.Store<IReadOnlyDynamicDataView3D<MovementCostModifier<TMovementMode>>>(system);
            return system;
        }

        protected virtual MovementResistanceDirectionalitySystem<TMovementMode> GetOrCreateDirectionalitySystem<TGameContext, TItemId>(IServiceResolver serviceResolver)
        {
            if (serviceResolver.TryResolve(out MovementResistanceDirectionalitySystem<TMovementMode> system))
            {
                return system;
            }

            if (!serviceResolver.TryResolve(out IReadOnlyDynamicDataView3D<MovementCostModifier<TMovementMode>> data))
            {
                data = GetOrCreateSensePropertiesSystem<TGameContext, TItemId>(serviceResolver);
            }

            system = new MovementResistanceDirectionalitySystem<TMovementMode>(data);
            serviceResolver.Store(system);
            serviceResolver.Store<IMovementResistanceDirectionView<TMovementMode>>(system);
            return system;
        }

        protected void RegisterEntities<TItemId>(IServiceResolver serviceResolver, EntityRegistry<TItemId> registry)
            where TItemId : IEntityKey
        {
            registry.RegisterNonConstructable<MovementCostModifier<TMovementMode>>();
        }

        readonly struct MovementDirectionalitySystemRegisteredMarker
        {
        }
    }
}