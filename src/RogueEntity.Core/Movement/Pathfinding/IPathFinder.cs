using System;
using System.Collections.Generic;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Algorithms;

namespace RogueEntity.Core.Movement.Pathfinding
{
    public interface IPathFinder : IDisposable
    {
        public PathFinderResult TryFindPath<TPosition>(in TPosition source,
                                                       out List<(TPosition, IMovementMode)> path,
                                                       List<(TPosition, IMovementMode)> pathBuffer = null,
                                                       int searchLimit = int.MaxValue)
            where TPosition: IPosition<TPosition>;
    }

    public interface IPathFinderPerformanceView
    {
        public int NodesEvaluated { get; }
        public TimeSpan TimeElapsed { get; }
    }
}
