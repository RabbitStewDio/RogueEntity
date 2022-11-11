using Microsoft.Extensions.ObjectPool;
using RogueEntity.Core.MovementPlaning.Pathfinding.Hierarchical.Systems;
using RogueEntity.Core.Utils.DataViews;

namespace RogueEntity.Core.MovementPlaning.Pathfinding.Hierarchical;

public class HierarchicalPathFinderPolicy : PooledObjectPolicy<HierarchicalPathFinder>
{
    readonly HierarchicalPathfindingSystemCollection highLevelPathfindingData;

    public HierarchicalPathFinderPolicy(DynamicDataViewConfiguration config,
                                        HierarchicalPathfindingSystemCollection highLevelPathfindingData)
    {
        this.highLevelPathfindingData = highLevelPathfindingData;
    }

    public override HierarchicalPathFinder Create()
    {
        return new HierarchicalPathFinder(highLevelPathfindingData);
    }

    public override bool Return(HierarchicalPathFinder obj)
    {
        obj.Reset();
        return true;
    }
}