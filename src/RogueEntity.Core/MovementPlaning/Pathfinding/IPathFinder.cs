using RogueEntity.Api.Utils;
using RogueEntity.Core.Movement;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Algorithms;
using System;

namespace RogueEntity.Core.MovementPlaning.Pathfinding
{
    /// <summary>
    ///   Attempts to find a navigation path from the given source to any target configured in the
    ///   IPathFinderBuilder that created this instance. 
    /// </summary>
    public interface IPathFinder : IDisposable
    {
        public PathFinderResult TryFindPath<TPosition>(in TPosition source,
                                                       out BufferList<(TPosition, IMovementMode)> path,
                                                       BufferList<(TPosition, IMovementMode)>? pathBuffer = null,
                                                       int searchLimit = int.MaxValue)
            where TPosition: IPosition<TPosition>;
    }

    public interface IPathFinderPerformanceView
    {
        public int NodesEvaluated { get; }
        public TimeSpan TimeElapsed { get; }
    }
}
