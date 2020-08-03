using System;
using ValionRL.Core.Infrastructure.Common;

namespace ValionRL.Core.MapFragments
{
    [Flags]
    public enum MapFragmentConnectivity
    {
        None = 0,
        North = 1,
        East = 2,
        South = 4,
        West = 8,
        All = North | East | South | West
    }

    public static class MapFragmentConnectivityExtensions
    {
        public static char ToBoxDrawing(this MapFragmentConnectivity c1)
        {
            switch ((int)c1)
            {
                //     WSEN
                case 0b0000: return ' ';
                case 0b0001: return '\u2575';
                case 0b0010: return '\u2576';
                case 0b0100: return '\u2577';
                case 0b1000: return '\u2574';
                case 0b0011: return '\u2514';
                case 0b0101: return '\u2502';
                case 0b0110: return '\u250C';
                case 0b0111: return '\u251C';
                case 0b1001: return '\u2518';
                case 0b1010: return '\u2500';
                case 0b1011: return '\u2534';
                case 0b1100: return '\u2510';
                case 0b1101: return '\u2524';
                case 0b1110: return '\u252C';
                case 0b1111: return '\u253C';
                default: return '@';
            }
        }

        public static bool CanConnectTo(this MapFragmentConnectivity c1, MapFragmentConnectivity c2)
        {
            if (c1.HasFlags(MapFragmentConnectivity.East) && c2.HasFlags(MapFragmentConnectivity.West))
            {
                return true;
            }

            if (c1.HasFlags(MapFragmentConnectivity.West) && c2.HasFlags(MapFragmentConnectivity.East))
            {
                return true;
            }

            if (c1.HasFlags(MapFragmentConnectivity.North) && c2.HasFlags(MapFragmentConnectivity.South))
            {
                return true;
            }

            if (c1.HasFlags(MapFragmentConnectivity.South) && c2.HasFlags(MapFragmentConnectivity.North))
            {
                return true;
            }

            return false;
        }
    }
}