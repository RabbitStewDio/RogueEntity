using EnTTSharp;
using JetBrains.Annotations;
using RogueEntity.Core.GridProcessing.Directionality;
using RogueEntity.Core.Movement.Cost;
using RogueEntity.Core.Movement.CostModifier;
using RogueEntity.Core.MovementPlaning.Pathfinding.Hierarchical.Data;
using RogueEntity.Core.MovementPlaning.Pathfinding.Hierarchical.Systems;
using RogueEntity.Core.MovementPlaning.Pathfinding.SingleLevel;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Algorithms;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.DataViews;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace RogueEntity.Core.MovementPlaning.Pathfinding.Hierarchical;

public class HierarchicalPathFinder : IPathFinder
{
    readonly struct PathfinderNode
    {
        public enum NodeState : byte
        {
            [UsedImplicitly] None = 0,
            Open = 1,
            Closed = 2
        }

        // Whether or not the node has been closed
        public readonly NodeState State;

        public readonly Position2D PreviousNode;

        public readonly ushort DistanceFromStart;

        // (Known) distance from start to this node, by shortest known path
        public readonly float AccumulatedCost;

        public PathfinderNode(NodeState state, float accumulatedCost, Position2D previousNode, ushort distanceFromStart)
        {
            State = state;
            AccumulatedCost = accumulatedCost;
            PreviousNode = previousNode;
            DistanceFromStart = distanceFromStart;
        }

        public static PathfinderNode Start(Position2D origin, float cost)
        {
            return new PathfinderNode(NodeState.Open, cost, origin, 0);
        }

        public PathfinderNode Close()
        {
            return new PathfinderNode(NodeState.Closed, AccumulatedCost, PreviousNode, DistanceFromStart);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsClosed()
        {
            return State == NodeState.Closed;
        }
    }

    readonly List<MovementCostData3D> movementSourceData;
    readonly PriorityQueue<float, Position2D> openNodes;
    readonly Dictionary<Position2D, AStarNode> closedNodes;
    readonly HierarchicalPathfindingSystemCollection highLevelPathfindingData;
    readonly HierarchicalPathfinderWorker highlevelPathfinderWorker;

    Optional<int> movementDataSource;
    HierarchicalPathfinderBuilder? currentOwner;
    IPathFinderTargetEvaluator? targetEvaluator;
    IPathFinder? fragmentPathfinder;
    bool disposed;

    public HierarchicalPathFinder(HierarchicalPathfindingSystemCollection highLevelPathfindingData)
    {
        this.highLevelPathfindingData = highLevelPathfindingData ?? throw new ArgumentNullException(nameof(highLevelPathfindingData));
        movementSourceData = new List<MovementCostData3D>();
        openNodes = new PriorityQueue<float, Position2D>(4096);
        closedNodes = new Dictionary<Position2D, AStarNode>();
        movementDataSource = Optional.Empty();
    }

    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        movementDataSource = Optional.Empty();
        disposed = true;
        currentOwner?.Return(this);
        currentOwner = null;
        targetEvaluator = null;
        openNodes.Clear();
        closedNodes.Clear();
    }

    public IPathFinderTargetEvaluator? TargetEvaluator => targetEvaluator;

    public IPathFinder WithTarget(IPathFinderTargetEvaluator evaluator)
    {
        this.targetEvaluator = evaluator ?? throw new ArgumentNullException(nameof(evaluator));
        return this;
    }

    public bool TryFindPath<TPosition>(in TPosition source, 
                                       out (PathFinderResult resultHint, IPath path, float pathCost) path, 
                                       int searchLimit = Int32.MaxValue) where TPosition : IPosition<TPosition>
    {
        Assert.NotNull(fragmentPathfinder);

        var heuristics = FindBestDistanceCalculation<TPosition>();
        if (targetEvaluator == null || !targetEvaluator.Initialize(source, heuristics))
        {
            path = default;
            return false;
        }

        var startPos = source.ToGridXY();
        var startZ = source.GridZ;

        if (!highLevelPathfindingData.ZoneDataView.TryGetRegionView2D(startZ, out var regionView))
        {
            path = default;
            return false;
        }

        IReadOnlyBoundedDataView<(TraversableZoneId, DirectionalityInformation)>? regionTile = null;
        var startZoneRaw = regionView.TryGetMapValue(ref regionTile, startPos.X, startPos.Y, default);
        if (regionTile == null)
        {
            path = default;
            return false;
        }

        var startZone = new GlobalTraversableZoneId(regionTile.Bounds.Position, startZoneRaw.Item1);
        highlevelPathfinderWorker.Initialize(movementSourceData, startZone, startPos, startZ);
        var sameZone = highlevelPathfinderWorker.AddTargets(targetEvaluator);

        Optional<(IPath path, float cost)> maybeDirectPath = Optional.Empty();
        if (sameZone)
        {
            if (this.fragmentPathfinder.TryFindPath(source, out var directPath))
            {
                if (directPath.resultHint == PathFinderResult.Found)
                {
                    maybeDirectPath = Optional.ValueOf((directPath.path, directPath.pathCost));
                }
                else
                {
                    directPath.path.Dispose();
                }
            }
        }

        if (!highlevelPathfinderWorker.FindPath(maybeDirectPath, fragmentPathfinder, out var hpath, out var cost))
        {
            if (maybeDirectPath.TryGetValue(out var value))
            {
                path = (PathFinderResult.Found, value.path, value.cost);
                return true;
            }
            
            path = default;
            return false;
        }
        
        path = (PathFinderResult.Found, hpath, cost);
        return false;
    }

    DistanceCalculation FindBestDistanceCalculation<TPosition>() where TPosition : IPosition<TPosition>
    {
        var heuristics = DistanceCalculation.Manhattan;
        foreach (var m in movementSourceData)
        {
            if (heuristics.IsOtherMoreAccurate(m.MovementCost.MovementStyle))
            {
                heuristics = m.MovementCost.MovementStyle;
            }
        }

        return heuristics;
    }

    public void Configure(HierarchicalPathfinderBuilder owner,
                          IPathFinderTargetEvaluator evaluator,
                          IPathFinder pathFinder)
    {
        this.disposed = false;
        this.movementDataSource = Optional.Empty();
        this.movementSourceData.Clear();
        this.fragmentPathfinder = pathFinder ?? throw new ArgumentNullException(nameof(pathFinder));
        this.currentOwner = owner ?? throw new ArgumentNullException(nameof(owner));
        this.targetEvaluator = evaluator ?? throw new ArgumentNullException(nameof(evaluator));
    }

    public void Reset()
    {
        this.disposed = false;
        this.movementDataSource = Optional.Empty();
        this.movementSourceData.Clear();
    }


    public void ConfigureMovementProfile(in MovementCost movementCost,
                                         IReadOnlyDynamicDataView3D<float> costs,
                                         IReadOnlyDynamicDataView3D<DirectionalityInformation> inboundDirections,
                                         IReadOnlyDynamicDataView3D<DirectionalityInformation> outboundDirections)
    {
        this.movementSourceData.Add(new MovementCostData3D(in movementCost, costs, inboundDirections, outboundDirections));
    }
}