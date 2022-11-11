using Microsoft.Extensions.ObjectPool;
using RogueEntity.Core.Movement;
using RogueEntity.Core.Movement.Cost;
using RogueEntity.Core.Utils;
using System;

namespace RogueEntity.Core.MovementPlaning.Pathfinding.Hierarchical;

public class HierarchicalPathfinderBuilder : IPathFinderBuilder
{
    readonly IMovementDataProvider movementDataProvider;
    readonly ObjectPool<HierarchicalPathFinder> pathfinderPool;
    readonly PooledObjectHandle<IPathFinderBuilder> fragmentPathfinderBuilder;
    IPathFinderTargetEvaluator? targetEvaluator;

    public HierarchicalPathfinderBuilder(IMovementDataProvider movementDataProvider,
                                         ObjectPool<HierarchicalPathFinder> pathfinderPool,
                                         PooledObjectHandle<IPathFinderBuilder> fragmentPathfinderBuilder)
    {
        this.movementDataProvider = movementDataProvider;
        this.pathfinderPool = pathfinderPool;
        this.fragmentPathfinderBuilder = fragmentPathfinderBuilder;
    }

    public void Return(HierarchicalPathFinder pf)
    {
        pf.TargetEvaluator?.Dispose();
        pathfinderPool.Return(pf);
        fragmentPathfinderBuilder.Dispose();
    }

    public IPathFinderBuilder WithTarget(IPathFinderTargetEvaluator evaluator)
    {
        this.targetEvaluator = evaluator;
        return this;
    }

    public IPathFinder Build(in AggregateMovementCostFactors movementProfile)
    {
        if (movementDataProvider.MovementCosts == null ||
            movementDataProvider.MovementCosts.Count == 0)
        {
            throw new InvalidOperationException("No movement cost data given");
        }

        var pf = pathfinderPool.Get();
        var te = targetEvaluator ?? throw new InvalidOperationException("No target evaluator given");
        te.Activate();
        var fragmentPathfinder = fragmentPathfinderBuilder.Data.Build(movementProfile);
        pf.Configure(this, te, fragmentPathfinder);

        foreach (var m in movementProfile.MovementCosts)
        {
            if (movementDataProvider.MovementCosts.TryGetValue(m.MovementMode, out var mapData))
            {
                pf.ConfigureMovementProfile(m, mapData.Costs, mapData.InboundDirections, mapData.OutboundDirections);
            }
        }

        return pf;
    }

    public void Reset()
    {
        this.targetEvaluator = null;
    }
}