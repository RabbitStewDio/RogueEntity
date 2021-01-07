using System;
using System.Collections.Generic;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Algorithms;
using RogueEntity.Core.Utils;

namespace RogueEntity.Core.Movement.Pathfinding
{
    public interface IPathFinder : IDisposable
    {
        public PathFinderResult TryFindPath<TPosition>(in TPosition source,
                                                       out BufferList<(TPosition, IMovementMode)> path,
                                                       BufferList<(TPosition, IMovementMode)> pathBuffer = null,
                                                       int searchLimit = int.MaxValue)
            where TPosition: IPosition<TPosition>;
    }

    public interface IPathFinderPerformanceView
    {
        public int NodesEvaluated { get; }
        public TimeSpan TimeElapsed { get; }
    }
}
