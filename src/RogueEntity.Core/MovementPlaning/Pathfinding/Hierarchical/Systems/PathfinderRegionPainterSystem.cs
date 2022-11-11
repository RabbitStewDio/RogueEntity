using Microsoft.Extensions.ObjectPool;
using RogueEntity.Api.Utils;
using RogueEntity.Core.GridProcessing.Directionality;
using RogueEntity.Core.Movement;
using RogueEntity.Core.Movement.CostModifier;
using RogueEntity.Core.MovementPlaning.Pathfinding.Hierarchical.Data;
using RogueEntity.Core.Positioning.Algorithms;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.DataViews;
using Serilog;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RogueEntity.Core.MovementPlaning.Pathfinding.Hierarchical.Systems;

public class PathfinderRegionPainterSystem
{
    static readonly ILogger logger = SLog.ForContext<PathfinderRegionPainterSystem>();
    readonly BufferList<PathfinderRegionPainterJob> regionPainterJobs;
    readonly BufferList<List<(long flag, MovementCostData2D data)>> jobMovementCosts;
    readonly ObjectPool<List<(long flag, MovementCostData2D data)>> movementCostDataPool;
    readonly PathfinderRegionPainterSharedDataFactory painterSharedDataFactory;
    readonly PathfinderRegionView3D dataView;
    readonly DynamicDataViewConfiguration config;
    readonly IMovementDataProvider dataProvider;
    readonly MovementModeEncoding movementModeEncoding;
    readonly bool allowParallelExecution;
    DistanceCalculation movementStyle;

    public PathfinderRegionPainterSystem(DynamicDataViewConfiguration config,
                                         IMovementDataProvider dataProvider,
                                         PathfinderRegionView3D dataView,
                                         MovementModeEncoding movementModeEncoding)
    {
        this.config = config;
        this.dataProvider = dataProvider;
        this.dataView = dataView;
        this.movementModeEncoding = movementModeEncoding;
        this.regionPainterJobs = new BufferList<PathfinderRegionPainterJob>();
        this.movementCostDataPool = new DefaultObjectPool<List<(long flag, MovementCostData2D data)>>(new ListObjectPoolPolicy<(long flag, MovementCostData2D data)>());
        this.painterSharedDataFactory = new PathfinderRegionPainterSharedDataFactory(config);
        this.jobMovementCosts = new BufferList<List<(long flag, MovementCostData2D data)>>();
        this.allowParallelExecution = true;
    }

    public void Initialize(DistanceCalculation c)
    {
        movementStyle = c;
    }
    
    public void RecomputePathfinderZone()
    {
        CollectDirtyRegions(movementStyle);
        if (allowParallelExecution)
        {
            Parallel.ForEach(regionPainterJobs, UpdateMovementZones);
        }
        else
        {
            foreach (var j in regionPainterJobs)
            {
                UpdateMovementZones(j);
            }
        }

        foreach (var job in jobMovementCosts)
        {
            movementCostDataPool.Return(job);
        }

        regionPainterJobs.Clear();
    }

    /// <summary>
    ///   Single threaded collector. 
    /// </summary>
    void CollectDirtyRegions(DistanceCalculation movementStyle)
    {
        regionPainterJobs.Clear();

        using var activeLayers = BufferListPool<int>.GetPooled();
        foreach (var layer in dataView.GetActiveLayers(activeLayers))
        {
            if (!dataView.TryGetWritableView(layer, out var layerView, DataViewCreateMode.CreateMissing))
            {
                continue;
            }

            var movementCosts = movementCostDataPool.Get();
            ProduceMovementData(layer, movementCosts);
            jobMovementCosts.Add(movementCosts);

            using var activeTiles = BufferListPool<Rectangle>.GetPooled();
            foreach (var tileBounds in layerView.GetActiveTiles(activeTiles))
            {
                if (!layerView.TryGetWriteAccess(tileBounds.X, tileBounds.Y, out var region) ||
                    region is not PathfinderRegionDataView pathfinderRegion)
                {
                    continue;
                }

                if (pathfinderRegion.State == RegionEdgeState.Dirty)
                {
                    pathfinderRegion.ClearData();
                    regionPainterJobs.Add(new PathfinderRegionPainterJob(painterSharedDataFactory,
                                                                         movementStyle, pathfinderRegion, layer, movementCosts));
                }
            }
        }

        logger.Debug("Collected {Count} regions", regionPainterJobs.Count);
    }

    void ProduceMovementData(int layer, List<(long flag, MovementCostData2D data)> movementCosts)
    {
        for (var index = 0; index < movementModeEncoding.ModeList.Count; index++)
        {
            var mode = movementModeEncoding.ModeList[index];
            if (!dataProvider.MovementCosts.TryGetValue(mode, out var m))
            {
                continue;
            }

            if (!m.Costs.TryGetView(layer, out var costs))
            {
                continue;
            }

            if (!m.InboundDirections.TryGetView(layer, out var inboundDirections))
            {
                continue;
            }

            if (!m.OutboundDirections.TryGetView(layer, out var outboundDirections))
            {
                continue;
            }

            var modeId = 1L << index;
            movementCosts.Add((modeId, new MovementCostData2D(m.MovementMode, costs, inboundDirections, outboundDirections)));
        }
    }

    void UpdateMovementZones(PathfinderRegionPainterJob p)
    {
        p.Process();
    }


}