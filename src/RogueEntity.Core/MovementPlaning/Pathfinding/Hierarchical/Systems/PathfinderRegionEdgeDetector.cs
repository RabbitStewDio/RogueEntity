using EnTTSharp;
using Microsoft.Extensions.ObjectPool;
using RogueEntity.Api.Utils;
using RogueEntity.Core.GridProcessing.Directionality;
using RogueEntity.Core.Movement.CostModifier;
using RogueEntity.Core.MovementPlaning.Pathfinding.Hierarchical.Data;
using RogueEntity.Core.Positioning.Algorithms;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.DataViews;
using Serilog;
using System;
using System.Collections.Generic;

namespace RogueEntity.Core.MovementPlaning.Pathfinding.Hierarchical.Systems
{
    public struct PathfinderRegionEdgeDetector
    {
        static readonly ILogger logger = SLog.ForContext<PathfinderRegionEdgeDetector>();
        static readonly ObjectPool<HashSet<TraversableZoneId>> dirtyZoneSetPool = new DefaultObjectPool<HashSet<TraversableZoneId>>(new DefaultPooledObjectPolicy<HashSet<TraversableZoneId>>());

        readonly List<(long flag, MovementCostData2D data)> movementData;
        DistanceCalculation movementKind;
        readonly PathfinderRegionView2D zoneData2D;
        readonly PathfinderRegionDataView zoneData;
        readonly PathfinderRegionEdgeData edgeData;
        readonly IPooledBoundedDataView<DirectionalityInformation> visitedNodes;
        int directionStep;
        IReadOnlyBoundedDataView<(TraversableZoneId zone, DirectionalityInformation zoneEdges)>? zoneTile;
        IReadOnlyBoundedDataView<DirectionalityInformation>? outboundDirectionTile;
        IReadOnlyBoundedDataView<DirectionalityInformation>? inboundDirectionTile;

        public PathfinderRegionEdgeDetector(IPooledBoundedDataView<DirectionalityInformation> visitedNodes,
                                            List<(long flag, MovementCostData2D data)> movementData,
                                            PathfinderRegionView2D zoneData2D,
                                            PathfinderRegionDataView zoneData,
                                            PathfinderRegionEdgeData edgeData)
        {
            this.visitedNodes = visitedNodes;
            this.movementData = movementData;
            this.zoneData = zoneData;
            this.edgeData = edgeData;
            this.zoneData2D = zoneData2D;
            this.zoneTile = null;
            this.outboundDirectionTile = null;
            this.inboundDirectionTile = null;
            this.movementKind = DistanceCalculation.Euclid;
            this.directionStep = 1;
        }

        void ConnectPartialArea(in Rectangle b, HashSet<TraversableZoneId> dirtyZones)
        {
            foreach (var p in b.Contents)
            {
                var r = zoneData[p.X, p.Y];
                if (!dirtyZones.Contains(r.zone))
                {
                    continue;
                }

                if (r.zoneEdges == DirectionalityInformation.None)
                {
                    continue;
                }

                if (!visitedNodes.TryGet(p.X, p.Y, out var v))
                {
                    continue;
                }

                // If all incoming zone edges have been processed, consider the cell closed.
                if ((v & r.zoneEdges) == r.zoneEdges)
                {
                    continue;
                }

                ReconnectEdgesAt(p);
            }
        }

        void MarkVisited(GridPosition2D pos, Direction d)
        {
            var defaultValue = DirectionalityInformation.None;
            ref var visitedStateRef = ref visitedNodes.TryGetForUpdate(pos.X, pos.Y, ref defaultValue, out _);
            visitedStateRef = visitedStateRef.With(d);
        }

        bool IsVisited(GridPosition2D pos, Direction d)
        {
            var defaultValue = DirectionalityInformation.All;
            ref var visitedStateRef = ref visitedNodes.TryGetForUpdate(pos.X, pos.Y, ref defaultValue, out _);
            return visitedStateRef.IsMovementAllowed(d);
        }

