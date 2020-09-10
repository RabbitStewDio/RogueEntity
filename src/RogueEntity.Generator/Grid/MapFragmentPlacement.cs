using System;
using System.Text;
using RogueEntity.Core.Utils;
using RogueEntity.Generator.MapFragments;
using ValionRL.Core.MapFragments;

namespace ValionRL.Core.Generator
{
    public struct MapFragmentPlacement : IEquatable<MapFragmentPlacement>
    {
        public Optional<string> North;
        public Optional<string> East;
        public Optional<string> South;
        public Optional<string> West;

        public MapFragmentPlacement(Optional<string> north, Optional<string> east, Optional<string> south, Optional<string> west)
        {
            North = north;
            East = east;
            South = south;
            West = west;
        }

        public MapFragmentConnectivity Connectivity
        {
            get
            {
                MapFragmentConnectivity c = MapFragmentConnectivity.None;
                c |= North.HasValue ? MapFragmentConnectivity.North : MapFragmentConnectivity.None;
                c |= East.HasValue ? MapFragmentConnectivity.East : MapFragmentConnectivity.None;
                c |= South.HasValue ? MapFragmentConnectivity.South : MapFragmentConnectivity.None;
                c |= West.HasValue ? MapFragmentConnectivity.West : MapFragmentConnectivity.None;
                return c;
            }
        }

        public bool Equals(MapFragmentPlacement other)
        {
            return North.Equals(other.North) && East.Equals(other.East) && South.Equals(other.South) && West.Equals(other.West);
        }

        public override bool Equals(object obj)
        {
            return obj is MapFragmentPlacement other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = North.GetHashCode();
                hashCode = (hashCode * 397) ^ East.GetHashCode();
                hashCode = (hashCode * 397) ^ South.GetHashCode();
                hashCode = (hashCode * 397) ^ West.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(MapFragmentPlacement left, MapFragmentPlacement right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(MapFragmentPlacement left, MapFragmentPlacement right)
        {
            return !left.Equals(right);
        }

        public static MapFragmentPlacement ToPlacementTemplate(MapFragmentInfo info)
        {
            var north = info.TryQueryTagRestrictions(MapFragmentConnectivity.North);
            var east = info.TryQueryTagRestrictions(MapFragmentConnectivity.East);
            var south = info.TryQueryTagRestrictions(MapFragmentConnectivity.South);
            var west = info.TryQueryTagRestrictions(MapFragmentConnectivity.West);
            return new MapFragmentPlacement(north, east, south, west);
        }

        public static MapFragmentPlacement ToPlacementTemplate(MapFragmentConnectivity info)
        {
            var north = info.HasFlags(MapFragmentConnectivity.North) ? Optional.ValueOf("") : Optional.Empty<string>();
            var east = info.HasFlags(MapFragmentConnectivity.East) ? Optional.ValueOf("") : Optional.Empty<string>();
            var south = info.HasFlags(MapFragmentConnectivity.South) ? Optional.ValueOf("") : Optional.Empty<string>();
            var west = info.HasFlags(MapFragmentConnectivity.West) ? Optional.ValueOf("") : Optional.Empty<string>();
            return new MapFragmentPlacement(north, east, south, west);
        }


        public override string ToString()
        {
            StringBuilder b = new StringBuilder();
            b.Append(nameof(MapFragmentPlacement));
            b.Append("(");
            bool dirty = false;
            if (North.TryGetValue(out var n))
            {
                b.Append(nameof(North));
                b.Append(": ");
                b.Append(n);
                dirty = true;
            }
            if (East.TryGetValue(out var e))
            {
                if (dirty)
                {
                    b.Append(", ");
                }
                b.Append(nameof(East));
                b.Append(": ");
                b.Append(e);
                dirty = true;
            }

            if (South.TryGetValue(out var s))
            {
                if (dirty)
                {
                    b.Append(", ");
                }
                b.Append(nameof(South));
                b.Append(": ");
                b.Append(s);
                dirty = true;
            }

            if (West.TryGetValue(out var w))
            {
                if (dirty)
                {
                    b.Append(", ");
                }
                b.Append(nameof(West));
                b.Append(": ");
                b.Append(w);
            }
            b.Append(")");
            return b.ToString();
        }
    }
}