using System.Collections.Generic;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Utils.DataViews;
using Serilog;

namespace RogueEntity.Core.Utils.Algorithms
{
    /// <summary>
    ///   Needs 5 bytes per node processed.
    /// </summary>
    public abstract class DynamicDijkstraGridBase
    {
        static readonly ILogger Logger = SLog.ForContext<DynamicDijkstraGridBase>();

        readonly IDynamicDataView2D<float> nodesWeight;
        readonly IDynamicDataView2D<Direction> nodesDirection;
        readonly PriorityQueue<DijkstraNodeWeight, Position2D> openNodes;
        readonly List<Direction> directions;

        protected DynamicDijkstraGridBase(IBoundedDataViewPool<float> weightPool, 
                                          IBoundedDataViewPool<Direction> directionPool)
        {
            this.directions = new List<Direction>();
            this.openNodes = new PriorityQueue<DijkstraNodeWeight, Position2D>(4096);
            this.nodesWeight = new PooledDynamicDataView2D<float>(weightPool);
            this.nodesDirection = new PooledDynamicDataView2D<Direction>(directionPool);
        }

        protected void PrepareScan()
        {
            openNodes.Clear();
            nodesDirection.Clear();
            nodesWeight.Clear();
        }

        protected void RescanMap()
        {
            RescanMap(out _);
        }

        public bool TryGetCumulativeCost(in Position2D pos, out float result)
        {
            return nodesWeight.TryGet(pos.X, pos.Y, out result);
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

                if (!nodesWeight.TryGet(openNodePosition.X, openNodePosition.Y, out var openNodeWeight))
                {
                    continue;
                }

                nodeCount += 1;

                PopulateDirections(openNodePosition, directions);
                foreach (var d in directions)
                {
                    var nextNodePos = openNodePosition + d;

                    if (!EdgeCostInformation(in openNodePosition, in d, openNodeWeight, out var totalPathCost))
                    {
                        continue;
                    }

                    if (IsExistingResultBetter(in nextNodePos, in totalPathCost))
                    {
                        // already visited.
                        continue;
                    }

                    EnqueueNode(in nextNodePos, totalPathCost, openNodePosition);
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
            nodesWeight[c.X, c.Y] = weight;
            nodesDirection[c.X, c.Y] = Direction.None;
        }

        protected void EnqueueNode(in Position2D pos, float weight, in Position2D prev)
        {
            openNodes.UpdatePriority(pos, new DijkstraNodeWeight(-weight, (float)DistanceCalculation.Euclid.Calculate2D(pos, prev)));
            nodesWeight[pos.X, pos.Y] = weight;
            nodesDirection[pos.X, pos.Y] = Directions.GetDirection(prev, pos);
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

            if (!nodesWeight.TryGet(coord.X, coord.Y, out var existingWeight))
            {
                // Stop processing nodes once we left the valid map area.
                return true;
            }

            return existingWeight >= newWeight;
        }

        public bool TryGetPreviousStep(in Position2D pos, out Position2D nextStep)
        {
            if (!nodesDirection.TryGet(pos.X, pos.Y, out var prevIdx) ||
                prevIdx == Direction.None)
            {
                nextStep = default;
                return false;
            }

            nextStep = pos - prevIdx.ToCoordinates();
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
            if (!nodesWeight.TryGet(p.X, p.Y, out goalStrength))
            {
                return pathAccumulator;
            }

            pathAccumulator.Add(p);

            while (pathAccumulator.Count < maxLength &&
                   TryGetPreviousStep(in p, out p) &&
                   nodesWeight.TryGet(p.X, p.Y, out goalStrength))
            {
                pathAccumulator.Add(p);
            }

            return pathAccumulator;
        }
    }
}