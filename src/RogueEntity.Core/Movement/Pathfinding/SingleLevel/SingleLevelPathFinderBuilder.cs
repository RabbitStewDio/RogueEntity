using System.Collections.Generic;
using Microsoft.Extensions.ObjectPool;
using RogueEntity.Core.Movement.Cost;

namespace RogueEntity.Core.Movement.Pathfinding.SingleLevel
{
    public class SingleLevelPathFinderBuilder: IPathFinderBuilder
    {
        readonly ObjectPool<SingleLevelPathFinder> pathFinderPool;
        readonly ObjectPool<DefaultPathFinderTargetEvaluator> defaultTargetEvaluators;

        IPathFinderTargetEvaluator targetEvaluator;
        
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

        public Dictionary<IMovementMode, SingleLevelPathFinderSource.MovementSourceData> MovementCostData { get; set; }

        public IPathFinderBuilder WithTarget(IPathFinderTargetEvaluator evaluator)
        {
            this.targetEvaluator = evaluator;
            return this;
        }

        public IPathFinder Build(in PathfindingMovementCostFactors movementProfile)
        {
            var pf = pathFinderPool.Get();
            var te = targetEvaluator ?? defaultTargetEvaluators.Get(); 
            te.Activate();
            pf.Configure(this, te);
            
            foreach (var m in movementProfile.MovementCosts)
            {
                if (MovementCostData.TryGetValue(m.MovementMode, out var mapData))
                {
                    pf.ConfigureMovementProfile(m, mapData.Costs, mapData.Directions);
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