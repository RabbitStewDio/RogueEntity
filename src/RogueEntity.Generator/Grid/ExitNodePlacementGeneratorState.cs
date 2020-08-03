using System;
using System.Collections.Generic;
using ValionRL.Core.MapFragments;

namespace ValionRL.Core.Generator
{
    public class ExitNodePlacementGeneratorState<TGameContext> : GeneratorStateBase<TGameContext> 
    {
        public ExitNodePlacementGeneratorState(PopulatedTilesGeneratorState<TGameContext> copy) : base(copy)
        {
        }

        public Node StartNode => Nodes[GridStartPosition.X, GridStartPosition.Y] ?? throw new InvalidOperationException();

        public bool PlaceExitNode()
        {
            var gs = Config;
            var candidateSet = BuildCandidateSet();
            if (candidateSet.Count == 0)
            {
                return false;
            }

            var maxDistance = candidateSet[0].DistanceFromStart;


            // Attempt to place an exit at the most remote location we can find. 
            // First we try to do it naturally, by letting Mr. Random have a shot.
            // Try to find a perfect match.
            foreach (var n in candidateSet)
            {
                // Try to find an exit node that exactly matches the existing connections for the given tile.
                if (gs.ExitZones.TrySelectReplacement(RandomGenerator, out var exit, n.Connections.Connectivity))
                {
                    ForcePlaceNode(MapFragmentPlacement.ToPlacementTemplate(exit.Info), n.GridPosition.X, n.GridPosition.Y, n.DistanceFromStart - 1, out var nx);
                    nx.SelectedTile = exit;
                    return true;
                }
            }


            // Desperate times: Try to find a approximate match. 
            // As this operation can disrupt the dungeon, we have to try to minimize the damage we do here. 
            // We simulate an entry, then floodfill the dungeon from the start to see which nodes are left.
            // Those nodes would have been disconnected by this hack. Count them to find the insert point
            // that does minimum damage (as we have to remove disconnected nodes).
            var floodState = new FloodFillState(Width, Height);
            var bestWeight = int.MaxValue;
            var bestPos = Coord.NONE;
            var bestSelection = default(MapFragment);

            foreach (var n in candidateSet)
            {
                // try to find a placement that at least partially matches the defined exits for the 
                // already given node. The actually used exits for the selected might not actually 
                // connect back to the start point, but we deal with that later...
                if (!gs.ExitZones.TrySelectForPlacement(RandomGenerator, out var exit, default, n.Connections.Connectivity))
                {
                    continue;
                }

                // Validate that the exit we want to place has a connection to the start position.
                // If there is no connection, the player could never enter the exit room, and would
                // be locked in the current level forever.
                if (!floodState.FloodFill(this, n.GridPosition, n.Connections.Connectivity, out var result))
                {
                    continue;
                }

                // We try to find an exit that does not cut off too much of the dungeon's rooms.
                // We also don't want to place an exit to close to the starting point. 
                // Calculate a placement score that takes both measures into account. 
                // Element A: Square of the 'lost distance from start' punishes nodes that are to close to the start.
                // Element B: 'Lost nodes' rewards not loosing to many cells in the exit placement.
                var weight = Math.Pow(maxDistance - n.DistanceFromStart, 2) + (candidateSet.Count - result); 
                if (weight < bestWeight)
                {
                    bestWeight = result;
                    bestPos = n.GridPosition;
                    bestSelection = exit;
                }
            }

            if (bestWeight == int.MaxValue)
            {
                // nothing left to do. There was not a single valid placement anywhere.
                return false;
            }

            ForcePlaceNode(MapFragmentPlacement.ToPlacementTemplate(bestSelection.Info), bestPos.X, bestPos.Y, 0, out var node);
            node.SelectedTile = bestSelection;
            return false;
        }

        List<Node> BuildCandidateSet()
        {
            var candidateSet = new List<Node>();
            foreach (var n in Nodes)
            {
                // excludes empty and the start node.
                if (n != null && n.DistanceFromStart > 1)
                {
                    candidateSet.Add(n);
                }
            }

            candidateSet.Sort((n1, n2) => -n1.DistanceFromStart.CompareTo(n2.DistanceFromStart));
            return candidateSet;
        }

        public void ForcePlaceNode(MapFragmentPlacement mf, int x, int y, int sourceDistanceFromStart, out Node node)
        {
            node = Nodes[x, y];
            if (node != null)
            {
                // reset edges back to Unset.
                foreach (var t in mf.Connectivity.Targets(x, y))
                {
                    var tNode = Nodes[t.c.X, t.c.Y];
                    if (tNode == null)
                    {
                        continue;
                    }

                    var edge = new Edge<Node>(tNode, t.d.DeltaY == 0);
                    tNode[t.d.Invert()] = edge;
                }
            }

            PlaceNodeInternal(mf, x, y, sourceDistanceFromStart, out node);
        }


    }
}