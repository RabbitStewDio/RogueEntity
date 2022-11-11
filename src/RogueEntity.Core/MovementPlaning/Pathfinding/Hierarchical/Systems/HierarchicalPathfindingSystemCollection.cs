using RogueEntity.Core.GridProcessing.Directionality;
using RogueEntity.Core.Movement;
using RogueEntity.Core.MovementPlaning.Pathfinding.Hierarchical.Data;
using RogueEntity.Core.MovementPlaning.Pathfinding.SingleLevel;
using RogueEntity.Core.Positioning.Algorithms;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.DataViews;
using System;
using System.Collections.Generic;

namespace RogueEntity.Core.MovementPlaning.Pathfinding.Hierarchical.Systems;

public class HierarchicalPathfindingSystemCollection: IDisposable
{
    readonly IMovementDataProvider dataProvider;
    readonly PathfinderRegionView3D dataView;
    readonly PathfinderRegionEdgeData3D edgeDataView;
    
    readonly MovementModeEncoding movementModeEncoding;
    readonly List<CostSourceBinding> dataBindings;

    readonly PathfinderRegionPainterSystem regionPainter;
    readonly PathfinderRegionEdgeDetectorSystem edgeDetector;
    readonly PathfinderRegionEdgeCostCalculatorSystem costCalculator;
    DistanceCalculation zonePainterMovementStyle;

    public HierarchicalPathfindingSystemCollection(DynamicDataViewConfiguration config,
                                                   IMovementDataProvider dataProvider,
                                                   SingleLevelPathFinderSource pathfinderSource)
    {
        this.Config = config;
        this.dataProvider = dataProvider;
        movementModeEncoding = new MovementModeEncoding();
        dataView = new PathfinderRegionView3D(new PathfinderRegionDataViewPool(config));
        edgeDataView = new PathfinderRegionEdgeData3D(movementModeEncoding);
        zonePainterMovementStyle = DistanceCalculation.Euclid;
        regionPainter = new PathfinderRegionPainterSystem(config, dataProvider, dataView, movementModeEncoding);
        edgeDetector = new PathfinderRegionEdgeDetectorSystem(config, dataProvider, dataView, edgeDataView, movementModeEncoding);
        dataBindings = new List<CostSourceBinding>();
        costCalculator = new PathfinderRegionEdgeCostCalculatorSystem(edgeDataView, dataProvider, pathfinderSource);
    }

    public DynamicDataViewConfiguration Config { get; }

    public PathfinderRegionView3D ZoneDataView => dataView;

    public PathfinderRegionEdgeData3D EdgeDataView => edgeDataView;

    public void RegisterMovementCombination(IMovementMode mode, DistanceCalculation c)
    {
        zonePainterMovementStyle = RestrictMovement(c);
        edgeDataView.RegisterMovement(c, mode);
    }

    public void Initialize()
    {
        foreach (var dataBinding in this.dataBindings)
        {
            dataBinding.Dispose();
        }

        dataBindings.Clear();

        foreach (var m in dataProvider.MovementCosts)
        {
            movementModeEncoding.Register(m.Key);
            dataBindings.Add(new CostSourceBinding(m.Value.Costs, dataView));
        }
        
        regionPainter.Initialize(zonePainterMovementStyle);
    }

    DistanceCalculation RestrictMovement(DistanceCalculation c) => zonePainterMovementStyle switch
    {
        DistanceCalculation.Euclid => c,
        DistanceCalculation.Manhattan => DistanceCalculation.Manhattan,
        DistanceCalculation.Chebyshev => c switch
        {
            DistanceCalculation.Euclid => DistanceCalculation.Chebyshev,
            DistanceCalculation.Manhattan => DistanceCalculation.Manhattan,
            DistanceCalculation.Chebyshev => DistanceCalculation.Chebyshev,
            _ => throw new ArgumentOutOfRangeException(nameof(c), c, null)
        },
        _ => throw new ArgumentOutOfRangeException()
    };

    public void Dispose()
    {
        dataView.Clear();
        edgeDataView.Clear();
        foreach (var b in dataBindings)
        {
            b.Dispose();
        }
        dataBindings.Clear();
    }

    public void Process()
    {
        edgeDetector.PrepareModifiedZones();
        regionPainter.RecomputePathfinderZone();
        edgeDetector.RecomputeEdges();

        using var modeCombinations = BufferListPool<(DistanceCalculation, IMovementMode)>.GetPooled();
        foreach (var (style, mode) in edgeDataView.GetMovements(modeCombinations))
        {
            costCalculator.CalculateInternalPaths(mode, style);
        }
    }

    class CostSourceBinding : DataView3DBinding<float, (TraversableZoneId, DirectionalityInformation)>
    {
        readonly PathfinderRegionView3D targetView;

        public CostSourceBinding(IReadOnlyDynamicDataView3D<float> sourceView,
                                 PathfinderRegionView3D targetView) : base(sourceView, targetView, true)
        {
            this.targetView = targetView ?? throw new ArgumentNullException(nameof(targetView));
            Init();
        }

        protected override void OnSourceViewProcessed(int zInfo, IBoundedDataView<(TraversableZoneId, DirectionalityInformation)> tile)
        {
            if (tile is PathfinderRegionDataView r)
            {
                r.State = RegionEdgeState.Dirty;
            }
        }

        protected override void OnChunkCreated(int z, IReadOnlyBoundedDataView<float> tileData)
        {
            if (!targetView.TryGetRegionView2D(z, out var region2D, DataViewCreateMode.CreateMissing))
            {
                return;
            }

            var pos = tileData.Bounds.Position;
            if (region2D.TryGetRegion(pos.X, pos.Y, out var tile, DataViewCreateMode.CreateMissing))
            {
                tile.State = RegionEdgeState.Dirty;
            }
        }

        protected override void OnChunkExpired(int z, IReadOnlyBoundedDataView<float> tileData)
        {
            if (!targetView.TryGetRegionView2D(z, out var region2D))
            {
                return;
            }

            var pos = tileData.Bounds.Position;
            if (region2D.TryGetRegion(pos.X, pos.Y, out var tile))
            {
                tile.State = RegionEdgeState.MarkedForRemove;
            }
        }
    }
}

