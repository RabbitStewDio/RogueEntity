using System.Collections.Generic;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Utils.DataViews;
using Serilog;

namespace RogueEntity.Core.Utils.Algorithms
{
    public abstract class DynamicDijkstraGridBase
    {
        static readonly ILogger Logger = SLog.ForContext<DynamicDijkstraGridBase>();

        readonly struct DijkstraNode
        {
            public readonly Direction DirectionToOrigin;
            public readonly float Weight;

            public DijkstraNode(Direction directionToOrigin, float weight)
            {
                DirectionToOrigin = directionToOrigin;
                Weight = weight;
            }
        }

        readonly DynamicDataView2D<DijkstraNode> nodes;
        readonly PriorityQueue<DijkstraNodeWeight, Position2D> openNodes;
        readonly List<Direction> directions;

        protected DynamicDijkstraGridBase(int tileSizeX, int tileSizeY) : this(0, 0, tileSizeX, tileSizeY)
        {
        }

        protected DynamicDijkstraGridBase(int offsetX, int offsetY, int tileSizeX, int tileSizeY)
        {
            this.directions = new List<Direction>();
            this.openNodes = new PriorityQueue<DijkstraNodeWeight, Position2D>(4096);
            this.nodes = new DynamicDataView2D<DijkstraNode>(offsetX, offsetY, tileSizeX, tileSizeY);
        }

        protected void PrepareScan()
        {
            openNodes.Clear();
            nodes.ClearData();
        }

        protected void RescanMap()
        {
            RescanMap(out _);
        }

        public bool TryGetCumulativeCost(in Position2D pos, out float result)
        {
            if (nodes.TryGet(pos.X, pos.Y, out var resultNode))
            {
                result = resultNode.Weight;
                return true;
            }

            result = default;
            return false;
        }

        protected abstract void PopulateDirections(Position2D basePosition, List<Direction> buffer);

        protected bool RescanMap(out Position2D lowestNode,
                                 int maxSteps = int.MaxValue)
        {
            Position2D openNodePosition = default;
            var nodeCount = 0;
            while (nodeCount < maxSteps && TryDequeueOpenNode(out openNodePosition))
            {
                if (IsTarget(in openNodePosition))
                {
                    lowestNode = openNodePosition;
                    return true;
                }

                if (!nodes.TryGet(openNodePosition.X, openNodePosition.Y, out var openNode))
                {
                    continue;
                }

                nodeCount += 1;

                PopulateDirections(openNodePosition, directions);
                foreach (var d in directions)
                {
                    var nextNodePos = openNodePosition + d;

                    if (!EdgeCostInformation(in openNodePosition, in d, openNode.Weight, out var cost))
                    {
                        continue;
                    }

                    var newWeight = openNode.Weight - cost;
                    if (IsExistingResultBetter(in nextNodePos, in newWeight))
                    {
                        // already visited.
                        continue;
                    }

                    EnqueueNode(in nextNodePos, newWeight, openNodePosition);
                }
            }

            lowestNode = openNodePosition;
            Logger.Verbose("Evaluated {Count} nodes during rescan.", nodeCount);
            return nodeCount > 0;
        }

        protected abstract bool EdgeCostInformation(in Position2D sourceNode, in Direction d, float sourceNodeCost, out float totalPathCost);

        protected virtual bool IsTarget(in Position2D openNodePosition)
        {
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="c"></param>
        /// <param name="weight">should be a positive value</param>
        protected void EnqueueStartingNode(in Position2D c, float weight)
        {
            openNodes.UpdatePriority(c, new DijkstraNodeWeight(-weight, 0));
            nodes[c] = new DijkstraNode(Direction.None, weight);
        }

        protected void EnqueueNode(in Position2D pos, float weight, in Position2D prev)
        {
            openNodes.UpdatePriority(pos, new DijkstraNodeWeight(-weight, (float)DistanceCalculation.Euclid.Calculate(pos, prev)));
            nodes[pos] = new DijkstraNode(Directions.GetDirection(prev, pos), weight);
        }

        protected bool TryDequeueOpenNode(out Position2D c)
        {
            if (openNodes.Count == 0)
            {
                c = default;
                return false;
            }

            c = openNodes.Dequeue();
            return true;
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

            return existingWeight.Weight >= newWeight;
        }

        public bool TryGetPreviousStep(in Position2D pos, out Position2D nextStep)
        {
            if (!nodes.TryGet(pos.X, pos.Y, out var prevIdx) ||
                prevIdx.DirectionToOrigin == Direction.None)
            {
                nextStep = default;
                return false;
            }

            nextStep = pos - prevIdx.DirectionToOrigin.ToCoordinates();
            return true;
        }

        /// <summary>
        ///   Returns a path from the current position to the nearest goal. 
        /// </summary>
        /// <param name="p"></param>
        /// <param name="goalStrength"></param>
        /// <param name="maxLength"></param>
        /// <returns></returns>
        protected List<Position2D> FindPath(Position2D p,
                                            out float goalStrength,
                                            int maxLength = int.MaxValue)
        {
            var pathAccumulator = new List<Position2D>();
            if (!nodes.TryGet(p.X, p.Y, out var node))
            {
                goalStrength = 0;
                return pathAccumulator;
            }

            goalStrength = node.Weight;
            pathAccumulator.Add(p);

            while (pathAccumulator.Count < maxLength &&
                   TryGetPreviousStep(in p, out p) &&
                   nodes.TryGet(p.X, p.Y, out node))
            {
                goalStrength = node.Weight;
                pathAccumulator.Add(p);
            }

            return pathAccumulator;
        }
    }
}