        void ReconnectEdgesAt(GridPosition2D origin)
        {
            var start = zoneData[origin];
            if (start.zone == TraversableZoneId.Empty)
            {
                return;
            }

            if (!visitedNodes.TryGet(origin.X, origin.Y, out var visitedState))
            {
                logger.Debug("Failed to get visited info");
                return;
            }

            if (!TryFindFirstClockWise(start.zoneEdges, Direction.Left, visitedState, out var startDir))
            {
                logger.Debug("Skipping edges at {Origin}", origin);
                return;
            }

            if (startDir == Direction.None)
            {
                logger.Debug("Recording isolated cell");
                RecordIsolatedCell(start.zone, origin);
                visitedNodes.TrySet(origin.X, origin.Y, DirectionalityInformation.All);
                return;
            }

            if (visitedState.IsMovementAllowed(startDir.Inverse()))
            {
                return;
            }

            logger.Debug("Processing edges at {Origin} with direction {Direction}", origin, startDir);
            logger.Verbose("  (Previous {Origin} with direction {Direction})", origin, visitedState);
            var firstStep = new TraversalStep(origin, startDir);
            var nextStep = new TraversalStep(origin, startDir);
            int i = 0;
            do
            {
                i += 1;
                if (i > 10_000)
                {
                    throw new Exception("Unable to finish iteration");
                }
                nextStep = Process(nextStep);
                logger.Verbose("    {NextPosition} {NextDirection} {CurrentEdge}", nextStep.Origin, nextStep.Direction, nextStep.CurrentEdge);
            } while (nextStep != firstStep);

            if (nextStep.CurrentEdge.TryGetValue(out var value))
            {
                var (center, dir) = value.positions[value.positions.Count / 2];
                var edge = value.edge.WithSourcePosition(center, dir);
                logger.Debug("  Adding Edge {Edge}", edge);
                edgeData.AddOutboundEdge(movementKind, edge);
                BufferListPool<(GridPosition2D, Direction)>.Return(value.positions);
            }
        }

        bool IsValidMovementTarget(GridPosition2D origin, Direction d)
        {
            var targetCell = origin + d;
            var targetDirection = d.Inverse();
            var movementFlags = 0L;
            foreach (var (f, m) in movementData)
            {
                var inboundMovement = m.InboundDirections.TryGetMapValue(ref inboundDirectionTile, targetCell.X, targetCell.Y, DirectionalityInformation.None);
                if (inboundMovement.IsMovementAllowed(targetDirection))
                {
                    movementFlags |= f;
                }
            }

            return movementFlags != 0;
        }

        /// <summary>
        ///    Processes a single cell at a region's edge. 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        TraversalStep Process(TraversalStep input)
        {
            var here = true;

            var cellPos = input.Origin + input.Direction;
            var (cellZone, cellEdges) = zoneData[cellPos];
            var cellGlobalZoneId = new GlobalTraversableZoneId(zoneData.Bounds.Position, cellZone);

            // a direction pointing towards the previous step. This is the starting direction for the search.
            var fromDir = input.Direction.Inverse();
            var edge = input.CurrentEdge;

            // search all other direction except for the direction that we came from
            for (int dx = 1; dx < 8; dx += directionStep)
            {
                var testDirection = fromDir.MoveClockwise(dx);
                var visited = IsVisited(cellPos, testDirection);
                MarkVisited(cellPos, testDirection);

                if (!IsValidMovementTarget(cellPos, testDirection))
                {
                    // A tested direction is blocked. This means if there is a currently
                    // open edge, we have to close out that edge.
                    if (dx != 1 && edge.TryGetValue(out var existingEdge))
                    {
                        logger.Debug("       --> Closing Edge: No movement allowed for {TestDirection}", testDirection);
                        var (center, dir) = existingEdge.positions[existingEdge.positions.Count / 2];
                        this.edgeData.AddOutboundEdge(movementKind, existingEdge.edge.WithSourcePosition(center, dir));
                        BufferListPool<(GridPosition2D, Direction)>.Return(existingEdge.positions);
                        edge = default;
                        continue;
                    }

                    if (here)
                    {
                        logger.Debug("       No movement allowed for {TestDirection}", testDirection);
                    }

                    continue;
                }

                // is the target cell is part of the currently processed region? 
                var testPosition = cellPos + testDirection;
                var data = QueryZoneData(testPosition);
                if (data == cellGlobalZoneId)
                {
                    // Is this part of the region's edge? If so, move to the next cell.  
                    if (cellEdges.IsMovementAllowed(testDirection))
                    {
                        if (here)
                        {
                            logger.Debug("       Edge traversal for {TestDirection}", testDirection);
                        }

                        if (dx == 1)
                        {
                            Console.WriteLine("HERE");
                        }

                        return new TraversalStep(cellPos, testDirection, edge);
                    }

                    // For some reason this is an inner cell. Close out any possibly processed
                    // edge, then move on.
                    if (edge.TryGetValue(out var e))
                    {
                        var (center, dir) = e.positions[e.positions.Count / 2];
                        edgeData.AddOutboundEdge(movementKind, e.edge.WithSourcePosition(center, dir));
                        BufferListPool<(GridPosition2D, Direction)>.Return(e.positions);
                        edge = default;
                    }

                    if (here)
                    {
                        logger.Debug("       Same zone (cont) for {TestDirection}", testDirection);
                    }

                    continue;
                }

                if (dx == 1)
                {
                    // skip the first diagonal test. The cell that we would test has already
                    // been inspected by the previous cell.
                    continue;
                }

                // the tested neighbour cell is a different zone, and is traversable.
                // check if its still the same edge. If not, we have to close out the current
                // edge.
                if (edge.TryGetValue(out var currentEdgeData))
                {
                    // Is this a continuation of the current edge?
                    if (currentEdgeData.edge.TargetZone == data)
                    {
                        var tp = (cellPos, testDirection);
                        if (currentEdgeData.positions[^1] != tp)
                        {
                            currentEdgeData.positions.Add(tp);
                        }

                        if (here)
                        {
                            logger.Debug("       Continued edge for {TestDirection}", testDirection);
                        }

                        continue;
                    }

                    if (here)
                    {
                        logger.Debug("       --> Closing edge for {TestDirection}", testDirection);
                    }

                    // must be a new edge; so close out the currently processed edge.
                    var (center, dir) = currentEdgeData.positions[currentEdgeData.positions.Count / 2];
                    edgeData.AddOutboundEdge(movementKind, currentEdgeData.edge.WithSourcePosition(center, dir));
                    BufferListPool<(GridPosition2D, Direction)>.Return(currentEdgeData.positions);
                }


                if (visited)
                {
                    // prevent duplicates.
                    edge = Optional.Empty();
                }
                else
                {
                    var positionAccumulator = BufferListPool<(GridPosition2D, Direction)>.Get();
                    positionAccumulator.Add((cellPos, testDirection));
                    edge = (new PathfinderRegionEdge(cellGlobalZoneId, zoneData.GenerateEdgeId(), testPosition, Direction.None, data), positionAccumulator);
                    if (here)
                    {
                        logger.Debug("       --> Opening edge {Edge} for {TestDirection}", testDirection, edge.Value.edge.LocalEdgeId);
                    }
                }
            }

            // dead end; revert back., edge
            return new TraversalStep(cellPos, fromDir, edge);
        }


