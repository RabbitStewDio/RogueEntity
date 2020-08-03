using System;
using System.Collections.Generic;
using ValionRL.Core.MapFragments;

namespace ValionRL.Core.Generator
{
    public class FloodFillState
    {
        readonly bool[,] seenNodes;
        readonly Queue<Node> openNodes;

        public FloodFillState(int w, int h)
        {
            seenNodes = new bool[w, h];
            openNodes = new Queue<Node>(w * h);
        }

        void Reset()
        {
            Array.Clear(seenNodes, 0, seenNodes.Length);
            openNodes.Clear();
        }

        public bool FloodFill<TGameContext>(ExitNodePlacementGeneratorState<TGameContext> rs,
                                            Coord replacementPos,
                                            MapFragmentConnectivity replacementConnections,
                                            out int validNodes) 
        {
            Reset();

            var start = rs.StartNode;
            openNodes.Enqueue(start);
            seenNodes[start.GridPosition.X, start.GridPosition.Y] = true;

            var foundPath = DoFill(rs.Nodes, replacementPos, replacementConnections);

            if (!foundPath)
            {
                validNodes = 0;
                return false;
            }

            int filled = 0;
            foreach (var n in seenNodes)
            {
                filled += n ? 1 : 0;
            }

            validNodes = filled;
            return true;
        }

        bool DoFill(Node[,] nodes,
                    Coord replacementPos,
                    MapFragmentConnectivity replacementConnections)
        {
            bool foundPath = false;
            while (openNodes.TryDequeue(out var node))
            {
                var connectivity = node.Connections;
                foreach (var (_, (x, y)) in connectivity.Connectivity.Targets(node.GridPosition.X, node.GridPosition.Y))
                {
                    if (seenNodes[x, y])
                    {
                        continue;
                    }

                    if (x == replacementPos.X && y == replacementPos.Y)
                    {
                        foundPath |= replacementConnections.CanConnectTo(connectivity.Connectivity);
                        continue;
                    }

                    var candidateNode = nodes[x, y];
                    if (candidateNode == null)
                    {
                        continue;
                    }

                    openNodes.Enqueue(candidateNode);
                    seenNodes[x, y] = true;
                }
            }

            return foundPath;
        }
    }
}