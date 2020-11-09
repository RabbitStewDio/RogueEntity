using EnTTSharp.Entities;
using RogueEntity.Core.GridProcessing.LayerAggregation;
using RogueEntity.Core.Infrastructure.Commands;
using RogueEntity.Core.Infrastructure.GameLoops;
using RogueEntity.Core.Infrastructure.Modules;
using RogueEntity.Core.Infrastructure.Modules.Services;
using RogueEntity.Core.Movement.Resistance.Directions;
using RogueEntity.Core.Movement.Resistance.Map;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Utils.DataViews;

namespace RogueEntity.Core.Movement.Resistance
{
    public abstract class MovementResistanceModule<TMovementMode>: ModuleBase
    {
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
            serviceResolver.Store<IAggregationLayerSystem<TGameContext, MovementCost<TMovementMode>>>(system);
            serviceResolver.Store<IReadOnlyDynamicDataView3D<MovementCost<TMovementMode>>>(system);
            return system;
        }

        protected virtual MovementResistanceDirectionalitySystem<TMovementMode> GetOrCreateDirectionalitySystem<TGameContext, TItemId>(IServiceResolver serviceResolver)
        {
            if (serviceResolver.TryResolve(out MovementResistanceDirectionalitySystem<TMovementMode> system))
            {
                return system;
            }

            if (!serviceResolver.TryResolve(out IReadOnlyDynamicDataView3D<MovementCost<TMovementMode>> data))
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
            registry.RegisterNonConstructable<MovementCost<TMovementMode>>();
        }

        readonly struct MovementDirectionalitySystemRegisteredMarker
        {
        }

    }
}