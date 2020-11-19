using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using RogueEntity.Core.Utils.DataViews;

namespace RogueEntity.Core.Utils.Algorithms
{
    public abstract class AStarGridBase
    {
        /// <summary>
        ///   12 bytes.
        /// </summary>
        readonly struct AStarNode
        {
            internal enum NodeState : byte
            {
                [UsedImplicitly] None = 0, 
                Open = 1, 
                Closed = 2
            }

            // Whether or not the node has been closed
            public readonly NodeState State;

            public readonly Direction DirectionToParent;

            public readonly ushort DistanceFromStart;

            // (Partly estimated) distance to end point going thru this node
            public readonly float HeuristicValue;

            // (Known) distance from start to this node, by shortest known path
            public readonly float AccumulatedCost;


            public AStarNode(NodeState state, float heuristicValue, float accumulatedCost, Direction directionToParent, ushort distanceFromStart)
            {
                State = state;
                HeuristicValue = heuristicValue;
                AccumulatedCost = accumulatedCost;
                DirectionToParent = directionToParent;
                DistanceFromStart = distanceFromStart;
            }

            public static AStarNode Start(float h)
            {
                return new AStarNode(NodeState.Open, h, 0, Direction.None, 0);
            }

            public AStarNode Close()
            {
                return new AStarNode(NodeState.Closed, HeuristicValue, AccumulatedCost, DirectionToParent, DistanceFromStart);
            }

            public bool IsClosed()
            {
                return State == NodeState.Closed;
            }
        }

        readonly DynamicDataView2D<AStarNode> nodes;
        readonly PriorityQueue<float, Position2D> openNodes;
        readonly List<Direction> directionsOfNeighbours;

        protected AStarGridBase(int tileSizeX, int tileSizeY): this(0, 0, tileSizeX, tileSizeY)
        {
            
        }
        
        protected AStarGridBase(int offsetX, int offsetY, int tileSizeX, int tileSizeY)
        {
            this.directionsOfNeighbours = new List<Direction>();
            this.openNodes = new PriorityQueue<float, Position2D>(4096);
            this.nodes = new DynamicDataView2D<AStarNode>(offsetX, offsetY, tileSizeX, tileSizeY);
        }

        protected abstract void PopulateDirections(Position2D basePosition, List<Direction> buffer);
        protected abstract bool EdgeCostInformation(in Position2D sourceNode, in Direction d, float sourceNodeCost, out float totalPathCost);

        protected abstract bool IsTargetNode(in Position2D pos);

        protected abstract float Heuristic(in Position2D pos);

        protected virtual List<Position2D> FindPath(Position2D start, Position2D end, List<Position2D> pathBuffer)
        {
            if (pathBuffer == null)
            {
                pathBuffer = new List<Position2D>();
            }
            else
            {
                pathBuffer.Clear();
            }

            if (IsTargetNode(start))
            {
                return pathBuffer;
            }

            nodes.ClearData();

            var startNode = AStarNode.Start(Heuristic(start));
            nodes[start] = startNode;
            openNodes.Enqueue(start, startNode.HeuristicValue);

            while (openNodes.Count != 0)
            {
                var currentPosition = openNodes.Dequeue();
                var currentNode = nodes[currentPosition];

                nodes[currentPosition] = currentNode.Close();

                if (IsTargetNode(currentPosition)) // We found the end, cleanup and return the path
                {
                    return ProduceResult(currentPosition, currentNode, pathBuffer);
                }

                if (currentNode.DistanceFromStart == ushort.MaxValue)
                {
                    continue;
                }
                
                PopulateDirections(currentPosition, directionsOfNeighbours);
                foreach (var dir in directionsOfNeighbours)
                {
                    var neighborPos = currentPosition + dir;
                    var neighbor = nodes[neighborPos];
                    if (neighbor.IsClosed())
                    {
                        // This neighbor has already been evaluated at shortest possible path, don't re-add
                        continue;
                    }

                    if (!EdgeCostInformation(in currentPosition, in dir, currentNode.AccumulatedCost, out var totalPathCost))
                    {
                        continue;
                    }

                    var isNeighborOpen = neighbor.State == AStarNode.NodeState.Open;
                    if (isNeighborOpen && IsExistingResultBetter(in neighborPos, in totalPathCost))
                    {
                        // Not a better path
                        continue;
                    }

                    // We found a best path, so record and update
                    nodes[neighborPos] = new AStarNode(AStarNode.NodeState.Open,
                                                       totalPathCost + Heuristic(in neighborPos),
                                                       totalPathCost,
                                                       Directions.GetDirection(currentPosition, neighborPos),
                                                       (ushort)(currentNode.DistanceFromStart + 1)
                    );

                    openNodes.UpdatePriority(neighborPos, neighbor.HeuristicValue);
                }
            }

            openNodes.Clear();
            return pathBuffer;
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

        List<Position2D> ProduceResult(Position2D currentPosition, AStarNode currentNode, List<Position2D> pathBuffer)
        {
            pathBuffer.Capacity = Math.Max(pathBuffer.Capacity, currentNode.DistanceFromStart);
            while (currentNode.DirectionToParent != Direction.None)
            {
                pathBuffer.Add(currentPosition);
                currentPosition -= currentNode.DirectionToParent.ToCoordinates();
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

            return pathBuffer;
        }
    }
}