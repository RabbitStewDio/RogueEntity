using System;
using System.Collections.Generic;
using RogueEntity.Core.Utils.DataViews;

namespace RogueEntity.Core.Utils.Algorithms
{
    public enum PathFinderResult
    {
        NotFound = 0,
        SearchLimitReached = 1,
        Found = 2
    }

    public abstract class AStarGridBase<TExtraNodeInfo>
    {
        readonly IDynamicDataView2D<AStarNode> nodes;
        IBoundedDataView<AStarNode> nodeTile;
        readonly PriorityQueue<float, Position2D> openNodes;
        readonly List<Direction> directionsOfNeighbours;

        protected AStarGridBase(IBoundedDataViewPool<AStarNode> pool)
        {
            this.directionsOfNeighbours = new List<Direction>();
            this.openNodes = new PriorityQueue<float, Position2D>(4096);
            this.nodes = new PooledDynamicDataView2D<AStarNode>(pool);
        }

        public IReadOnlyDynamicDataView2D<AStarNode> Nodes => nodes;

        protected abstract void PopulateTraversableDirections(Position2D basePosition, List<Direction> buffer);
        protected abstract bool EdgeCostInformation(in Position2D sourceNode, in Direction d, float sourceNodeCost, out float totalPathCost, out TExtraNodeInfo nodeInfo);

        protected abstract bool IsTargetNode(in Position2D pos);

        protected abstract float Heuristic(in Position2D pos);

        protected void Clear()
        {
            nodes.Clear();
            openNodes.Clear();
            nodeTile = null;
        }

        protected void EnqueueStartPosition(Position2D start)
        {
            nodes[start.X, start.Y] = AStarNode.Start();
            openNodes.Enqueue(start, Heuristic(start));
        }

        protected PathFinderResult FindPath(Position2D start,
                                            List<Position2D> pathBuffer,
                                            int searchLimit = int.MaxValue)
        {
            pathBuffer.Clear();

            if (IsTargetNode(start))
            {
                return PathFinderResult.Found;
            }

            Clear();
            EnqueueStartPosition(start);
            return ContinueFindPath(pathBuffer, searchLimit);
        }

        protected PathFinderResult ContinueFindPath(List<Position2D> pathBuffer,
                                                    int searchLimit = int.MaxValue)
        {
            int searchedNodes = 0;
            while (openNodes.Count != 0)
            {
                searchedNodes += 1;
                var currentPosition = openNodes.Dequeue();
                var currentNode = nodes.TryGetForUpdate(ref nodeTile, currentPosition.X, currentPosition.Y, in AStarNode.Empty, DataViewCreateMode.CreateMissing);

                var closedNode = currentNode.Close();
                if (!nodes.TryUpdate(ref nodeTile, currentPosition.X, currentPosition.Y, in closedNode, DataViewCreateMode.CreateMissing))
                {
                    throw new Exception("Unable to update existing node.");
                }

                if (IsTargetNode(currentPosition)) // We found the end, cleanup and return the path
                {
                    ProduceResult(currentPosition, currentNode, pathBuffer);
                    return PathFinderResult.Found;
                }

                if (searchedNodes >= searchLimit)
                {
                    ProduceResult(currentPosition, currentNode, pathBuffer);
                    return PathFinderResult.SearchLimitReached;
                }

                if (currentNode.DistanceFromStart == ushort.MaxValue)
                {
                    continue;
                }

                PopulateTraversableDirections(currentPosition, directionsOfNeighbours);
                for (var index = 0; index < directionsOfNeighbours.Count; index++)
                {
                    var dir = directionsOfNeighbours[index];
                    var neighborPos = currentPosition.Add(dir.ToCoordinates());
                    var neighbor = nodes.TryGetForUpdate(ref nodeTile, neighborPos.X, neighborPos.Y, in AStarNode.Empty, DataViewCreateMode.CreateMissing);

                    if (neighbor.IsClosed())
                    {
                        // This neighbor has already been evaluated at shortest possible path, don't re-add
                        continue;
                    }

                    if (!EdgeCostInformation(in currentPosition, in dir, currentNode.AccumulatedCost, out var totalPathCost, out var nodeInfo))
                    {
                        continue;
                    }

#if DEBUG
                    if (totalPathCost < currentNode.AccumulatedCost)
                    {
                        throw new Exception("Assertion failed: Path segment cost should never be negative");
                    }
#endif

                    var isNeighborOpen = neighbor.State == AStarNode.NodeState.Open;
                    if (isNeighborOpen && IsExistingResultBetter(in neighborPos, in totalPathCost))
                    {
                        // Not a better path
                        continue;
                    }

                    // We found a best path, so record and update
                    var newNode = new AStarNode(AStarNode.NodeState.Open,
                                                totalPathCost,
                                                Directions.GetDirection(currentPosition, neighborPos),
                                                (ushort)(currentNode.DistanceFromStart + 1)
                    );

                    if (nodes.TryUpdate(ref nodeTile, neighborPos.X, neighborPos.Y, in newNode, DataViewCreateMode.CreateMissing))
                    {
                        openNodes.UpdatePriority(neighborPos, totalPathCost + Heuristic(in neighborPos));
                        UpdateNode(neighborPos, nodeInfo);
                    }
                }
            }

            openNodes.Clear();
            Console.WriteLine("Searched: " + searchedNodes);
            return PathFinderResult.NotFound;
        }

        protected virtual void UpdateNode(in Position2D pos, TExtraNodeInfo nodeInfo)
        {
        }

        protected virtual bool IsExistingResultBetter(in Position2D coord, in float newWeight)
        {
            // All non-starting nodes are initialized with a weight of zero at the start.
            // This allows us to use Array.Clear which uses MemSet to efficiently
            // erase the memory.
            //
            // Starting nodes are initialized with a positive weight, that is reduced 
            // by the cost of each step until it reaches zero to produce a outward flowing
            // field of influence
            //
            // Thus a node is considered "better" if its existing weight is *larger*
            // than the newly given weight, representing the idea that that node must 
            // be closer to one of the source nodes.

            if (newWeight <= 0)
            {
                // Stop processing nodes once we hit the lower bounds of the weight values.
                return true;
            }

            if (!nodes.TryGet(coord.X, coord.Y, out var existingWeight))
            {
                // Stop processing nodes once we left the valid map area.
                return true;
            }

            return existingWeight.AccumulatedCost < newWeight;
        }

        void ProduceResult(Position2D currentPosition, AStarNode currentNode, List<Position2D> pathBuffer)
        {
            pathBuffer.Capacity = Math.Max(pathBuffer.Capacity, currentNode.DistanceFromStart);
            while (currentNode.DirectionToParent != Direction.None)
            {
                pathBuffer.Add(currentPosition);
                var dir = currentNode.DirectionToParent.ToCoordinates();
                var cp = currentPosition.With(currentPosition.X - dir.X, currentPosition.Y - dir.Y);
                currentPosition = cp;

                ushort distanceFromStart = currentNode.DistanceFromStart;
                if (!nodes.TryGet(currentPosition.X, currentPosition.Y, out currentNode))
                {
                    throw new ArgumentException();
                }

                if (currentNode.DistanceFromStart >= distanceFromStart)
                {
                    throw new ArgumentException();
                }
            }
        }
    }
}