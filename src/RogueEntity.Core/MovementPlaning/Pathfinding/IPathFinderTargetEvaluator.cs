using RogueEntity.Api.Utils;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Algorithms;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Utils;
using System;

namespace RogueEntity.Core.MovementPlaning.Pathfinding
{
    /// <summary>
    ///   Checks whether a given position is considered a valid target for a path search.
    /// </summary>
    public interface IPathFinderTargetEvaluator: IDisposable
    {
        public void Activate();
        public bool Initialize<TPosition>(in TPosition sourcePosition, DistanceCalculation c) where TPosition: IPosition<TPosition>;
        public bool IsTargetNode(int z, in Position2D pos);
        public float TargetHeuristic(int z, in Position2D pos);
        public BufferList<EntityGridPosition> CollectTargets(BufferList<EntityGridPosition>? buffer = null);

    }
}