using System.Collections.Generic;
using RogueEntity.Core.Utils;
using RogueEntity.Generator.MapFragments;
using ValionRL.Core.MapFragments;

namespace ValionRL.Core.Generator
{
    public class Node
    {
        public readonly MapFragmentPlacement Connections;
        public Optional<MapFragment> SelectedTile;
        public readonly Coord GridPosition;
        readonly Dictionary<Direction, Edge<Node>> edges;
        public int DistanceFromStart;

        public Edge<Node> this[Direction d]
        {
            get
            {
                if (edges.TryGetValue(d, out var v))
                {
                    return v;
                }

                return default;
            }
            set
            {
                edges[d] = value;
            }
        }

        public Node(MapFragmentPlacement tile, int x, int y, int distanceFromStart)
        {
            edges = new Dictionary<Direction, Edge<Node>>();

            Connections = tile;
            GridPosition = new Coord(x, y);
            DistanceFromStart = distanceFromStart;
        }

        public override string ToString()
        {
            return $"Node({GridPosition}, {Connections})";
        }
    }
}