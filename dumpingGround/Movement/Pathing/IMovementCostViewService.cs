using EnTTSharp.Entities;

namespace RogueEntity.Core.Movement.Pathing
{
    public interface IMovementCostViewService<TGameContext, TActorId>
        where TActorId : IEntityKey
    {
        public ActorMovementCostCache CreateCostView(TGameContext context, TActorId actor, int zLevel);
    }
}