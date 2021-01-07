using System;
using System.Collections.Generic;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.DataViews;
using Serilog;

namespace RogueEntity.Core.Positioning.Algorithms
{
    /// <summary>
    ///   This Dijkstra grid graph allows you to find all possible paths to a set of
    ///   start nodes in a dense array. This is suitable for search tasks that cover
    ///   a known and rather small search area. 
    ///
    ///   Remember that in a Dijkstra search the search starts from all start nodes
    ///   (the target of your path) towards all target nodes at once (or all cells, if no
    ///   explicit target nodes are used). If performing classic pathfinding, set
    ///   your movement targets as start nodes, and once finished attempt to compute
    ///   a path from your current position.
    ///
    ///   The start nodes must be enqueued as starting node with a positive
    ///   weight before use. The weight indirectly defines a maximum path length of
    ///   any path leading to that target.
    ///
    ///   If you want to guarantee that all cells contain pathing information, make sure
    ///   that your weights for each target are greater than the number of cells in the
    ///   search area.
    /// </summary>
    public abstract class DijkstraGridBase<TExtraNodeInfo>
    {
        static readonly ILogger Logger = SLog.ForContext<DijkstraGridBase<TExtraNodeInfo>>();

        Rectangle bounds;
        readonly BoundedDataView<Direction> resultMapCoords;
        readonly BoundedDataView<float> resultMapDistanceCost;
        readonly PriorityQueue<DijkstraNodeWeight, ShortPosition2D> openNodes;

        protected DijkstraGridBase(in Rectangle bounds)
        {
            this.openNodes = new PriorityQueue<DijkstraNodeWeight, ShortPosition2D>(Math.Max(16, bounds.Width * bounds.Height));
            this.resultMapCoords = new BoundedDataView<Direction>(in bounds);
            this.resultMapDistanceCost = new BoundedDataView<float>(in bounds);
            this.bounds = bounds;
        }

        protected virtual void Resize(in Rectangle newBounds)
        {
            if (newBounds.Width <= bounds.Width &&
                newBounds.Height <= bounds.Height)
            {
                // reorient, not resize.
                this.bounds = new Rectangle(newBounds.MinExtentX, newBounds.MinExtentY, bounds.Width, bounds.Height);
            }
            else
            {
                this.bounds = new Rectangle(newBounds.MinExtentX, newBounds.MinExtentY,
                                            Math.Max(newBounds.Width, bounds.Width),
                                            Math.Max(newBounds.Height, bounds.Height));
                this.openNodes.Resize(Math.Max(openNodes.Capacity, bounds.Width * bounds.Height));
            }

            this.resultMapCoords.Resize(in bounds);
            this.resultMapDistanceCost.Resize(in bounds);
        }

        public Rectangle Bounds => bounds;

        protected void PrepareScan()
        {
            openNodes.Clear();
            resultMapCoords.Clear();
            resultMapDistanceCost.Clear();
        }

        public bool TryGetCumulativeCost(in ShortPosition2D pos, out float result)
        {
            return resultMapDistanceCost.TryGet(in pos, out result);
        }

        protected abstract ReadOnlyListWrapper<Direction> PopulateTraversableDirections(ShortPosition2D basePosition);

        protected bool RescanMap(int maxSteps = int.MaxValue)
        {
            var nodeCount = 0;
            while (nodeCount < maxSteps)
            {
                if (!TryDequeueOpenNode(out var openNodePosition))
                {
                    break;
                }

                if (!resultMapDistanceCost.TryGet(openNodePosition, out var openNodeWeight))
                {
                    continue;
                }

                nodeCount += 1;

                var directions = PopulateTraversableDirections(openNodePosition);
                foreach (var d in directions)
                {
                    var nextNodePos = openNodePosition + d;

                    if (!resultMapCoords.Contains(nextNodePos.X, nextNodePos.Y))
                    {
                        continue;
                    }

                    if (!EdgeCostInformation(in openNodePosition, in d, openNodeWeight, out var totalPathCost, out TExtraNodeInfo ni))
                    {
                        continue;
                    }

                    // var newWeight = openNodeWeight - totalPathCost;
                    if (IsExistingResultBetter(in nextNodePos, in totalPathCost))
                    {
                        // already visited.
                        continue;
                    }

                    EnqueueNode(in nextNodePos, totalPathCost, openNodePosition);
                    UpdateNode(nextNodePos, ni);
                }
            }

            Logger.Verbose("Evaluated {Count} nodes during rescan.", nodeCount);
            NodesEvaluated = nodeCount;
            return nodeCount > 0;
        }

        public int NodesEvaluated { get; private set; }

        protected abstract void UpdateNode(in ShortPosition2D nextNodePos, TExtraNodeInfo nodeInfo);

        protected abstract bool EdgeCostInformation(in ShortPosition2D sourceNode, in Direction d, float sourceNodeCost, out float totalPathCost, out TExtraNodeInfo nodeInfo);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="c"></param>
        /// <param name="weight">should be a positive value</param>
        protected void EnqueueStartingNode(in ShortPosition2D c, float weight)
        {
            if (resultMapCoords.TryGetRawIndex(in c, out _))
            {
                openNodes.UpdatePriority(c, new DijkstraNodeWeight(-weight, 0));
                resultMapCoords[c] = 0;
                resultMapDistanceCost[c] = weight;
            }
        }

        protected void EnqueueNode(in ShortPosition2D pos, float weight, in ShortPosition2D prev)
        {
            if (resultMapCoords.TryGetRawIndex(pos.X, pos.Y, out _))
            {
                openNodes.UpdatePriority(pos, new DijkstraNodeWeight(-weight, (float)DistanceCalculation.Euclid.Calculate2D(pos, prev)));
                resultMapDistanceCost[pos.X, pos.Y] = weight;
                resultMapCoords[pos] = Directions.GetDirection(prev, pos);
            }
        }

        protected bool TryDequeueOpenNode(out ShortPosition2D c)
        {
            if (openNodes.Count == 0)
            {
                c = default;
                return false;
            }

            c = openNodes.Dequeue();
            return true;
        }

        bool IsExistingResultBetter(in ShortPosition2D coord, in float newWeight)
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

            if (!resultMapDistanceCost.TryGet(in coord, out var existingWeight))
            {
                // Stop processing nodes once we left the valid map area.
                return true;
            }

            return existingWeight >= newWeight;
        }

        protected bool TryGetPreviousStep(in ShortPosition2D pos, out ShortPosition2D nextStep)
        {
            if (!resultMapCoords.TryGet(pos, out var prevIdx) || prevIdx == Direction.None)
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
        /// <param name="pathAccumulator"></param>
        /// <param name="maxLength"></param>
        /// <returns></returns>
        protected BufferList<ShortPosition2D> FindPath(ShortPosition2D p,
                                                       out float goalStrength,
                                                       BufferList<ShortPosition2D> pathAccumulator = null,
                                                       int maxLength = int.MaxValue)
        {
            pathAccumulator = BufferList.PrepareBuffer(pathAccumulator);

            if (!resultMapDistanceCost.TryGet(p, out goalStrength))
            {
                return pathAccumulator;
            }

            pathAccumulator.Add(p);

            while (pathAccumulator.Count < maxLength &&
                   TryGetPreviousStep(in p, out p) &&
                   resultMapDistanceCost.TryGet(p, out goalStrength))
            {
                pathAccumulator.Add(p);
            }

            return pathAccumulator;
        }
    }
}