        /// <summary>
        ///    Processes an isolated single cell region. Scans the traversal-zone neighbours for walkable connections;
        ///    merges connected zones. 
        /// </summary>
        /// <param name="self"></param>
        /// <param name="origin"></param>
        void RecordIsolatedCell(TraversableZoneId self, GridPosition2D origin)
        {
            var availableDirections = DirectionalityInformation.None;
            foreach (var (_, m) in movementData)
            {
                var directions = m.OutboundDirections.TryGetMapValue(ref outboundDirectionTile, origin.X, origin.Y, DirectionalityInformation.None);
                if (directions != DirectionalityInformation.None)
                {
                    availableDirections |= directions;
                }
            }

            Optional<PathfinderRegionEdge> edge = default;
            PathfinderRegionEdge generatedEdge;
            for (var dx = 0; dx < 8; dx += directionStep)
            {
                var d = Direction.Up.MoveClockwise(dx);
                if (!availableDirections.IsMovementAllowed(d))
                {
                    if (edge.TryGetValue(out generatedEdge))
                    {
                        this.edgeData.AddOutboundEdge(movementKind ,generatedEdge);
                        edge = default;
                    }

                    continue;
                }

                var pos = origin + d;
                var targetZone = QueryZoneData(pos);
                if (targetZone.ZoneId == self)
                {
                    if (edge.TryGetValue(out generatedEdge))
                    {
                        this.edgeData.AddOutboundEdge(movementKind ,generatedEdge);
                        edge = default;
                    }

                    continue;
                }

                if (edge.TryGetValue(out generatedEdge))
                {
                    if (generatedEdge.TargetZone == targetZone)
                    {
                        continue;
                    }

                    this.edgeData.AddOutboundEdge(movementKind ,generatedEdge);
                }

                var edgeId = zoneData.GenerateEdgeId();
                generatedEdge = new PathfinderRegionEdge(new GlobalTraversableZoneId(zoneData.Bounds.Position, self),
                                                    edgeId, origin, d,
                                                    targetZone);
                edge = generatedEdge;
            }

            if (edge.TryGetValue(out generatedEdge))
            {
                this.edgeData.AddOutboundEdge(movementKind ,generatedEdge);
            }
        }

