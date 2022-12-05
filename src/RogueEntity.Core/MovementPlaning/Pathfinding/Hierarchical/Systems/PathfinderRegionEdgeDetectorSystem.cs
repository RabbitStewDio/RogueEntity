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

public class PathfinderRegionEdgeDetectorSystem
{
    static readonly ILogger logger = SLog.ForContext<PathfinderRegionEdgeDetectorSystem>();
    readonly PathfinderRegionView3D dataView;
    readonly PathfinderRegionEdgeData3D edgeDataView;
    readonly IMovementDataProvider dataProvider;
    readonly BufferList<EdgeJob> edgeDetectorCoreJobs;
    readonly BufferList<EdgeJob> edgeDetectorNeighbourJobs;
    readonly List<NeighbourEdgeData> neighbourJobParams;
    readonly BufferList<List<(long flag, MovementCostData2D data)>> jobMovementCosts;
    readonly ObjectPool<List<(long flag, MovementCostData2D data)>> movementCostDataPool;
    readonly MovementModeEncoding movementModeEncoding;
    readonly DefaultBoundedDataViewPool<DirectionalityInformation> edgeDetectorPagePool;
    readonly bool allowParallelExecution;

    public PathfinderRegionEdgeDetectorSystem(DynamicDataViewConfiguration config,
                                              IMovementDataProvider dataProvider,
                                              PathfinderRegionView3D dataView,
                                              PathfinderRegionEdgeData3D edgeDataView,
                                              MovementModeEncoding movementModeEncoding)
    {
        this.dataProvider = dataProvider;
        this.dataView = dataView;
        this.edgeDataView = edgeDataView;
        this.movementModeEncoding = movementModeEncoding;
        this.neighbourJobParams = CreateNeighbourJobParameters(config);
        this.edgeDetectorCoreJobs = new BufferList<EdgeJob>();
        this.edgeDetectorNeighbourJobs = new BufferList<EdgeJob>();
        this.jobMovementCosts = new BufferList<List<(long flag, MovementCostData2D data)>>();
        this.movementCostDataPool = new DefaultObjectPool<List<(long flag, MovementCostData2D data)>>(new ListObjectPoolPolicy<(long flag, MovementCostData2D data)>());
        this.edgeDetectorPagePool = new DefaultBoundedDataViewPool<DirectionalityInformation>(config);
        this.allowParallelExecution = true;
    }
    
    /// <summary>
    ///    Removes edge connections for removed zones and zones that are going to be recomputed.
    /// </summary>
    public void PrepareModifiedZones()
    {
        CollectZoneRemovedJobs();
        foreach (var job in edgeDetectorCoreJobs)
        {
            Prepare(job);
            if (job.regionData.State == RegionEdgeState.MarkedForRemove)
            {
                var pos = job.regionData.Bounds.Position;
                job.regionData2D.RemoveView(pos.X, pos.Y, out _);
                job.edgeData2D.RemoveView(pos.X, pos.Y);
            }
        }
    }
    
