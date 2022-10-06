using EnTTSharp;
using Microsoft.Extensions.ObjectPool;
using RogueEntity.Api.Utils;
using RogueEntity.Core.GridProcessing.Directionality;
using RogueEntity.Core.Movement;
using RogueEntity.Core.Positioning.Algorithms;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.DataViews;
using Serilog;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MovementCostData2D = RogueEntity.Core.Movement.CostModifier.MovementCostData2D;

namespace RogueEntity.Core.MovementPlaning.Pathfinding.Hierarchical
{
    public enum EdgeDirection { North, NorthEast, East, SouthEast, South, SouthWest, West, NorthWest }

    /// <summary>
    ///    Pre-Calculates pathfinding data. This computation operates on pathfinder regions. Each region corresponds to
    ///    a bounded data view. Each pathfinding system instance calculates data for a specific movement mode. Each
    ///    region is subdivided into traversable zones. All cells of a given zone share the same movement profile - that is
    ///    each cell inside the zone allows exactly the same movement mode. Any change in available movement modes results
    ///    in a transition to a new movement zone.
    /// </summary>
    /// <typeparam name="TMovementMode"></typeparam>
    public class HierarchicalPathfindingSystem<TMovementMode> where TMovementMode : IMovementMode
    {
        static readonly ILogger logger = SLog.ForContext<HierarchicalPathfindingSystem<TMovementMode>>();
        readonly DynamicDataViewConfiguration config;
        readonly IMovementDataProvider dataProvider;
        readonly DistanceCalculation movementType;
        readonly PooledDynamicDataView3D<(TraversableZoneId, DirectionalityInformation)> dataView;
        readonly BufferList<int> activeLayers;
        readonly BufferList<Rectangle> activeTiles;
        readonly BufferList<(int z, Position2D pos, EdgeDirection direction)> dirtyNeighbours;
        readonly BufferList<PathfinderRegionPainter> regionPainterJobs;
        readonly DefaultObjectPool<List<MovementCostData2D>> movementCostDataPool;
        readonly PathfinderRegionPainterSharedDataFactory painterSharedDataFactory;
        DataView3DBinding<float, (TraversableZoneId, DirectionalityInformation)> sourceBinding;
        Optional<MovementSourceData> movementCostData;

        public HierarchicalPathfindingSystem(DynamicDataViewConfiguration config,
                                             IMovementDataProvider dataProvider,
                                             DistanceCalculation movementType)
        {
            this.config = config;
            if (config.TileSizeX * config.TileSizeY >= ushort.MaxValue)
            {
                throw new ArgumentException("Hierarchical pathfinding regions must be less than 64k cells");
            }

            this.dataProvider = dataProvider ?? throw new ArgumentNullException(nameof(dataProvider));
            this.movementType = movementType;
            this.movementCostDataPool = new DefaultObjectPool<List<MovementCostData2D>>(new ListObjectPoolPolicy<MovementCostData2D>(), 255);
            this.painterSharedDataFactory = new PathfinderRegionPainterSharedDataFactory(config);

            dirtyNeighbours = new BufferList<(int z, Position2D pos, EdgeDirection direction)>();
            regionPainterJobs = new BufferList<PathfinderRegionPainter>();
            activeLayers = new BufferList<int>();
            activeTiles = new BufferList<Rectangle>();
            dataView = new PooledDynamicDataView3D<(TraversableZoneId, DirectionalityInformation)>(new PathfinderRegionDataViewPool(config));
            movementCostData = default;
        }

        public IDynamicDataView3D<(TraversableZoneId, DirectionalityInformation)> ZoneInformation => dataView;

        public void Initialize()
        {
            if (!dataProvider.TryGet<TMovementMode>(out var movementCostDataRaw))
            {
                throw new InvalidOperationException();
            }

            movementCostData = movementCostDataRaw;
            this.sourceBinding = new DataView3DBinding<float, (TraversableZoneId, DirectionalityInformation)>(movementCostDataRaw.Costs, dataView);
        }

