using System.Collections.Generic;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Utils.Algorithms;

namespace RogueEntity.Core.Movement.Pathfinding
{
    public interface IPathFinder
    {
        public PathFinderResult TryFindPath(EntityGridPosition source,
                                            EntityGridPosition target,
                                            out List<(EntityGridPosition, IMovementMode)> path,
                                            List<(EntityGridPosition, IMovementMode)> pathBuffer = null,
                                            int searchLimit = int.MaxValue);
    }
}