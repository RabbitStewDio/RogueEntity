using System;
using Microsoft.Extensions.ObjectPool;

namespace RogueEntity.Core.Movement.Pathfinding
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