using System;
using System.Runtime.CompilerServices;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.DataViews;
using System.Diagnostics.CodeAnalysis;

namespace RogueEntity.Core.Positioning.Algorithms
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
        readonly PriorityQueue<float, GridPosition2D> openNodes;
        IBoundedDataView<AStarNode>? nodeTile;

        protected AStarGridBase(IBoundedDataViewPool<AStarNode> pool)
        {
            this.openNodes = new PriorityQueue<float, GridPosition2D>(4096);
            this.nodes = new PooledDynamicDataView2D<AStarNode>(pool);
        }

        public IReadOnlyDynamicDataView2D<AStarNode> Nodes => nodes;

        /// <summary>
        ///   Returns a list of neighbouring nodes from this position. It is safe to return
        ///   the same (but updated) list instance each call, as it is guaranteed that previous
        ///   values of the return value of this method is not retained or accessed between
        ///   subsequent method calls.
        /// </summary>
        /// <param name="basePosition"></param>
        /// <returns></returns>
        protected abstract ReadOnlyListWrapper<Direction> PopulateTraversableDirections(in GridPosition2D basePosition);

        protected abstract bool EdgeCostInformation(in GridPosition2D sourceNode,
                                                    in Direction d,
                                                    float sourceNodeCost,
                                                    out float totalPathCost,
                                                    [MaybeNullWhen(false)] out TExtraNodeInfo nodeInfo);

        protected abstract bool IsTargetNode(in GridPosition2D pos);

        protected abstract float Heuristic(in GridPosition2D pos);

        protected void Clear()
        {
            nodes.Clear();
            openNodes.Clear();
            nodeTile = null;
            NodesEvaluated = 0;
        }

        protected void EnqueueStartPosition(GridPosition2D start)
        {
            nodes.TrySet(start.X, start.Y, AStarNode.Start());
            openNodes.Enqueue(start, Heuristic(start));
        }

        void ThrowNodeUpdateError(in GridPosition2D pos)
        {
            throw new InvalidOperationException($"Unable to update existing node at {pos}.");
        }

        protected (PathFinderResult result, float cost) FindPath(GridPosition2D start,
                                                                 BufferList<GridPosition2D> pathBuffer,
                                                                 int searchLimit = int.MaxValue)
        {
            pathBuffer.Clear();

            if (IsTargetNode(start))
            {
                return (PathFinderResult.Found, 0);
            }

            Clear();
            EnqueueStartPosition(start);
            return ContinueFindPath(pathBuffer, searchLimit);
        }

        protected (PathFinderResult, float cost) ContinueFindPath(BufferList<GridPosition2D> pathBuffer,
                                                                  int searchLimit = int.MaxValue)
        {
            var defaultNode = AStarNode.Empty;
            int searchedNodes = NodesEvaluated;
            AStarNode bestSoFar = default;
            GridPosition2D bestSoFarPos = default;
            float bestSoFarH = float.MaxValue;
            while (openNodes.Count != 0)
            {
                var currentPosition = openNodes.Dequeue();
                // if (!nodes.TryGetTileForUpdate(ref nodeTile, currentPosition.X, currentPosition.Y, DataViewCreateMode.CreateMissing))
                // {
                //     ThrowNodeUpdateError(in currentPosition);
                // }

                ref var currentNode = ref nodes.TryGetRefForUpdate(ref nodeTile, currentPosition.X, currentPosition.Y, ref defaultNode, out var success);
                if (!success)
                {
                    ThrowNodeUpdateError(in currentPosition);
                }

                if (currentNode.IsClosed())
                {
                    continue;
                }

                searchedNodes += 1;
                var currentHeuristic = Heuristic(in currentPosition);
                if (currentHeuristic < bestSoFarH)
                {
                    bestSoFarH = currentHeuristic;
                    bestSoFarPos = currentPosition;
                    bestSoFar = currentNode;
                }

                currentNode = currentNode.Close();

                if (IsTargetNode(currentPosition)) // We found the end, cleanup and return the path
                {
                    ProduceResult(currentPosition, currentNode, pathBuffer);
                    NodesEvaluated = searchedNodes;
                    openNodes.Clear();
                    return (PathFinderResult.Found, currentNode.AccumulatedCost);
                }

                if (searchedNodes >= searchLimit)
                {
                    ProduceResult(currentPosition, currentNode, pathBuffer);
                    NodesEvaluated = searchedNodes;
                    openNodes.Clear();
                    return (PathFinderResult.SearchLimitReached, currentNode.AccumulatedCost);
                }

                if (currentNode.DistanceFromStart == ushort.MaxValue)
                {
                    continue;
                }


                var directionsOfNeighbours = PopulateTraversableDirections(currentPosition);
                for (var index = 0; index < directionsOfNeighbours.Count; index++)
                {
                    var dir = directionsOfNeighbours[index];
                    var neighborPos = currentPosition.Add(dir.ToCoordinates());
                    ref var neighborRef = ref nodes.TryGetRefForUpdate(ref nodeTile, neighborPos.X, neighborPos.Y, ref defaultNode, out success, DataViewCreateMode.CreateMissing);
                    if (!success)
                    {
                        ThrowNodeUpdateError(in neighborPos);
                    }

                    if (neighborRef.IsClosed())
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

                    var isNeighborOpen = neighborRef.State == AStarNode.NodeState.Open;
                    if (isNeighborOpen && IsExistingResultBetter(in neighborPos, in totalPathCost))
                    {
                        // Not a better path
                        continue;
                    }

                    // We found a best path, so record and update
                    neighborRef = new AStarNode(AStarNode.NodeState.Open,
                                                totalPathCost,
                                                Directions.GetDirection(currentPosition, neighborPos),
                                                (ushort)(currentNode.DistanceFromStart + 1)
                    );

                    openNodes.UpdatePriority(neighborPos, totalPathCost + Heuristic(in neighborPos));
                    UpdateNode(neighborPos, nodeInfo);
                }
            }

            NodesEvaluated = searchedNodes;
            var cost = 0f;
            if (bestSoFarH < float.MaxValue)
            {
                ProduceResult(bestSoFarPos, bestSoFar, pathBuffer);
                cost = bestSoFar.AccumulatedCost;
            }
            else
            {
                pathBuffer.Clear();
            }

            openNodes.Clear();
            return (PathFinderResult.NotFound, cost);
        }

        public int NodesEvaluated { get; private set; }

        protected virtual void UpdateNode(in GridPosition2D pos, TExtraNodeInfo nodeInfo)
        {
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected bool IsExistingResultBetter(in GridPosition2D coord, in float newWeight)
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

        void ProduceResult(GridPosition2D currentPosition, AStarNode currentNode, BufferList<GridPosition2D> pathBuffer)
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