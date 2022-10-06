using RogueEntity.Api.Utils;
using RogueEntity.Core.GridProcessing.Directionality;
using RogueEntity.Core.Movement.CostModifier;
using RogueEntity.Core.Positioning.Algorithms;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.DataViews;
using System;
using System.Buffers;
using System.Collections.Generic;

namespace RogueEntity.Core.MovementPlaning.Pathfinding.Hierarchical
{
    public readonly struct PathfinderRegionPainter
    {
        public readonly PathfinderRegionDataView Region;
        public readonly int Z;
        readonly MovementCostData2D movement;
        readonly PathfinderRegionPainterSharedDataFactory sharedData;
        readonly ReadOnlyListWrapper<Direction>[] directionData;
        readonly DirectionalityInformation[] edgeMapping;
        public readonly List<MovementCostData2D> MovementCostsOnLevel;

        public PathfinderRegionPainter(PathfinderRegionPainterSharedDataFactory sharedData,
                                       DistanceCalculation movementType,
                                       PathfinderRegionDataView region,
                                       int z,
                                       MovementCostData2D movement,
                                       List<MovementCostData2D> movementCostsOnLevel)
        {
            this.sharedData = sharedData;
            this.Region = region;
            this.Z = z;
            this.movement = movement;
            this.MovementCostsOnLevel = movementCostsOnLevel;
            this.directionData = DirectionalityLookup.Get(movementType.AsAdjacencyRule());
            this.edgeMapping = movementType == DistanceCalculation.Manhattan ? edgeMappingCardinal : edgeMappingComplete;
        }

        public void Process()
        {
            var directionsTileIn = ArrayPool<IReadOnlyBoundedDataView<DirectionalityInformation>>.Shared.Rent(MovementCostsOnLevel.Count);
            var directionsTileOut = ArrayPool<IReadOnlyBoundedDataView<DirectionalityInformation>>.Shared.Rent(MovementCostsOnLevel.Count);
            try
            {
                var tileRefs = new CachedTileReferences(directionsTileIn, directionsTileOut);

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
            }
            finally
            {
                Array.Clear(directionsTileIn, 0, directionsTileIn.Length);
                Array.Clear(directionsTileOut, 0, directionsTileOut.Length);
                ArrayPool<IReadOnlyBoundedDataView<DirectionalityInformation>>.Shared.Return(directionsTileIn);
                ArrayPool<IReadOnlyBoundedDataView<DirectionalityInformation>>.Shared.Return(directionsTileOut);
            }
        }

        long ComputeAvailableMovementModes(int targetPosX, int targetPosY, ref CachedTileReferences tileRefs)
        {
            long availableMovementModes = 0;
            for (var index = 0; index < MovementCostsOnLevel.Count; index++)
            {
                var s = MovementCostsOnLevel[index];
                ref var dt = ref tileRefs.OutboundDirectionsTile[index];
                var dir = s.OutboundDirections.TryGetMapValue(ref dt, targetPosX, targetPosY, DirectionalityInformation.None);
                if (dir != DirectionalityInformation.None)
                {
                    availableMovementModes |= (1L << index);
                }
            }

            return availableMovementModes;
        }

        long ComputeAvailableMovementModesInbound(int targetPosX, int targetPosY, ref CachedTileReferences tileRefs)
        {
            long availableMovementModes = 0;
            for (var index = 0; index < MovementCostsOnLevel.Count; index++)
            {
                var s = MovementCostsOnLevel[index];
                ref var dt = ref tileRefs.InboundDirectionsTile[index];
                var dir = s.InboundDirections.TryGetMapValue(ref dt, targetPosX, targetPosY, DirectionalityInformation.None);
                if (dir != DirectionalityInformation.None)
                {
                    availableMovementModes |= (1L << index);
                }
            }

            return availableMovementModes;
        }

        void FloodFill(in Position2D start, ref CachedTileReferences tileRefs)
        {
            // ReSharper disable once ReplaceWithSingleAssignment.False
            var debug = false;
            if (start == new Position2D(3, 3))
            {
                debug = true;
            }

            var zoneMovementModes = ComputeAvailableMovementModes(start.X, start.Y, ref tileRefs);
            if (zoneMovementModes == 0)
            {
                return;
            }

            var zoneId = Region.GenerateZoneId(start);
            using var d = sharedData.Get();
            var (openNodes, _) = d.Data;

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
                        ThrowNodeUpdateError(in neighborPos);
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

                    if (!Region.TryGetRawIndex(neighborPos, out _))
                    {
                        // this tile would be out of bounds. This is automatically treated as boundary edge of the traversal zone.
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
                    var isNeighborOpen = neighborRef.zone == TraversableZoneId.Empty;
                    if (isNeighborOpen)
                    {
                        // Not a better path
                        // continue;
                    }

                    openNodes.Enqueue(neighborPos, 0);
                }

                Region.Data[Region.GetRawIndexUnsafe(currentPosition)] = (zoneId, edgeMapping[(int)directionInfo]);
            }
        }

