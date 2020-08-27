using EnTTSharp.Entities;
using RogueEntity.Core.Infrastructure.Modules;
using RogueEntity.Core.Movement.ItemCosts;
using RogueEntity.Core.Movement.Pathing;

namespace RogueEntity.Core.Movement
{
    public class MovementModule<TGameContext>: ModuleBase<TGameContext>
    {
        public MovementModule()
        {
            Id = "Core.Movement";
            Author = "RogueEntity.Core";
            Name = "RogueEntity Core Module - Movement";
            Description = "Provides base classes and behaviours for movement decisions and pathfinding";
        }
         
        protected void RegisterAll<TItemId>(TGameContext context, IModuleInitializer<TGameContext> initializer)
            where TItemId : IEntityKey
        {
            var entityContext = initializer.DeclareEntityContext<TItemId>();
            entityContext.Register("Core.Entities.Movement.ActorData", -19_000, RegisterActorEntities);
            entityContext.Register("Core.Entities.Movement.ItemData", -19_000, RegisterItemEntities);
        }

        void RegisterItemEntities<TItemId>(EntityRegistry<TItemId> entities) where TItemId : IEntityKey
        {
            entities.RegisterNonConstructable<MovementCostProperties>();
        }

        protected void RegisterActorEntities<TItemId>(EntityRegistry<TItemId> entities) where TItemId : IEntityKey
        {
            entities.RegisterNonConstructable<ActorMovementCostCache>();
            entities.RegisterNonConstructable<SwimmingMovementData>();
            entities.RegisterNonConstructable<WalkingMovementData>();
            entities.RegisterNonConstructable<EtherealMovementData>();
            entities.RegisterNonConstructable<FlyingMovementData>();
        }
    }
}