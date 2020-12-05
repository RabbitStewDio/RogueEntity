using Microsoft.Extensions.ObjectPool;

namespace RogueEntity.Core.Movement.GoalFinding.SingleLevel
{
    public class SingleLevelGoalFinderPolicy : IPooledObjectPolicy<SingleLevelGoalFinder>
    {
        public SingleLevelGoalFinder Create()
        {
            return new SingleLevelGoalFinder();
        }

        public bool Return(SingleLevelGoalFinder obj)
        {
            return true;
        }
    }
}