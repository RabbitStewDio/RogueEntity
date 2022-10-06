using Microsoft.Extensions.ObjectPool;
using RogueEntity.Core.Movement;
using RogueEntity.Core.Movement.Cost;
using System;
using System.Collections.Generic;

namespace RogueEntity.Core.MovementPlaning.Pathfinding.SingleLevel
{
    public class SingleLevelPathFinderBuilder: IPathFinderBuilder
    {
        readonly ObjectPool<SingleLevelPathFinder> pathFinderPool;
        readonly ObjectPool<DefaultPathFinderTargetEvaluator> defaultTargetEvaluators;

        IPathFinderTargetEvaluator? targetEvaluator;
        
        public SingleLevelPathFinderBuilder(ObjectPool<SingleLevelPathFinder> pathFinderPool,
                                            ObjectPool<DefaultPathFinderTargetEvaluator> defaultTargetEvaluators)
        {
            this.pathFinderPool = pathFinderPool;
            this.defaultTargetEvaluators = defaultTargetEvaluators;
        }

        public void Return(SingleLevelPathFinder pf)
        {
            pf.TargetEvaluator?.Dispose();
            pathFinderPool.Return(pf);
        }

        public IReadOnlyDictionary<IMovementMode, MovementSourceData>? MovementCostData { get; set; }

        public IPathFinderBuilder WithTarget(IPathFinderTargetEvaluator evaluator)
        {
            this.targetEvaluator = evaluator;
            return this;
        }

        public IPathFinder Build(in AggregateMovementCostFactors movementProfile)
        {
            if (MovementCostData == null) throw new InvalidOperationException();
            
            var pf = pathFinderPool.Get();
            var te = targetEvaluator ?? defaultTargetEvaluators.Get(); 
            te.Activate();
            pf.Configure(this, te);
            
            foreach (var m in movementProfile.MovementCosts)
            {
                if (MovementCostData.TryGetValue(m.MovementMode, out var mapData))
                {
                    pf.ConfigureMovementProfile(m, mapData.Costs, mapData.InboundDirections, mapData.OutboundDirections);
                }
            }
        
            return pf;
        }

        public void Reset()
        {
            this.MovementCostData = null;
            this.targetEvaluator = null;
        }
    }
}