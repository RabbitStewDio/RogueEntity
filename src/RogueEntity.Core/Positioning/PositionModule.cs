using EnTTSharp.Entities;
using RogueEntity.Core.Infrastructure.Commands;
using RogueEntity.Core.Infrastructure.GameLoops;
using RogueEntity.Core.Infrastructure.Modules;
using RogueEntity.Core.Positioning.Continuous;
using RogueEntity.Core.Positioning.Grid;

namespace RogueEntity.Core.Positioning
{
    public static class PositionModule
    {
        public static readonly EntitySystemId RegisterGridPositions = "Core.Entities.GridPosition";
        public static readonly EntitySystemId RegisterContinuousPositions = "Core.Entities.ContinuousPosition";
        public static readonly EntitySystemId RegisterClearContinuousPositionChangeTracker = "Core.Entities.ContinuousPosition.ClearChangeTracker";
        public static readonly EntitySystemId RegisterClearGridPositionChangeTracker = "Core.Entities.GridPosition.ClearChangeTracker";
    }

    public abstract class PositionModule<TGameContext> : ModuleBase<TGameContext>
    {
        protected PositionModule()
        {
            Id = "Core.Inventory";
            Author = "RogueEntity.Core";
            Name = "RogueEntity Core Module - Positioning";
            Description = "Provides support for positioning items in a grid or continuous coordinate system";
        }

        protected void RegisterAll<TActorId>(TGameContext context, IModuleInitializer<TGameContext> initializer)
            where TActorId : IEntityKey
        {
            var entityContext = initializer.DeclareEntityContext<TActorId>();
            entityContext.Register(PositionModule.RegisterGridPositions, 0, RegisterGridEntities);
            entityContext.Register(PositionModule.RegisterContinuousPositions, 0, RegisterGridEntities);

            entityContext.Register(PositionModule.RegisterClearGridPositionChangeTracker, 10, RegisterGridEntities);
            entityContext.Register(PositionModule.RegisterClearContinuousPositionChangeTracker, 10, RegisterGridEntities);
        }

        protected void RegisterGridEntities<TActorId>(EntityRegistry<TActorId> registry)
            where TActorId : IEntityKey
        {
            registry.RegisterNonConstructable<EntityGridPosition>();
            registry.RegisterNonConstructable<EntityGridPositionChangedMarker>();
        }

        protected void RegisterContinuousEntities<TActorId>(EntityRegistry<TActorId> registry)
            where TActorId : IEntityKey
        {
            registry.RegisterNonConstructable<ContinuousMapPosition>();
            registry.RegisterNonConstructable<ContinuousMapPositionChangedMarker>();
        }

        protected void RegisterClearGridPositionChangeTrackers<TActorId>(IGameLoopSystemRegistration<TGameContext> context,
                                                                         EntityRegistry<TActorId> registry,
                                                                         ICommandHandlerRegistration<TGameContext, TActorId> processor) where TActorId : IEntityKey
        {
            void ClearGridPositionAction(TGameContext c)
            {
                registry.ResetComponent<EntityGridPositionChangedMarker>();
            }

            context.AddFixedStepHandlers(ClearGridPositionAction);
            EntityGridPositionChangedMarker.InstallChangeHandler(registry);
        }

        protected void RegisterClearContinuousPositionChangeTrackers<TActorId>(IGameLoopSystemRegistration<TGameContext> context,
                                                                               EntityRegistry<TActorId> registry,
                                                                               ICommandHandlerRegistration<TGameContext, TActorId> processor) where TActorId : IEntityKey
        {
            void ClearContinuousPositionAction(TGameContext c)
            {
                registry.ResetComponent<ContinuousMapPositionChangedMarker>();
            }

            context.AddFixedStepHandlers(ClearContinuousPositionAction);
            ContinuousMapPositionChangedMarker.InstallChangeHandler(registry);
        }
    }
}