using GoRogue;
using GoRogue.Pathing;
using RogueEntity.Core.Positioning.Grid;

namespace RogueEntity.Core.Movement.Pathing
{
    /// <summary>
    ///    A pathfinder that will find a movement that avoids certain cells.
    ///    This is used for making units move out of the way of other units.
    /// </summary>
    public interface IEvasionPathfinder<TGameContext, TActorId>
    {
        public PathfinderResult FindClearObstructionPath(TGameContext context,
                                                         TActorId actor,
                                                         EntityGridPosition actorPosition,
                                                         out Path<EntityGridPosition> resultPath);
    }
}