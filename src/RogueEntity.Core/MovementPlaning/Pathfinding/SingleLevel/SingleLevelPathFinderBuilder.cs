using Microsoft.Extensions.ObjectPool;
using RogueEntity.Core.Movement;
using RogueEntity.Core.Movement.Cost;
using System;

namespace RogueEntity.Core.MovementPlaning.Pathfinding.SingleLevel
{
    public class SingleLevelPathFinderBuilder: IPathFinderBuilder
    {
        readonly IMovementDataProvider movementDataProvider;
        readonly ObjectPool<SingleLevelPathFinder> pathFinderPool;

        IPathFinderTargetEvaluator? targetEvaluator;
        
        public SingleLevelPathFinderBuilder(IMovementDataProvider movementDataProvider,
                                            ObjectPool<SingleLevelPathFinder> pathFinderPool)
        {
            this.movementDataProvider = movementDataProvider;
            this.pathFinderPool = pathFinderPool;
        }

        public void Return(SingleLevelPathFinder pf)
        {
            pf.TargetEvaluator?.Dispose();
            pathFinderPool.Return(pf);
        }

        public IPathFinderBuilder WithTarget(IPathFinderTargetEvaluator evaluator)
        {
            this.targetEvaluator = evaluator;
            return this;
        }

        public IPathFinder Build(in AggregateMovementCostFactors movementProfile)
        {
            if (movementDataProvider.MovementCosts == null ||
                movementDataProvider.MovementCosts.Count == 0)
            {
                throw new InvalidOperationException("No movement cost data given");
            }
            
            var pf = pathFinderPool.Get();
            pf.Configure(this);
            
            foreach (var m in movementProfile.MovementCosts)
            {
                if (movementDataProvider.MovementCosts.TryGetValue(m.MovementMode, out var mapData))
                {
                    pf.ConfigureMovementProfile(m, mapData.Costs, mapData.InboundDirections, mapData.OutboundDirections);
                }
            }
        
            if (targetEvaluator != null)
            {
                pf.WithTarget(targetEvaluator);
            }
            return pf;
        }

        public void Reset()
        {
            this.targetEvaluator = null;
        }
    }
}