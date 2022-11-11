using Microsoft.Extensions.ObjectPool;
using RogueEntity.Core.MovementPlaning.Pathfinding.SingleLevel;

namespace RogueEntity.Core.MovementPlaning.GoalFinding.SingleLevel;

public class SingleLevelGoalFinderPolicy : IPooledObjectPolicy<SingleLevelGoalFinder>
{
    readonly SingleLevelPathPool pathPool;

    public SingleLevelGoalFinderPolicy(SingleLevelPathPool pathPool)
    {
        this.pathPool = pathPool;
    }

    public SingleLevelGoalFinder Create()
    {
        return new SingleLevelGoalFinder(pathPool);
    }

    public bool Return(SingleLevelGoalFinder obj)
    {
        return true;
    }
}