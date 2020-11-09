using System;
using System.Collections.Generic;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Utils.DataViews;
using Serilog;

namespace RogueEntity.Core.Utils.Algorithms
{
    /// <summary>
    ///   This Dijkstra grid graph allows you to find all possible paths to a set of
    ///   start nodes.
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
    public abstract class DijkstraGridBase
    {
        /// <summary>
        ///   A weight that adds a small preference for cardinal directions to the
        ///   algorithm result without interfering with the real weight system.
        ///
        ///   This makes the resulting paths a lot more human if the weight system
        ///   uses zero-costs and the adjacency rule also uses equal cost movements
        ///   (Cheby, I'm looking at you here!). 
        /// </summary>
        readonly struct WeightedEuclid : IComparable<WeightedEuclid>, IComparable
        {
            readonly float weight;
            readonly float euclidHint;

            public WeightedEuclid(float weight, float euclidHint)
            {
                this.weight = weight;
                this.euclidHint = euclidHint;
            }

            public int CompareTo(WeightedEuclid other)
            {
                var weightComparison = weight.CompareTo(other.weight);
                if (weightComparison != 0)
                {
                    return weightComparison;
                }

                return euclidHint.CompareTo(other.euclidHint);
            }

            public int CompareTo(object obj)
            {
                if (ReferenceEquals(null, obj))
                {
                    return 1;
                }

                return obj is WeightedEuclid other ? CompareTo(other) : throw new ArgumentException($"Object must be of type {nameof(WeightedEuclid)}");
            }

            public static bool operator <(WeightedEuclid left, WeightedEuclid right)
            {
                return left.CompareTo(right) < 0;
            }

            public static bool operator >(WeightedEuclid left, WeightedEuclid right)
            {
                return left.CompareTo(right) > 0;
            }

            public static bool operator <=(WeightedEuclid left, WeightedEuclid right)
            {
                return left.CompareTo(right) <= 0;
            }

            public static bool operator >=(WeightedEuclid left, WeightedEuclid right)
            {
                return left.CompareTo(right) >= 0;
            }
        }
        
        static readonly ILogger Logger = SLog.ForContext<DijkstraGridBase>();

        Rectangle bounds;
        readonly BoundedDataView<int> resultMapCoords;
        readonly BoundedDataView<float> resultMapDistanceCost;
        readonly IntPriorityQueue<WeightedEuclid> openNodes;
        readonly List<Direction> directions;

        protected DijkstraGridBase(in Rectangle bounds)
        {
            this.bounds = bounds;

            this.directions = new List<Direction>();
            this.openNodes = new IntPriorityQueue<WeightedEuclid>(bounds.Width * bounds.Height);
            this.resultMapCoords = new BoundedDataView<int>(in bounds);
            this.resultMapDistanceCost = new BoundedDataView<float>(in bounds);
        }

        public Rectangle Bounds => bounds;

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
                this.openNodes.Resize(bounds.Width * bounds.Height);
            }
            
            this.resultMapCoords.Resize(in bounds);
            this.resultMapDistanceCost.Resize(in bounds);
        }
        
        protected void PrepareScan()
        {
            openNodes.Clear();
            resultMapCoords.Clear();
            resultMapDistanceCost.Clear();
        }

        protected void RescanMap()
        {
            RescanMap(out _);
        }

        public bool TryGetCumulativeCost(in Position2D pos, out float result)
        {
            return resultMapDistanceCost.TryGet(in pos, out result);
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

                if (!resultMapDistanceCost.TryGet(openNodePosition, out var openNodeWeight))
                {
                    continue;
                }

                nodeCount += 1;

                PopulateDirections(openNodePosition, directions);
                foreach (var d in directions)
                {
                    var nextNodePos = openNodePosition + d;

                    if (!IsValidNode(in nextNodePos))
                    {
                        continue;
                    }

                    if (!EdgeCostInformation(in openNodePosition, in d, openNodeWeight, out var cost))
                    {
                        continue;
                    }

                    var newWeight = openNodeWeight - cost;
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

        protected abstract bool EdgeCostInformation(in Position2D sourceNode, in Direction d, float sourceNodeCost, out float cost);
        
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
            if (resultMapCoords.TryGetRawIndex(in c, out var idx))
            {
                openNodes.UpdatePriority(idx, new WeightedEuclid(-weight, 0));
                resultMapCoords[c] = 0;
                resultMapDistanceCost[c] = weight;
            }
        }

        protected void EnqueueNode(in Position2D pos, float weight, in Position2D prev)
        {
            if (resultMapCoords.TryGetRawIndex(in pos, out var idx))
            {
                openNodes.UpdatePriority(idx, new WeightedEuclid(-weight, (float)DistanceCalculation.Euclid.Calculate(pos, prev)));
                resultMapDistanceCost[pos] = weight;

                if (resultMapCoords.TryGetRawIndex(in prev, out var prevIdx))
                {
                    resultMapCoords[pos] = 1 + prevIdx;
                }
            }
        }

        protected bool TryDequeueOpenNode(out Position2D c)
        {
            if (openNodes.Count == 0)
            {
                c = default;
                return false;
            }

            var idx = openNodes.Dequeue();
            return resultMapCoords.TryGetFromRawIndex(idx, out c);
        }

        protected bool IsValidNode(in Position2D nextNodePos)
        {
            return resultMapCoords.TryGetRawIndex(in nextNodePos, out _);
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

            if (!resultMapDistanceCost.TryGet(in coord, out var existingWeight))
            {
                // Stop processing nodes once we left the valid map area.
                return true;
            }
           
            return existingWeight >= newWeight;

        }
        
        public bool TryGetPreviousStep(in Position2D pos, out Position2D nextStep)
        {
            if (!resultMapCoords.TryGet(pos, out var prevIdx) || prevIdx <= 0)
            {
                nextStep = default;
                return false;
            }

            return resultMapCoords.TryGetFromRawIndex(prevIdx - 1, out nextStep);
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
/*
        public override string ToString()
        {
            var b = new StringBuilder();
            for (var y = 0; y < Height; y += 1)
            {
                for (var x = 0; x < Width; x += 1)
                {
                    if (x > 0)
                    {
                        b.Append("  ");
                    }

                    var v = this[x, y];
                    if (!v.ParentNode.TryGetValue(out var p))
                    {
                        b.Append($"{v.WeightHint,4:0.0}:{"-",-10}");
                    }
                    else
                    {
                        var d = Directions.GetDirection(new Coord(x, y), p);
                        b.Append($"{v.WeightHint,4:0.0}:{d,-10}");
                    }
                }

                b.Append('\n');
            }

            return b.ToString();
        }
        */
    }
}