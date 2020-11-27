using System;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.Algorithms;

namespace RogueEntity.Core.Movement.Pathfinding
{
    public interface IPathFinderTargetEvaluator: IDisposable
    {
        public void Activate();
        public bool Initialize<TPosition>(in TPosition sourcePosition, DistanceCalculation c) where TPosition: IPosition;
        public bool IsTargetNode(int z, in Position2D pos);
        public float TargetHeuristic(int z, in Position2D pos);
    }
}