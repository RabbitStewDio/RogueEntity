using Microsoft.Extensions.ObjectPool;
using RogueEntity.Api.Utils;
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
using System.Collections.Generic;
using IMovementMode = RogueEntity.Core.Movement.IMovementMode;

namespace RogueEntity.Core.MovementPlaning.Pathfinding.Hierarchical;

partial class HierarchicalPathfinderWorker : DijkstraGridBase<IMovementMode>
{
    readonly SingleLevelPathPool pathPool;
    readonly ObjectPool<List<Position2D>> positionPool;
    readonly HierarchicalPathfindingSystemCollection data;
    readonly Dictionary<GlobalTraversableZoneId, List<Position2D>> targetsByZones;
    readonly List<MovementCostData2D> movementCostsOnLevel;
    readonly PriorityQueue<float, HighLevelNode> openNodesHighLevel;
    readonly ReadOnlyListWrapper<Direction>[] directionData;
    readonly Dictionary<IMovementMode, MovementCost> movementCosts;
    
    readonly BufferList<IReadOnlyBoundedDataView<DirectionalityInformation>?> inboundDirectionsTile;
    readonly BufferList<IReadOnlyBoundedDataView<DirectionalityInformation>?> outboundDirectionsTile;
    readonly BufferList<IReadOnlyBoundedDataView<float>?> costsTile;

    readonly BoundedDataView<IMovementMode> nodesSources;
    readonly Dictionary<Position2D, HighLevelNode> closedNodesHighLevel;
    PathfinderRegionEdgeData2D? edgeData2D;

    int z;
    GlobalTraversableZoneId originZone;
    Position2D originPos;
    PathfinderRegionDataView? zoneRegion;
    PathfinderRegionEdgeData? edgeRegion;
    IPathFinderTargetEvaluator? targetEvaluator;

    public HierarchicalPathfinderWorker(DynamicDataViewConfiguration config,
                                        SingleLevelPathPool pathPool,
                                        HierarchicalPathfindingSystemCollection data) : base(config.GetDefaultBounds())
    {
        this.pathPool = pathPool;
        this.data = data;
        this.positionPool = new DefaultObjectPool<List<Position2D>>(new ListObjectPoolPolicy<Position2D>());
        this.targetsByZones = new Dictionary<GlobalTraversableZoneId, List<Position2D>>();
        this.movementCostsOnLevel = new List<MovementCostData2D>();
        this.openNodesHighLevel = new PriorityQueue<float, HighLevelNode>(4096);
        this.closedNodesHighLevel = new Dictionary<Position2D, HighLevelNode>();
        this.directionData = DirectionalityLookup.Get(AdjacencyRule.EightWay);
        this.movementCosts = new Dictionary<IMovementMode, MovementCost>();

        this.outboundDirectionsTile = new BufferList<IReadOnlyBoundedDataView<DirectionalityInformation>?>();
        this.inboundDirectionsTile = new BufferList<IReadOnlyBoundedDataView<DirectionalityInformation>?>();
        this.costsTile = new BufferList<IReadOnlyBoundedDataView<float>?>();
        this.nodesSources = new BoundedDataView<IMovementMode>(config.GetDefaultBounds());
    }

    void AddTarget(GlobalTraversableZoneId target, Position2D pos)
    {
        if (!targetsByZones.TryGetValue(target, out var positionsInZone))
        {
            positionsInZone = positionPool.Get();
            targetsByZones[target] = positionsInZone;
        }

        positionsInZone.Add(pos);
    }

    public void Clear()
    {
        this.movementCostsOnLevel.Clear();
        this.movementCosts.Clear();
        outboundDirectionsTile.Clear();
        inboundDirectionsTile.Clear();
        outboundDirectionsTile.Clear();
        foreach (var targetsByZone in this.targetsByZones)
        {
            positionPool.Return(targetsByZone.Value);
        }

        targetsByZones.Clear();
    }

    public bool Initialize(List<MovementCostData3D> mc,
                           GlobalTraversableZoneId origin,
                           Position2D originPos,
                           int z)
    {
        this.originZone = origin;
        this.originPos = originPos;
        this.z = z;
        Clear();

        foreach (var costData3D in mc)
        {
            movementCosts[costData3D.MovementCost.MovementMode] = costData3D.MovementCost;
            if (costData3D.TryGetMovementData2D(z, out var costs))
            {
                movementCostsOnLevel.Add(costs);
            }
        }

        if (!data.EdgeDataView.TryGetView(z, out edgeData2D))
        {
            return false;
        }

        outboundDirectionsTile.EnsureCapacity(movementCostsOnLevel.Count);
        inboundDirectionsTile.EnsureCapacity(movementCostsOnLevel.Count);
        outboundDirectionsTile.EnsureCapacity(movementCostsOnLevel.Count);
        outboundDirectionsTile.Clear();
        inboundDirectionsTile.Clear();
        outboundDirectionsTile.Clear();

        return true;
    }
    
    public bool AddTargets(IPathFinderTargetEvaluator targetEvaluator)
    {
        this.targetEvaluator = targetEvaluator;
        using var targetBuffer = BufferListPool<EntityGridPosition>.GetPooled();
        if (!data.ZoneDataView.TryGetRegionView2D(z, out var zoneView2D))
        {
            return false;
        }

        bool sameZone = false;
        IReadOnlyBoundedDataView<(TraversableZoneId zone, DirectionalityInformation zoneEdges)>? regionTile = null;
        foreach (var target in targetEvaluator.CollectTargets(targetBuffer))
        {
            if (target.GridZ != z)
            {
                continue;
            }

            var pos = target.ToGridXY();
            var targetZoneRaw = zoneView2D.TryGetMapValue(ref regionTile, pos.X, pos.Y, default);
            if (regionTile != null)
            {
                var targetZone = new GlobalTraversableZoneId(regionTile.Bounds.Position, targetZoneRaw.Item1);
                sameZone |= targetZone.Equals(originZone);
                AddTarget(targetZone, pos);
            }
        }

        return sameZone;
    }

}