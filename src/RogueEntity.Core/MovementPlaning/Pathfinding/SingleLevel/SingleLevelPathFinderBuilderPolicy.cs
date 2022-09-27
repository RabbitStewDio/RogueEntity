using Microsoft.Extensions.ObjectPool;

namespace RogueEntity.Core.MovementPlaning.Pathfinding.SingleLevel
{
    class SingleLevelPathFinderBuilderPolicy : IPooledObjectPolicy<SingleLevelPathFinderBuilder>
    {
        readonly ObjectPool<SingleLevelPathFinder> pathFinderPool;
        readonly ObjectPool<DefaultPathFinderTargetEvaluator> targetEvaluatorPool;

        public SingleLevelPathFinderBuilderPolicy(IPooledObjectPolicy<SingleLevelPathFinder> policy)
        {
            this.pathFinderPool = new DefaultObjectPool<SingleLevelPathFinder>(policy);
            this.targetEvaluatorPool = new DefaultObjectPool<DefaultPathFinderTargetEvaluator>(new DefaultPathFinderTargetEvaluatorPolicy(ReturnTargetEvaluator));
        }

        void ReturnTargetEvaluator(DefaultPathFinderTargetEvaluator obj)
        {
            this.targetEvaluatorPool.Return(obj);
        }

        public SingleLevelPathFinderBuilder Create()
        {
            return new SingleLevelPathFinderBuilder(pathFinderPool, targetEvaluatorPool);
        }

        public bool Return(SingleLevelPathFinderBuilder obj)
        {
            obj.Reset();
            return true;
        }
    }
}