        static readonly DirectionalityInformation[] edgeMappingComplete = new DirectionalityInformation[256];
        static readonly DirectionalityInformation[] edgeMappingCardinal = new DirectionalityInformation[256];

        static PathfinderRegionPainter()
        {
            for (int idx = 0; idx < 256; idx += 1)
            {
                var org = (DirectionalityInformation)idx;
                edgeMappingComplete[idx] = DetectEdge(org);
                edgeMappingCardinal[idx] = DetectEdgeCardinal(org);
            }
        }

        internal static DirectionalityInformation TestEdge(DirectionalityInformation input, DirectionalityInformation test, DirectionalityInformation potentialEdges)
        {
            if ((input & test) != 0)
            {
                return DirectionalityInformation.None;
            }
            
            var maybeEdge = input & potentialEdges;
            if (maybeEdge == potentialEdges)
            {
                return DirectionalityInformation.None;
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

        void ThrowNodeUpdateError(in Position2D pos)
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
        ReadOnlyListWrapper<Direction> PopulateTraversableDirections(in Position2D basePos, ref CachedTileReferences tileRefs)
        {
            var targetPosX = basePos.X;
            var targetPosY = basePos.Y;
            var allowedMovements = DirectionalityInformation.None;

            for (var index = 0; index < MovementCostsOnLevel.Count; index++)
            {
                var s = MovementCostsOnLevel[index];
                ref var dtOut = ref tileRefs.OutboundDirectionsTile[index];
                ref var dtIn = ref tileRefs.InboundDirectionsTile[index];
                var dirOut = s.OutboundDirections.TryGetMapValue(ref dtOut, targetPosX, targetPosY, DirectionalityInformation.None);
                var dirIn = s.InboundDirections.TryGetMapValue(ref dtIn, targetPosX, targetPosY, DirectionalityInformation.None);
                allowedMovements |= (dirIn & dirOut);
            }

            return directionData[(int)allowedMovements];
        }

        struct CachedTileReferences
        {
            public CachedTileReferences(IReadOnlyBoundedDataView<DirectionalityInformation>?[] outboundDirectionsTile,
                                        IReadOnlyBoundedDataView<DirectionalityInformation>?[] inboundDirectionsTile) : this()
            {
                this.OutboundDirectionsTile = outboundDirectionsTile;
                this.InboundDirectionsTile = inboundDirectionsTile;
            }

            public readonly IReadOnlyBoundedDataView<DirectionalityInformation>?[] OutboundDirectionsTile;
            public readonly IReadOnlyBoundedDataView<DirectionalityInformation>?[] InboundDirectionsTile;
            public IReadOnlyBoundedDataView<DirectionalityInformation>? EdgeOutboundTile;
            public IReadOnlyBoundedDataView<float>? EdgeCostsTile;
        }

        bool EdgeCostInformation(in Position2D sourceNode,
                                 in Direction d,
                                 ref CachedTileReferences tileRefs,
                                 out long movementModes)
        {
            var sourcePosX = sourceNode.X;
            var sourcePosY = sourceNode.Y;
            var dir = movement.OutboundDirections.TryGetMapValue(ref tileRefs.EdgeOutboundTile, sourcePosX, sourcePosY, DirectionalityInformation.None);
            if (dir == DirectionalityInformation.None)
            {
                movementModes = 0;
                return false;
            }

            if (!dir.IsMovementAllowed(d))
            {
                movementModes = 0;
                return false;
            }

            var targetPos = sourceNode + d;
            var sourceTileCost = movement.Costs.TryGetMapValue(ref tileRefs.EdgeCostsTile, sourcePosX, sourcePosY, 0);
            var targetTileCost = movement.Costs.TryGetMapValue(ref tileRefs.EdgeCostsTile, targetPos.X, targetPos.Y, 0);
            var tileCost = (sourceTileCost + targetTileCost) / 2.0f;
            if (tileCost <= 0)
            {
                // a cost of zero means its undefined. This should mean the tile is not valid.
                movementModes = 0;
                return false;
            }


            movementModes = ComputeAvailableMovementModesInbound(targetPos.X, targetPos.Y, ref tileRefs);
            return true;
        }
    }
}