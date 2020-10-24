using System;
using System.Collections.Generic;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Utils.Maps;
using Serilog;

namespace RogueEntity.Core.Utils.Algorithms
{
    public delegate bool DijkstraCostDelegate(in Position2D origin, in Direction d, out float cost);

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
        static readonly ILogger Logger = SLog.ForContext<DijkstraGridBase>();

        Rectangle bounds;
        readonly BoundedDataView<int> resultMapCoords;
        readonly BoundedDataView<float> resultMapDistanceCost;
        readonly IntPriorityQueue openNodes;

        protected DijkstraGridBase(in Rectangle bounds)
        {
            this.bounds = bounds;

            this.openNodes = new IntPriorityQueue(bounds.Width * bounds.Height);
            this.resultMapCoords = new BoundedDataView<int>(in bounds);
            this.resultMapDistanceCost = new BoundedDataView<float>(in bounds);
        }

        public Rectangle Bounds => bounds;

        protected abstract ReadOnlyListWrapper<Direction> AdjacencyRule { get; }

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

                foreach (var d in AdjacencyRule)
                {
                    var nextNodePos = openNodePosition + d;

                    if (!IsValidNode(in nextNodePos))
                    {
                        continue;
                    }

                    if (!EdgeCostInformation(in openNodePosition, in d, openNodeWeight, out var cost) ||
                        cost == 0)
                    {
                        continue;
                    }

                    var newWeight = openNodeWeight - cost;
                    if (IsExistingResultBetter(in nextNodePos, in newWeight))
                    {
                        // already visited.
                        continue;
                    }
                    
                    Console.WriteLine("Process Node " + nextNodePos);
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
                openNodes.UpdatePriority(idx, -weight);
                resultMapCoords[c] = 0;
                resultMapDistanceCost[c] = weight;
            }
        }

        protected void EnqueueNode(in Position2D pos, float weight, in Position2D prev)
        {
            if (resultMapCoords.TryGetRawIndex(in pos, out var idx))
            {
                openNodes.UpdatePriority(idx, -weight);
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
            if (newWeight <= 0)
            {
                // All nodes are initialized with a weight of zero at the start.
                // This allows us to use Array.Clear which uses MemSet to efficiently
                // erase the memory.
                //
                // Stop processing nodes once we hit the lower bounds of the weight values.
                return true;
            }

            if (!resultMapDistanceCost.TryGet(in coord, out var weight))
            {
                // Stop processing nodes once we left the valid map area.
                return true;
            }

            return newWeight <= weight;

        }
        
        public bool TryGetPreviousStep(in Position2D pos, out Position2D nextStep)
        {
            if (!resultMapCoords.TryGet(pos, out var prevIdx) ||
                prevIdx <= 0)
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