    public void RecomputeEdges()
    {
        CollectEdgeComputationJobs();

        if (allowParallelExecution)
        {
            Parallel.ForEach(edgeDetectorCoreJobs, DetectEdges);
            Parallel.ForEach(edgeDetectorNeighbourJobs, DetectEdges);
        }
        else
        {
            foreach (var job in edgeDetectorCoreJobs)
            {
                DetectEdges(job);
            }
            foreach (var job in edgeDetectorNeighbourJobs)
            {
                DetectEdges(job);
            }
        }

        foreach (var job in edgeDetectorCoreJobs)
        {
            PostProcess(job);
        }
        foreach (var job in edgeDetectorNeighbourJobs)
        {
            PostProcess(job);
        }

        foreach (var job in jobMovementCosts)
        {
            movementCostDataPool.Return(job);
        }
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

    void CollectZoneRemovedJobs()
    {
        edgeDetectorCoreJobs.Clear();
        
        using var activeLayers = BufferListPool<int>.GetPooled();
        foreach (var layer in dataView.GetActiveLayers(activeLayers))
        {
            if (!dataView.TryGetRegionView2D(layer, out var layerView))
            {
                continue;
            }
            
            if (!edgeDataView.TryGetView(layer, out var edgeView, DataViewCreateMode.CreateMissing))
            {
                continue;
            }

            var movementCosts = movementCostDataPool.Get();
            ProduceMovementData(layer, movementCosts);
            jobMovementCosts.Add(movementCosts);
            
            using var activeTiles = BufferListPool<Rectangle>.GetPooled();
            layerView.GetActiveTiles(activeTiles);
            foreach (var tileBounds in activeTiles.Data)
            {
                if (!layerView.TryGetRegion(tileBounds.X, tileBounds.Y, out var pathfinderRegion))
                {
                    continue;
                }
                
                if (!edgeView.TryGetView(tileBounds.Position, out var edgeRegion, DataViewCreateMode.CreateMissing))
                {
                    continue;
                }

                if (pathfinderRegion.State == RegionEdgeState.MarkedForRemove ||
                    pathfinderRegion.State == RegionEdgeState.Dirty)
                {
                    edgeDetectorCoreJobs.Add(new EdgeJob(layerView, pathfinderRegion, edgeView, edgeRegion, movementCosts));
                    MarkNeighboursAsDirty(layerView, pathfinderRegion.Bounds);
                }
            }
        }

        logger.Debug("Collected {Count} core and {Neighbours} jobs", edgeDetectorCoreJobs.Count, edgeDetectorNeighbourJobs.Count);
    }
    
    void CollectEdgeComputationJobs()
    {
        edgeDetectorCoreJobs.Clear();
        edgeDetectorNeighbourJobs.Clear();
        
        using var activeLayers = BufferListPool<int>.GetPooled();
        foreach (var layer in dataView.GetActiveLayers(activeLayers))
        {
            if (!dataView.TryGetRegionView2D(layer, out var zoneView))
            {
                continue;
            }

            if (!edgeDataView.TryGetView(layer, out var edgeView))
            {
                continue;
            }

            var movementCosts = movementCostDataPool.Get();
            ProduceMovementData(layer, movementCosts);
            jobMovementCosts.Add(movementCosts);

            using var activeTiles = BufferListPool<Rectangle>.GetPooled();
            zoneView.GetActiveTiles(activeTiles);
            foreach (var tileBounds in activeTiles.Data)
            {
                if (!zoneView.TryGetRegion(tileBounds.X, tileBounds.Y, out var zoneRegion))
                {
                    continue;
                }

                if (!edgeView.TryGetView(tileBounds.Position, out var edgeRegion))
                {
                    continue;
                }

                if (zoneRegion.State == RegionEdgeState.Dirty)
                {
                    edgeDetectorCoreJobs.Add(new EdgeJob(zoneView, zoneRegion, edgeView, edgeRegion, movementCosts));
                }
            }

            foreach (var tileBounds in activeTiles.Data)
            {
                if (!zoneView.TryGetRegion(tileBounds.X, tileBounds.Y, out var zoneRegion))
                {
                    continue;
                }

                if (!edgeView.TryGetView(tileBounds.Position, out var edgeRegion))
                {
                    continue;
                }

                if (zoneRegion.State != RegionEdgeState.Dirty &&
                    zoneRegion.State != RegionEdgeState.Clean)
                {
                    edgeDetectorNeighbourJobs.Add(new EdgeJob(zoneView, zoneRegion, edgeView, edgeRegion, movementCosts));
                }
            }
        }

        logger.Debug("Collected {Count} core and {Neighbours} jobs", edgeDetectorCoreJobs.Count, edgeDetectorNeighbourJobs.Count);
    }

    void Prepare(EdgeJob jobParams)
    {
        var job = new PathfinderRegionEdgeDetectorPreparationJob(jobParams.regionData, 
                                                                 jobParams.edgeData2D, 
                                                                 jobParams.edgeData);
        job.Process();
    }

    void DetectEdges(EdgeJob jobParam)
    {
        var directionPage = edgeDetectorPagePool.Lease(jobParam.regionData.Bounds, 0);
        var job = new PathfinderRegionEdgeDetector(directionPage, 
                                                   jobParam.movementData, 
                                                   jobParam.regionData2D, 
                                                   jobParam.regionData, 
                                                   jobParam.edgeData);

        using var movementStyles = BufferListPool<DistanceCalculation>.GetPooled();
        foreach (var m in edgeDataView.GetMovementStyles(movementStyles))
        {
            job.ReconnectEdges(m);
        }
    }
    
    void PostProcess(EdgeJob jobParams)
    {
        var job = new PathfinderRegionEdgeDetectorPostProcessor(jobParams.regionData, 
                                                                jobParams.edgeData2D, 
                                                                jobParams.edgeData);
        job.ReconnectZoneEdges();
    }

    List<NeighbourEdgeData> CreateNeighbourJobParameters(DynamicDataViewConfiguration config)
    {
        var retval = new List<NeighbourEdgeData>
        {
            new NeighbourEdgeData(-config.TileSizeX, -config.TileSizeY, RegionEdgeState.EdgeSouth | RegionEdgeState.EdgeEast),
            new NeighbourEdgeData(-config.TileSizeX, 0, RegionEdgeState.EdgeEast),
            new NeighbourEdgeData(-config.TileSizeX, +config.TileSizeY, RegionEdgeState.EdgeNorth | RegionEdgeState.EdgeEast),
            new NeighbourEdgeData(config.TileSizeX, -config.TileSizeY, RegionEdgeState.EdgeSouth | RegionEdgeState.EdgeWest),
            new NeighbourEdgeData(config.TileSizeX, 0, RegionEdgeState.EdgeWest),
            new NeighbourEdgeData(config.TileSizeX, +config.TileSizeY, RegionEdgeState.EdgeNorth | RegionEdgeState.EdgeWest),
            new NeighbourEdgeData(0, +config.TileSizeY, RegionEdgeState.EdgeNorth),
            new NeighbourEdgeData(0, -config.TileSizeY, RegionEdgeState.EdgeSouth)
        };
        return retval;
    }

    void MarkNeighboursAsDirty(PathfinderRegionView2D layerView, in Rectangle bounds)
    {
        foreach (var j in neighbourJobParams)
        {
            if (layerView.TryGetRegion(bounds.X + j.DeltaX, bounds.Y + j.DeltaY, out var r))
            {
                r.MarkDirty(j.State);
            }
        }
    }

    readonly struct EdgeJob
    {
        public readonly PathfinderRegionView2D regionData2D;
        public readonly PathfinderRegionDataView regionData;
        public readonly PathfinderRegionEdgeData2D edgeData2D;
        public readonly PathfinderRegionEdgeData edgeData;
        public readonly List<(long flag, MovementCostData2D data)> movementData;

        public EdgeJob(PathfinderRegionView2D regionData2D, PathfinderRegionDataView regionData,
                       PathfinderRegionEdgeData2D edgeData2D, PathfinderRegionEdgeData edgeData, List<(long flag, MovementCostData2D data)> movementData)
        {
            this.regionData2D = regionData2D;
            this.regionData = regionData;
            this.edgeData2D = edgeData2D;
            this.edgeData = edgeData;
            this.movementData = movementData;
        }
    }

    readonly struct NeighbourEdgeData
    {
        public readonly int DeltaX;
        public readonly int DeltaY;
        public readonly RegionEdgeState State;

        public NeighbourEdgeData(int deltaX, int deltaY, RegionEdgeState state)
        {
            this.DeltaX = deltaX;
            this.DeltaY = deltaY;
            this.State = state;
        }
    }

}