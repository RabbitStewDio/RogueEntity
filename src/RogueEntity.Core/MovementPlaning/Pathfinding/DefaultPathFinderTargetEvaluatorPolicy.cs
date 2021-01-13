using Microsoft.Extensions.ObjectPool;
using System;

namespace RogueEntity.Core.MovementPlaning.Pathfinding
{
    public class DefaultPathFinderTargetEvaluatorPolicy : IPooledObjectPolicy<DefaultPathFinderTargetEvaluator>
    {
        readonly Action<DefaultPathFinderTargetEvaluator> returnToPoolFunction;

        public DefaultPathFinderTargetEvaluatorPolicy(Action<DefaultPathFinderTargetEvaluator> returnToPoolFunction)
        {
            this.returnToPoolFunction = returnToPoolFunction;
        }

        public DefaultPathFinderTargetEvaluator Create()
        {
            return new DefaultPathFinderTargetEvaluator(returnToPoolFunction);
        }

        public bool Return(DefaultPathFinderTargetEvaluator obj)
        {
            return true;
        }
    }
}