using EnttSharp.Entities;
using RogueEntity.Core.Infrastructure.Meta.Items;

namespace RogueEntity.Core.Movement.Pathing
{
    public class ActorMovementCostCacheTrait<TGameContext, TActorId> : SimpleReferenceItemComponentTraitBase<TGameContext, TActorId, ActorMovementCostCache> 
        where TActorId : IEntityKey
    {
        public ActorMovementCostCacheTrait() : base("Core.Actor.System.MovementCostCache", 0)
        {
        }

        protected override ActorMovementCostCache CreateInitialValue(TGameContext c, TActorId actor)
        {
            return new ActorMovementCostCache();
        }
    }
}