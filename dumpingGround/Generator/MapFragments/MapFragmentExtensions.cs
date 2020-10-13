using System;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.Maps;
using ValionRL.Core.MapFragments;

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

            var c = mf.Info.Connectivity.Swap(MapFragmentConnectivity.East, MapFragmentConnectivity.West);

            var props = mf.Info.Properties.Copy();
            props.DefineProperty(ConnectivityProperty, c.AsPropertyString());
            var nextInfo = new MapFragmentInfo(mf.Info.Name + "[V]", mf.Info.Type, c, props, mf.Info.Tags);
            return new MapFragment(nextMapData, nextInfo);
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

            var c = mf.Info.Connectivity.Swap(MapFragmentConnectivity.North, MapFragmentConnectivity.South);

            var props = mf.Info.Properties.Copy();
            props.DefineProperty(ConnectivityProperty, c.AsPropertyString());
            var nextInfo = new MapFragmentInfo(mf.Info.Name + "[H]", mf.Info.Type, c, props, mf.Info.Tags);
            return new MapFragment(nextMapData, nextInfo);
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
            if (m.Properties.TryGetValue(MirrorProperty, out string mraw))
            {
                if (mraw.Equals("Both", StringComparison.InvariantCultureIgnoreCase))
                {
                    return MapFragmentMirror.Both;
                }

                if (mraw.Equals("Horizontal", StringComparison.InvariantCultureIgnoreCase))
                {
                    return MapFragmentMirror.Horizontal;
                }

                if (mraw.Equals("Vertical", StringComparison.InvariantCultureIgnoreCase))
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