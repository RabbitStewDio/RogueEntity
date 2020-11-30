using System;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.Algorithms;

namespace RogueEntity.Core.Movement.Pathfinding
{
    public class DefaultPathFinderTargetEvaluator: IPathFinderTargetEvaluator
    {
        readonly Action<DefaultPathFinderTargetEvaluator> returnToPoolFunction;
        DistanceCalculation distanceCalculation;
        Position2D targetPosition;
        int targetZLevel;
        bool disposed;

        public DefaultPathFinderTargetEvaluator(Action<DefaultPathFinderTargetEvaluator> returnToPoolFunction = null)
        {
            this.returnToPoolFunction = returnToPoolFunction;
        }

        public DefaultPathFinderTargetEvaluator WithTargetPosition<TPosition>(TPosition value) 
            where TPosition: IPosition
        {
            if (value.IsInvalid) throw new Exception();
            targetZLevel = value.GridZ;
            targetPosition = value.ToGridXY();
            return this;
        }

        public void Dispose()
        {
            if (!disposed)
            {
                this.disposed = true;
                this.returnToPoolFunction?.Invoke(this);
            }
        }

        public void Activate()
        {
            this.disposed = false;
            this.distanceCalculation = DistanceCalculation.Euclid;
        }

        public bool Initialize<TPosition>(in TPosition sourcePosition, DistanceCalculation c)
            where TPosition : IPosition
        {
            if (sourcePosition.IsInvalid)
            {
                return false;
            }
            
            this.distanceCalculation = c;
            return true;
        }

        public bool IsTargetNode(int z, in Position2D pos)
        {
            return z == targetZLevel && pos == targetPosition;
        }

        public float TargetHeuristic(int z, in Position2D pos)
        {
            return (float) distanceCalculation.Calculate2D(pos, targetPosition);
        }
    }
}