        /// <summary>
        ///   Single threaded collector. 
        /// </summary>
        public void CollectDirtyRegions()
        {
            if (!movementCostData.TryGetValue(out var movementData))
            {
                logger.Warning("No movement cost data available - has Initialize() been called?");
                return;
            }

            dirtyNeighbours.Clear();
            regionPainterJobs.Clear();

            dataView.GetActiveLayers(activeLayers);
            foreach (var layer in activeLayers)
            {
                if (!movementData.Costs.TryGetView(layer, out var costForLayer) ||
                    !movementData.InboundDirections.TryGetView(layer, out var inboundDirectionsForLayer) ||
                    !movementData.OutboundDirections.TryGetView(layer, out var outboundDirectionsForLayer))
                {
                    continue;
                }

                if (!dataView.TryGetWritableView(layer, out var layerView, DataViewCreateMode.CreateMissing))
                {
                    continue;
                }

                var movementDataForLayer = new MovementCostData2D(movementData.MovementMode, costForLayer, inboundDirectionsForLayer, outboundDirectionsForLayer);

                var movementCosts = movementCostDataPool.Get();
                foreach (var m in dataProvider.MovementCosts.Values)
                {
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

                    movementCosts.Add(new MovementCostData2D(m.MovementMode, costs, inboundDirections, outboundDirections));
                }

                layerView.GetActiveTiles(activeTiles);
                foreach (var tileBounds in activeTiles)
                {
                    if (!layerView.TryGetWriteAccess(tileBounds.X, tileBounds.Y, out var region) ||
                        region is not PathfinderRegionDataView pathfinderRegion)
                    {
                        continue;
                    }

                    if (pathfinderRegion.IsDirty)
                    {
                        pathfinderRegion.Clear();
                        dirtyNeighbours.Add((layer, new Position2D(tileBounds.X - config.TileSizeX, tileBounds.Y - config.TileSizeY), EdgeDirection.NorthWest));
                        dirtyNeighbours.Add((layer, new Position2D(tileBounds.X, tileBounds.Y - config.TileSizeY), EdgeDirection.North));
                        dirtyNeighbours.Add((layer, new Position2D(tileBounds.X + config.TileSizeX, tileBounds.Y - config.TileSizeY), EdgeDirection.NorthEast));
                        dirtyNeighbours.Add((layer, new Position2D(tileBounds.X + config.TileSizeX, tileBounds.Y), EdgeDirection.East));
                        dirtyNeighbours.Add((layer, new Position2D(tileBounds.X + config.TileSizeX, tileBounds.Y + config.TileSizeY), EdgeDirection.SouthEast));
                        dirtyNeighbours.Add((layer, new Position2D(tileBounds.X, tileBounds.Y + config.TileSizeY), EdgeDirection.South));
                        dirtyNeighbours.Add((layer, new Position2D(tileBounds.X - config.TileSizeX, tileBounds.Y + config.TileSizeY), EdgeDirection.SouthWest));
                        dirtyNeighbours.Add((layer, new Position2D(tileBounds.X - config.TileSizeX, tileBounds.Y), EdgeDirection.West));
                        regionPainterJobs.Add(new PathfinderRegionPainter(painterSharedDataFactory, movementType, pathfinderRegion, layer, movementDataForLayer, movementCosts));
                    }
                }
            }

            logger.Debug("Collected {Count} regions", regionPainterJobs.Count);
        }

        public void PaintDirtyPathfinderRegions()
        {
            // todo Make delegate cached
            Parallel.ForEach(regionPainterJobs, PaintDirtyRegion);
            Parallel.ForEach(regionPainterJobs, ReconnectRegions);
            Parallel.ForEach(dirtyNeighbours, ReconnectNeighbourEdge);

            foreach (var job in regionPainterJobs)
            {
                movementCostDataPool.Return(job.MovementCostsOnLevel);
            }
        }

        void ReconnectNeighbourEdge((int z, Position2D pos, EdgeDirection direction) obj)
        {
            if (!dataView.TryGetView(obj.z, out var v1))
            {
                return;
            }

            if (!v1.TryGetData(obj.pos.X, obj.pos.Y, out var regionRaw) || regionRaw is not PathfinderRegionDataView region)
            {
                return;
            }

            ReconnectEdge(region, obj.z, obj.direction);
        }

        void ReconnectRegions(PathfinderRegionPainter p)
        {
            ReconnectEdge(p.Region, p.Z, EdgeDirection.North);
            ReconnectEdge(p.Region, p.Z, EdgeDirection.NorthEast);
            ReconnectEdge(p.Region, p.Z, EdgeDirection.East);
            ReconnectEdge(p.Region, p.Z, EdgeDirection.SouthEast);
            ReconnectEdge(p.Region, p.Z, EdgeDirection.South);
            ReconnectEdge(p.Region, p.Z, EdgeDirection.SouthWest);
            ReconnectEdge(p.Region, p.Z, EdgeDirection.West);
            ReconnectEdge(p.Region, p.Z, EdgeDirection.NorthWest);
        }

        void PaintDirtyRegion(PathfinderRegionPainter p)
        {
            p.Process();
        }

        void ReconnectEdge(PathfinderRegionDataView regionDataView, int z, EdgeDirection direction)
        {
        }

        class CostSourceBinding : DataView3DBinding<float, (TraversableZoneId, DirectionalityInformation)>
        {
            public CostSourceBinding(IReadOnlyDynamicDataView3D<float> sourceView, 
                                     IDynamicDataView3D<(TraversableZoneId, DirectionalityInformation)> targetView) : base(sourceView, targetView)
            {
            }

            protected override void OnSourceViewProcessed(int zInfo, IBoundedDataView<(TraversableZoneId, DirectionalityInformation)> tile)
            {
                if (tile is PathfinderRegionDataView r)
                {
                    r.IsDirty = true;
                }
            }
        }
    }
}