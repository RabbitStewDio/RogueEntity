using Microsoft.Extensions.ObjectPool;
using RogueEntity.Core.Movement;
using RogueEntity.Core.MovementPlaning.Pathfinding.SingleLevel;
using RogueEntity.Core.Utils;

namespace RogueEntity.Core.MovementPlaning.Pathfinding.Hierarchical;

public class HierarchicalPathfinderSource : IPathFinderSource
{
    readonly ObjectPool<IPathFinderBuilder> pathfinderBuilderPool;
    readonly ObjectPool<HierarchicalPathFinder> pathfinderPool;
    
    public HierarchicalPathfinderSource(SingleLevelPathFinderSource fragmentPathfinder, HierarchicalPathFinderPolicy policy, IMovementDataProvider movementDataProvider)
    {
        Assert.NotNull(policy, nameof(policy));
        Assert.NotNull(fragmentPathfinder, nameof(fragmentPathfinder));
        Assert.NotNull(movementDataProvider, nameof(movementDataProvider));

        this.pathfinderPool = new DefaultObjectPool<HierarchicalPathFinder>(policy);
        var builderPolicy = new HierarchicalPathfinderBuilderPolicy(movementDataProvider, pathfinderPool, fragmentPathfinder);
        this.pathfinderBuilderPool = new DefaultObjectPool<IPathFinderBuilder>(builderPolicy.DownGrade<IPathFinderBuilder, HierarchicalPathfinderBuilder>());
    }

    public void Return(IPathFinderBuilder t)
    {
        if (t is HierarchicalPathfinderBuilder s)
        {
            pathfinderBuilderPool.Return(s);
        }
    }

    public PooledObjectHandle<IPathFinderBuilder> GetPathFinder()
    {
        var pathFinderBuilder = pathfinderBuilderPool.Get();
        return new PooledObjectHandle<IPathFinderBuilder>(pathfinderBuilderPool, pathFinderBuilder);
    }

    class HierarchicalPathfinderBuilderPolicy : IPooledObjectPolicy<HierarchicalPathfinderBuilder>
    {
        readonly IMovementDataProvider movementDataProvider;
        readonly ObjectPool<HierarchicalPathFinder> pathfinderPool;
        readonly SingleLevelPathFinderSource fragmentPathfinderSource;

        public HierarchicalPathfinderBuilderPolicy(IMovementDataProvider movementDataProvider,
                                                   ObjectPool<HierarchicalPathFinder> pathfinderPool,
                                                   SingleLevelPathFinderSource fragmentPathfinderSource)
        {
            this.movementDataProvider = movementDataProvider;
            this.pathfinderPool = pathfinderPool;
            this.fragmentPathfinderSource = fragmentPathfinderSource;
        }

        public HierarchicalPathfinderBuilder Create()
        {
            return new HierarchicalPathfinderBuilder(movementDataProvider, pathfinderPool, fragmentPathfinderSource.GetPathFinder());
        }

        public bool Return(HierarchicalPathfinderBuilder obj)
        {
            obj.Reset();
            return true;
        }
    }
}