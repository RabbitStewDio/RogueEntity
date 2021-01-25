using RogueEntity.Api.Utils;
using System;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.Maps;

namespace RogueEntity.Generator.MapFragments
{
    public static class MapFragmentExtensions
    {
        public const string MirrorProperty = "MirrorAxis";
        public const string ConnectivityProperty = "Connections";

        /// <summary>
        ///   Mirrors the map fragment along the y-axis
        /// </summary>
        /// <param name="mf"></param>
        /// <returns></returns>
        public static MapFragment MirrorVertically(this MapFragment mf)
        {
            var mapData = mf.MapData;
            var nextMapData = new DenseMapData<MapFragmentTagDeclaration>(mapData.Width, mapData.Height);
            for (var y = 0; y < mapData.Height; y += 1)
            {
                for (var x = 0; x < mapData.Width; x += 1)
                {
                    nextMapData[x, y] = mapData[mapData.Width - x - 1, y];
                }
            }

            if (!mf.Properties.TryGet(out MapFragmentConnectivity c))
            {
                return mf;
            }

            mf = mf.WithName(mf.Info.Name + "[V]")
                   .WithMapData(nextMapData);
            
            c = c.Swap(MapFragmentConnectivity.East, MapFragmentConnectivity.West);

            var properties = mf.Properties.With(c);
            return mf.WithProperties(properties);
        }

        /// <summary>
        ///   Mirrors the map fragment along the x-axis
        /// </summary>
        /// <param name="mf"></param>
        /// <returns></returns>
        public static MapFragment MirrorHorizontally(this MapFragment mf)
        {
            var mapData = mf.MapData;
            var nextMapData = new DenseMapData<MapFragmentTagDeclaration>(mapData.Width, mapData.Height);
            for (var y = 0; y < mapData.Height; y += 1)
            {
                for (var x = 0; x < mapData.Width; x += 1)
                {
                    nextMapData[x, y] = mapData[x, mapData.Height - y - 1];
                }
            }

            mf = mf.WithName(mf.Info.Name + "[H]")
                   .WithMapData(nextMapData);
            if (!mf.Properties.TryGet(out MapFragmentConnectivity c))
            {
                return mf;
            }

            c = c.Swap(MapFragmentConnectivity.North, MapFragmentConnectivity.South);

            var properties = mf.Properties.With(c);
            return mf.WithProperties(properties);

        }

        public static Optional<string> TryQueryTagRestrictions(this MapFragment f, MapFragmentConnectivity c)
        {
            if (!f.Properties.TryGet(out MapFragmentConnectivity self))
            {
                return Optional.Empty();
            }

            if (self.HasFlags(c))
            {
                if (f.Info.Properties.TryGetValue("Require_" + c, out string tagPattern))
                {
                    return Optional.ValueOf(tagPattern);
                }

                return Optional.ValueOf("");
            }

            return Optional.Empty();
        }

        public static MapFragmentConnectivity Swap(this MapFragmentConnectivity source, MapFragmentConnectivity a, MapFragmentConnectivity b)
        {
            var c = source & (~(a | b));
            if (source.HasFlags(a))
            {
                c |= b;
            }

            if (source.HasFlags(b))
            {
                c |= a;
            }

            return c;
        }

        public static MapFragmentMirror QueryMapFragmentMirror(this MapFragmentInfo m)
        {
            if (m.Properties.TryGetValue(MirrorProperty, out string raw))
            {
                if (raw.Equals("Both", StringComparison.InvariantCultureIgnoreCase))
                {
                    return MapFragmentMirror.Both;
                }

                if (raw.Equals("Horizontal", StringComparison.InvariantCultureIgnoreCase))
                {
                    return MapFragmentMirror.Horizontal;
                }

                if (raw.Equals("Vertical", StringComparison.InvariantCultureIgnoreCase))
                {
                    return MapFragmentMirror.Vertical;
                }
            }

            return MapFragmentMirror.None;
        }

        public static string AsPropertyString(this MapFragmentConnectivity mc)
        {
            int north = mc.HasFlags(MapFragmentConnectivity.North) ? 1 : 0;
            int east = mc.HasFlags(MapFragmentConnectivity.East) ? 1 : 0;
            int south = mc.HasFlags(MapFragmentConnectivity.South) ? 1 : 0;
            int west = mc.HasFlags(MapFragmentConnectivity.West) ? 1 : 0;
            return $"N{north}E{east}S{south}W{west}";
        }

        public static MapFragmentConnectivity ParseMapFragmentConnectivity(string s)
        {
            if (string.IsNullOrWhiteSpace(s))
            {
                return MapFragmentConnectivity.None;
            }

            var r = MapFragmentConnectivity.None;
            if (s.IndexOf("N1", StringComparison.InvariantCultureIgnoreCase) != -1)
            {
                r |= MapFragmentConnectivity.North;
            }

            if (s.IndexOf("E1", StringComparison.InvariantCultureIgnoreCase) != -1)
            {
                r |= MapFragmentConnectivity.East;
            }

            if (s.IndexOf("S1", StringComparison.InvariantCultureIgnoreCase) != -1)
            {
                r |= MapFragmentConnectivity.South;
            }

            if (s.IndexOf("W1", StringComparison.InvariantCultureIgnoreCase) != -1)
            {
                r |= MapFragmentConnectivity.West;
            }

            return r;
        }
    }
}
