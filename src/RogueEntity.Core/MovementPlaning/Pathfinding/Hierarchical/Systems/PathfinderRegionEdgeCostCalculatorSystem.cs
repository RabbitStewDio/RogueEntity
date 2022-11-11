using RogueEntity.Api.Utils;
using RogueEntity.Core.Movement;
using RogueEntity.Core.MovementPlaning.Pathfinding.Hierarchical.Data;
using RogueEntity.Core.MovementPlaning.Pathfinding.SingleLevel;
using RogueEntity.Core.Positioning.Algorithms;
using RogueEntity.Core.Utils;
using System.Threading.Tasks;

namespace RogueEntity.Core.MovementPlaning.Pathfinding.Hierarchical.Systems;

public class PathfinderRegionEdgeCostCalculatorSystem
{
    readonly struct PathConnectorJob
    {
        public readonly PathfinderRegionEdgeData Tile;
        public readonly int ZLayer;
        public readonly MovementSourceData MovementData;
        public readonly DistanceCalculation MovementStyle;

        public PathConnectorJob(PathfinderRegionEdgeData tile, int zLayer, MovementSourceData movementData, DistanceCalculation movementStyle)
        {
            this.Tile = tile;
            this.ZLayer = zLayer;
            this.MovementData = movementData;
            this.MovementStyle = movementStyle;
        }
    }
    
    readonly BufferList<PathConnectorJob> regionPathfindingJobs;
    readonly PathfinderRegionEdgeData3D edgeDataView;
    readonly IMovementDataProvider dataProvider;
    readonly SingleLevelPathFinderSource pathfinderSource;

    public PathfinderRegionEdgeCostCalculatorSystem(PathfinderRegionEdgeData3D edgeDataView, 
                                                    IMovementDataProvider dataProvider, 
                                                    SingleLevelPathFinderSource pathfinderSource)
    {
        this.edgeDataView = edgeDataView;
        this.dataProvider = dataProvider;
        this.pathfinderSource = pathfinderSource;
        this.regionPathfindingJobs = new BufferList<PathConnectorJob>();
    }

    public void CalculateInternalPaths(IMovementMode mode, DistanceCalculation movementStyle)
    {
        if (!dataProvider.MovementCosts.TryGetValue(mode, out var cost))
        {
            return;
        }
        
        regionPathfindingJobs.Clear();

        using var activeLayers = BufferListPool<int>.GetPooled();
        using var activeTiles = BufferListPool<PathfinderRegionEdgeData>.GetPooled();
        foreach (var z in edgeDataView.GetActiveLayers(activeLayers))
        {
            if (!edgeDataView.TryGetView(z, out var edgeData2D))
            {
                continue;
            }

            foreach (var t in edgeData2D.GetActiveTiles(activeTiles))
            {
                if (t.State == PathfinderRegionEdgeDataState.Clean)
                {
                    continue;
                }

                regionPathfindingJobs.Add(new PathConnectorJob(t, z, cost, movementStyle));
            }
        }

        Parallel.ForEach(regionPathfindingJobs, CalculateInternalPaths);

        regionPathfindingJobs.Clear();
    }

    void CalculateInternalPaths(PathConnectorJob job)
    {
        using var pathfinderBuilder = this.pathfinderSource.GetPathFinder();
        var edgeFinder = new PathfinderRegionEdgeCostCalculatorJob(job.Tile, pathfinderBuilder.Data, job.ZLayer, job.MovementData, job.MovementStyle);
        edgeFinder.Process();
    }
}