using System;
using System.Collections.Generic;
using RogueEntity.Core.Infrastructure.Positioning.Grid;

namespace ValionRL.Core.Generator
{
    public class GeneratorStateBase<TGameContext> 
    {
        public readonly GeneratorConfig<TGameContext> Config;
        public readonly Func<double> RandomGenerator;
        public readonly EntityGridPosition StartingMapPosition;
        public readonly Coord GridStartPosition;
        public readonly int Width;
        public readonly int Height;
        public readonly Node[,] Nodes;
        public readonly Queue<Node> OpenList;
        public readonly Queue<PendingPlacementRecord> PendingList;

        protected GeneratorStateBase(GeneratorStateBase<TGameContext> copy)
        {
            this.Config = copy.Config;
            this.RandomGenerator = copy.RandomGenerator;
            this.GridStartPosition = copy.GridStartPosition;
            this.Width = copy.Width;
            this.Height = copy.Height;
            this.Nodes = copy.Nodes;
            this.OpenList = copy.OpenList;
            this.PendingList = copy.PendingList;
            this.StartingMapPosition = copy.StartingMapPosition;
        }

        protected GeneratorStateBase(GeneratorConfig<TGameContext> config,
                                     Func<double> randomGenerator,
                                     EntityGridPosition startingPosition, 
                                     Coord gridStartPosition)
        {
            this.Config = config;
            this.PendingList = new Queue<PendingPlacementRecord>();

            this.RandomGenerator = randomGenerator;
            this.StartingMapPosition = startingPosition;
            this.GridStartPosition = gridStartPosition;

            var width = config.GridWidth;
            var height = config.GridHeight;
            this.Nodes = new Node[width, height];
            this.OpenList = new Queue<Node>();
            this.Width = width;
            this.Height = height;
        }

        protected void PlaceNodeInternal(MapFragmentPlacement mf, int x, int y, int sourceDistanceFromStart, out Node node)
        {
            var dist = sourceDistanceFromStart;
            node = new Node(mf, x, y, sourceDistanceFromStart + 1);
            foreach (var t in mf.Connectivity.Targets(x, y))
            {
                var tNode = Nodes[t.c.X, t.c.Y];
                if (tNode == null)
                {
                    node[t.d] = new Edge<Node>(node, t.d.IsHorizontal());
                    continue;
                }

                // Other node has more weight
                if (tNode.DistanceFromStart <= dist)
                {
                    var edge = tNode[t.d.Invert()];
                    if (!edge.ValidSource())
                    {
                        // no connection. Probably an error.
                        throw new InvalidOperationException("Edge from node " + t.c + " along " + t.d.Invert() + " has no defined source node.");
                    }

                    edge = edge.WithTarget(node);
                    tNode[t.d.Invert()] = edge;
                    node[t.d] = edge;
                    dist = tNode.DistanceFromStart;
                }
                else
                {
                    var edge = new Edge<Node>(node, t.d.IsHorizontal());
                    node[t.d] = edge;
                    tNode[t.d.Invert()] = edge;
                }
            }

            node.DistanceFromStart = dist + 1;
            Nodes[x, y] = node;
        }

    }
}