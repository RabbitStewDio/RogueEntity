using GoRogue.Pathing;
using RogueEntity.Core.Positioning.Grid;

namespace RogueEntity.Core.Movement.Pathing
{
    public interface IPathfinderService<TGameContext, TActorId>
    {
        PathfinderResult FindPath(TGameContext context,
                                  TActorId actor,
                                  EntityGridPosition startPosition,
                                  EntityGridPosition targetPos,
                                  float targetDistance,
                                  out Path<EntityGridPosition> resultPath);
    }
}