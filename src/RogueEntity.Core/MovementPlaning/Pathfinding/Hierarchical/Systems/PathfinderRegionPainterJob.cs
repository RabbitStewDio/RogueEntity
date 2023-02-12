using RogueEntity.Api.Utils;
using RogueEntity.Core.GridProcessing.Directionality;
using RogueEntity.Core.Movement.CostModifier;
using RogueEntity.Core.MovementPlaning.Pathfinding.Hierarchical.Data;
using RogueEntity.Core.Positioning.Algorithms;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.DataViews;
using System;
using System.Buffers;
using System.Collections.Generic;

namespace RogueEntity.Core.MovementPlaning.Pathfinding.Hierarchical.Systems
{
    public readonly struct PathfinderRegionPainterJob
    {
        public readonly PathfinderRegionDataView Region;
        public readonly int Z;
        readonly PathfinderRegionPainterSharedDataFactory sharedData;
        readonly DistanceCalculation movementType;
        readonly ReadOnlyListWrapper<Direction>[] directionData;
        readonly DirectionalityInformation[] edgeMapping;
        readonly List<(long flag, MovementCostData2D data)> movementCostsOnLevel;

        public PathfinderRegionPainterJob(PathfinderRegionPainterSharedDataFactory sharedData,
                                          DistanceCalculation movementType,
                                          PathfinderRegionDataView region,
                                          int z,
                                          List<(long flag, MovementCostData2D data)> movementCostsOnLevel)
        {
            this.sharedData = sharedData;
            this.movementType = movementType;
            this.Region = region;
            this.Z = z;
            this.movementCostsOnLevel = movementCostsOnLevel;
            this.directionData = DirectionalityLookup.Get(movementType.AsAdjacencyRule());
            this.edgeMapping = movementType == DistanceCalculation.Manhattan ? edgeMappingCardinal : EdgeMappingComplete;
        }

        public void Process()
        {
            var costTile = ArrayPool<IReadOnlyBoundedDataView<float>>.Shared.Rent(movementCostsOnLevel.Count);
            try
            {
                var tileRefs = new CachedTileReferences(costTile);

                var rawData = Region.Data;
                Region.ClearData();

                // first map out the region by connecting all cells into traversable zones.
                foreach (var pos in Region.Bounds.Contents)
                {
                    var idx = Region.GetRawIndexUnsafe(pos);
                    var regionKey = rawData[idx];

                    if (regionKey.zone != TraversableZoneId.Empty)
                    {
                        // already processed.
                        continue;
                    }

                    FloodFill(pos, ref tileRefs);
                }

                if (movementType != DistanceCalculation.Manhattan)
                {
                    foreach (var pos in Region.Bounds.Contents)
                    {
                        RemoveInnerEdge(pos);
                    }
                }
            }
            finally
            {
                Array.Clear(costTile, 0, costTile.Length);
                ArrayPool<IReadOnlyBoundedDataView<float>>.Shared.Return(costTile);
            }
        }

        /// <summary>
        ///   It is impossible to decide whether a node is part of the edge for some nodes. Nodes that have a diagonal
        ///   edge passing by are within one (diagonal) step of the zone boundary, but should not be part of the
        ///   boundary itself. 
        /// </summary>
        /// <param name="pos"></param>
        void RemoveInnerEdge(GridPosition2D pos)
        {
            var idx = Region.GetRawIndexUnsafe(pos);
            var rawData = Region.Data;
            var (zone, connections) = rawData[idx];

            for (var d = 0; d < 8; d += 2)
            {
                var dir = Direction.Up.MoveClockwise(d);
                var testPos = pos + dir;
                var testIdx = Region.GetRawIndexUnsafe(testPos);
                if (testIdx < 0 || testIdx >= rawData.Length)
                {
                    continue;
                }

                var testDir = dir.Inverse();
                if (!rawData[testIdx].zoneEdges.IsMovementAllowed(testDir))
                {
                    connections = connections.WithOut(dir);
                }
            }

            rawData[idx] = (zone, connections);
        }

        long ComputeAvailableMovementModes(int targetPosX, int targetPosY, ref CachedTileReferences tileRefs)
        {
            long availableMovementModes = 0;
            for (var index = 0; index < movementCostsOnLevel.Count; index++)
            {
                var (f, s) = movementCostsOnLevel[index];
                ref var dt = ref tileRefs.CostDirectionsTile[index];
                var dir = s.Costs.TryGetMapValue(ref dt, targetPosX, targetPosY, 0);
                if (dir > 0)
                {
                    availableMovementModes |= f;
                }
            }

            return availableMovementModes;
        }

        void ComputeIsolatedZone(GridPosition2D start, ref CachedTileReferences tileRefs)
        {
            var outboundMovements = 0L;
            foreach (var (f, m) in movementCostsOnLevel)
            {
                var outboundMovementDirections = m.OutboundDirections.TryGetMapValue(ref tileRefs.EdgeOutboundTile,
                                                                                     start.X, start.Y,
                                                                                     DirectionalityInformation.None);
                if (outboundMovementDirections != DirectionalityInformation.None)
                {
                    outboundMovements |= f;
                }
            }

            if (outboundMovements != 0)
            {
                var defaultNode = default((TraversableZoneId, DirectionalityInformation));
                ref var currentNode = ref Region.TryGetForUpdate(start.X, start.Y, ref defaultNode, out var success);
                if (!success)
                {
                    ThrowNodeUpdateError(in start);
                }

                currentNode = (Region.GenerateZoneId(), edgeMapping[(int)DirectionalityInformation.None]);
            }
        }

        void FloodFill(in GridPosition2D start, ref CachedTileReferences tileRefs)
        {
            if (start == new GridPosition2D(1, 1))
            {
                Console.WriteLine("HERE!");
            }
            
            var zoneMovementModes = ComputeAvailableMovementModes(start.X, start.Y, ref tileRefs);
            if (zoneMovementModes == 0)
            {
                ComputeIsolatedZone(start, ref tileRefs);
                return;
            }

            var zoneId = Region.GenerateZoneId();
            using var d = sharedData.Get();
            var openNodes = d.Data;

            openNodes.Clear();
            openNodes.Enqueue(start, 0);

            while (openNodes.Count != 0)
            {
                var currentPosition = openNodes.Dequeue();
                var defaultNode = default((TraversableZoneId, DirectionalityInformation));
                ref var currentNode = ref Region.TryGetForUpdate(currentPosition.X, currentPosition.Y, ref defaultNode, out var success);
                if (!success)
                {
                    ThrowNodeUpdateError(in currentPosition);
                }

                if (currentNode.zone != TraversableZoneId.Empty)
                {
                    continue;
                }

                var directionInfo = default(DirectionalityInformation);

                var directionsOfNeighbours = PopulateTraversableDirections(currentPosition, ref tileRefs);
                for (var index = 0; index < directionsOfNeighbours.Count; index++)
                {
                    var dir = directionsOfNeighbours[index];
                    var neighborPos = currentPosition.Add(dir.ToCoordinates());
                    ref var neighborRef = ref Region.TryGetForUpdate(neighborPos.X, neighborPos.Y, ref defaultNode, out success);
                    if (!success)
                    {
                        // the node is outside of the region and thus never evaluated.
                        continue;
                    }

                    if (neighborRef.zone == zoneId)
                    {
                        directionInfo = directionInfo.With(dir);
                        continue;
                    }

                    if (neighborRef.zone != TraversableZoneId.Empty)
                    {
                        // This neighbor has already been evaluated at shortest possible path, don't re-add
                        continue;
                    }

                    if (!EdgeCostInformation(in currentPosition, in dir, ref tileRefs, out var movementModesOnEdge))
                    {
                        continue;
                    }

                    if (movementModesOnEdge != zoneMovementModes)
                    {
                        // any change in movement modes is considered a zone edge
                        continue;
                    }

                    directionInfo = directionInfo.With(dir);
                    openNodes.Enqueue(neighborPos, 0);
                }

                currentNode = (zoneId, edgeMapping[(int)directionInfo]);
            }
        }

        internal static readonly DirectionalityInformation[] EdgeMappingComplete = new DirectionalityInformation[256];
        static readonly DirectionalityInformation[] edgeMappingCardinal = new DirectionalityInformation[256];

        static PathfinderRegionPainterJob()
        {
            for (int idx = 0; idx < 256; idx += 1)
            {
                var org = (DirectionalityInformation)idx;
                EdgeMappingComplete[idx] = DetectEdge(org);
                edgeMappingCardinal[idx] = DetectEdgeCardinal(org);
            }
        }

        /// <summary>
        ///    Tests whether
        /// </summary>
        /// <param name="input"></param>
        /// <param name="test"></param>
        /// <param name="potentialEdges"></param>
        /// <returns></returns>
        internal static DirectionalityInformation TestEdge(DirectionalityInformation input, DirectionalityInformation test, DirectionalityInformation potentialEdges)
        {
            if ((input & test) != 0)
            {
                return DirectionalityInformation.None;
            }

            var maybeEdge = input & potentialEdges;
            if (maybeEdge == potentialEdges)
            {
                //      return DirectionalityInformation.None;
            }

            return maybeEdge;
        }

        internal static DirectionalityInformation DetectEdge(DirectionalityInformation input)
        {
            var retval = DirectionalityInformation.None;
            retval |= TestEdge(input, DirectionalityInformation.Up, DirectionalityInformation.UpLeft | DirectionalityInformation.UpRight);
            retval |= TestEdge(input, DirectionalityInformation.UpLeft, DirectionalityInformation.Up | DirectionalityInformation.Left);
            retval |= TestEdge(input, DirectionalityInformation.Left, DirectionalityInformation.UpLeft | DirectionalityInformation.DownLeft);
            retval |= TestEdge(input, DirectionalityInformation.DownLeft, DirectionalityInformation.Left | DirectionalityInformation.Down);
            retval |= TestEdge(input, DirectionalityInformation.Down, DirectionalityInformation.DownLeft | DirectionalityInformation.DownRight);
            retval |= TestEdge(input, DirectionalityInformation.DownRight, DirectionalityInformation.Down | DirectionalityInformation.Right);
            retval |= TestEdge(input, DirectionalityInformation.Right, DirectionalityInformation.DownRight | DirectionalityInformation.UpRight);
            retval |= TestEdge(input, DirectionalityInformation.UpRight, DirectionalityInformation.Right | DirectionalityInformation.Up);
            return retval;
        }

        static DirectionalityInformation DetectEdgeCardinal(DirectionalityInformation input)
        {
            var retval = DirectionalityInformation.None;
            retval |= TestEdge(input, DirectionalityInformation.Up, DirectionalityInformation.Left | DirectionalityInformation.Right);
            retval |= TestEdge(input, DirectionalityInformation.Down, DirectionalityInformation.Left | DirectionalityInformation.Right);
            retval |= TestEdge(input, DirectionalityInformation.Left, DirectionalityInformation.Up | DirectionalityInformation.Down);
            retval |= TestEdge(input, DirectionalityInformation.Right, DirectionalityInformation.Up | DirectionalityInformation.Down);
            return retval;
        }

        void ThrowNodeUpdateError(in GridPosition2D pos)
        {
            throw new InvalidOperationException($"Unable to update existing node at {pos}.");
        }

        /// <summary>
        ///   Returns a list of neighbouring nodes from this position. The returned directions
        ///   are filtered by all movement modes as a zone is defined as all cells with the same
        ///   movement modes available on each cell. So even if the target movement mode could
        ///   traverse to other cells, those cells would be part of a difference traversable zone
        ///   if they support a different set of movements. 
        /// </summary>
        /// <param name="basePos"></param>
        /// <param name="tileRefs"></param>
        /// <returns></returns>
        ReadOnlyListWrapper<Direction> PopulateTraversableDirections(in GridPosition2D basePos,
                                                                     ref CachedTileReferences tileRefs)
        {
            var targetPosX = basePos.X;
            var targetPosY = basePos.Y;
            var result = DirectionalityInformation.None;
            foreach (var (_, m) in movementCostsOnLevel)
            {
                var dirIn = m.InboundDirections.TryGetMapValue(ref tileRefs.EdgeInboundTile, targetPosX, targetPosY, DirectionalityInformation.None);
                var dirOut = m.OutboundDirections.TryGetMapValue(ref tileRefs.EdgeOutboundTile, targetPosX, targetPosY, DirectionalityInformation.None);
                var allowedMovements = (dirIn & dirOut);
                result |= allowedMovements;
            }

            return directionData[(int)result];
        }

        struct CachedTileReferences
        {
            public CachedTileReferences(IReadOnlyBoundedDataView<float>?[] costTile) : this()
            {
                this.CostDirectionsTile = costTile;
            }

            public readonly IReadOnlyBoundedDataView<float>?[] CostDirectionsTile;
            public IReadOnlyBoundedDataView<DirectionalityInformation>? EdgeOutboundTile;
            public IReadOnlyBoundedDataView<DirectionalityInformation>? EdgeInboundTile;
        }

        bool EdgeCostInformation(in GridPosition2D sourceNode,
                                 in Direction d,
                                 ref CachedTileReferences tileRefs,
                                 out long movementModes)
        {
            var targetPos = sourceNode + d;
            movementModes = ComputeAvailableMovementModes(targetPos.X, targetPos.Y, ref tileRefs);
            return true;
        }
    }
}