        GlobalTraversableZoneId QueryZoneData(GridPosition2D pos)
        {
            var x = zoneData2D.TryGetMapValue(ref zoneTile, pos.X, pos.Y, default);
            if (zoneTile != null)
            {
                var tileIndex = zoneTile.Bounds.Position;
                return new GlobalTraversableZoneId(tileIndex, x.zone);
            }

            return default;
        }

        bool TryFindFirstClockWise(DirectionalityInformation availableDirections,
                                   Direction comingFrom,
                                   DirectionalityInformation visitedDirections,
                                   out Direction result)
        {
            for (var dx = 0; dx < 8; dx += directionStep)
            {
                var d = comingFrom.MoveClockwise(dx);
                if (visitedDirections.IsMovementAllowed(d))
                {
                    continue;
                }

                if (availableDirections.IsMovementAllowed(d))
                {
                    result = d;
                    return true;
                }
            }

            // Special case of an isolated single-cell zone.
            result = Direction.None;
            return true;
        }

        public void ReconnectEdges(DistanceCalculation movementKind)
        {
            this.movementKind = movementKind;
            this.directionStep = (movementKind == DistanceCalculation.Manhattan) ? 2 : 1;

            var zoneSet = dirtyZoneSetPool.Get();
            try
            {
                visitedNodes.Clear();
                var state = zoneData.State;
                var bounds = zoneData.Bounds;
                if (state == RegionEdgeState.Dirty)
                {
                    CollectTraversableZones(zoneSet, bounds);
                }
                else
                {
                    if ((state & RegionEdgeState.EdgeNorth) == RegionEdgeState.EdgeNorth)
                    {
                        CollectTraversableZones(zoneSet, new Rectangle(bounds.X, bounds.Y, bounds.Width, 1));
                    }
                    if ((state & RegionEdgeState.EdgeEast) == RegionEdgeState.EdgeEast)
                    {
                        CollectTraversableZones(zoneSet, new Rectangle(bounds.MaxExtentX, bounds.Y, 1, bounds.Height));
                    }
                    if ((state & RegionEdgeState.EdgeSouth) == RegionEdgeState.EdgeSouth)
                    {
                        CollectTraversableZones(zoneSet, new Rectangle(bounds.X, bounds.MaxExtentY, bounds.Width, 1));
                    }
                    if ((state & RegionEdgeState.EdgeWest) == RegionEdgeState.EdgeWest)
                    {
                        CollectTraversableZones(zoneSet, new Rectangle(bounds.X, bounds.Y, 1, bounds.Height));
                    }
                    
                }

                if (zoneSet.Count == 0)
                {
                    return;
                }
                ConnectPartialArea(zoneData.Bounds, zoneSet);
            }
            finally
            {
                dirtyZoneSetPool.Return(zoneSet);
            }
        }

        void CollectTraversableZones(HashSet<TraversableZoneId> zoneSet, Rectangle area)
        {
            foreach (var pos in area.Contents)
            {
                var z = zoneData[pos.X, pos.Y].zone;
                if (z == TraversableZoneId.Empty)
                {
                    continue;
                }

                zoneSet.Add(z);
            }
        }

        readonly struct TraversalStep : IEquatable<TraversalStep>
        {
            public readonly GridPosition2D Origin;
            public readonly Direction Direction;
            public readonly Optional<(PathfinderRegionEdge edge, BufferList<(GridPosition2D, Direction)> positions)> CurrentEdge;

            public TraversalStep(GridPosition2D origin, Direction direction)
            {
                Origin = origin;
                Direction = direction;
                CurrentEdge = default;
            }

            public TraversalStep(GridPosition2D origin, Direction direction, Optional<(PathfinderRegionEdge edge, BufferList<(GridPosition2D, Direction)> positions)> currentEdge)
            {
                Origin = origin;
                Direction = direction;
                CurrentEdge = currentEdge;
            }

            public bool Equals(TraversalStep other)
            {
                return Origin.Equals(other.Origin) && Direction == other.Direction;
            }

            public override bool Equals(object? obj)
            {
                return obj is TraversalStep other && Equals(other);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (Origin.GetHashCode() * 397) ^ (int)Direction;
                }
            }

            public static bool operator ==(TraversalStep left, TraversalStep right)
            {
                return left.Equals(right);
            }

            public static bool operator !=(TraversalStep left, TraversalStep right)
            {
                return !left.Equals(right);
            }
        };
    }
}