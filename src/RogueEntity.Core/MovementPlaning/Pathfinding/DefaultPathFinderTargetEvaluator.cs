using Microsoft.Extensions.ObjectPool;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Algorithms;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Positioning.MapLayers;
using RogueEntity.Core.Utils;
using System;

namespace RogueEntity.Core.MovementPlaning.Pathfinding
{
    public class DefaultPathFinderTargetEvaluator: IPathFinderTargetEvaluator
    {
        static void ReturnToPool(DefaultPathFinderTargetEvaluator e)
        {
            pool.Return(e);
        }
        
        static readonly ObjectPool<DefaultPathFinderTargetEvaluator> pool = 
            new DefaultObjectPool<DefaultPathFinderTargetEvaluator>(new DefaultPathFinderTargetEvaluatorPolicy(ReturnToPool));

        public static DefaultPathFinderTargetEvaluator GetSharedInstance() => pool.Get(); 
        
        readonly Action<DefaultPathFinderTargetEvaluator>? returnToPoolFunction;
        DistanceCalculation distanceCalculation;
        Position2D targetPosition;
        int targetZLevel;
        bool disposed;

        public DefaultPathFinderTargetEvaluator(Action<DefaultPathFinderTargetEvaluator>? returnToPoolFunction = null)
        {
            this.returnToPoolFunction = returnToPoolFunction;
        }

        public DefaultPathFinderTargetEvaluator WithTargetPosition<TPosition>(TPosition value) 
            where TPosition: IPosition<TPosition>
        {
            if (value.IsInvalid) throw new Exception();
            targetZLevel = value.GridZ;
            targetPosition = value.ToGridXY();
            return this;
        }

        public BufferList<EntityGridPosition> CollectTargets(BufferList<EntityGridPosition>? buffer = null)
        {
            buffer = BufferList.PrepareBuffer(buffer);
            buffer.Add(EntityGridPosition.Of(MapLayer.Indeterminate, targetPosition.X, targetPosition.Y, targetZLevel));
            return buffer;
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
            where TPosition : IPosition<TPosition>
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