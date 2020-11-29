using System;
using System.Collections.Generic;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Utils.Algorithms;

namespace RogueEntity.Core.Movement.Pathfinding
{
    public interface IPathFinder: IDisposable
    {
        public PathFinderResult TryFindPath(EntityGridPosition source,
                                            out List<(EntityGridPosition, IMovementMode)> path,
                                            List<(EntityGridPosition, IMovementMode)> pathBuffer = null,
                                            int searchLimit = int.MaxValue);
        
    }

    public interface IPathFinderPerformanceView
    {
        public int NodesEvaluated { get; }
        public TimeSpan TimeElapsed { get; }
    }
}