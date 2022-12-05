using Microsoft.Extensions.ObjectPool;
using RogueEntity.Core.MovementPlaning.Pathfinding.Hierarchical.Systems;
using RogueEntity.Core.MovementPlaning.Pathfinding.SingleLevel;

namespace RogueEntity.Core.MovementPlaning.Pathfinding.Hierarchical;

public class HierarchicalPathFinderPolicy : PooledObjectPolicy<HierarchicalPathFinder>
{
    readonly HierarchicalPathfindingSystemCollection highLevelPathfindingData;
    readonly SingleLevelPathPool pathPool;

    public HierarchicalPathFinderPolicy(HierarchicalPathfindingSystemCollection highLevelPathfindingData,
                                        SingleLevelPathPool pathPool)
    {
        this.highLevelPathfindingData = highLevelPathfindingData;
        this.pathPool = pathPool;
    }

    public override HierarchicalPathFinder Create()
    {
        return new HierarchicalPathFinder(highLevelPathfindingData, pathPool);
    }

    public override bool Return(HierarchicalPathFinder obj)
    {
        obj.Reset();
        return true;
    }
}