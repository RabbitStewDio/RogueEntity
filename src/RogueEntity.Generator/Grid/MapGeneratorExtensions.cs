using System;
using System.Collections.Generic;
using System.Linq;
using RogueEntity.Core.Utils;
using RogueEntity.Generator;
using RogueEntity.Generator.MapFragments;
using Serilog;
using ValionRL.Core.MapFragments;

namespace ValionRL.Core.Generator
{
    public static class MapGeneratorExtensions
    {
        static readonly ILogger log = SLog.ForContext(typeof(MapGeneratorExtensions));
        static readonly List<MapFragment> tmp;
        static readonly List<MapFragmentPlacement> tmpFragmentPlacement;

        static MapGeneratorExtensions()
        {
            tmp = new List<MapFragment>();
            tmpFragmentPlacement = new List<MapFragmentPlacement>();
        }

        public static bool TrySelectAny(this ReadOnlyListWrapper<MapFragment> source, Func<double> rng, out MapFragment f)
        {
            if (source.Count == 0)
            {
                f = default;
                return false;
            }

            f = source[rng.Next(0, source.Count)];
            return true;
        }

        public static bool TrySelectForPlacement(this ReadOnlyListWrapper<MapFragment> source, 
                                                 Func<double> rng, out MapFragment f,
                                                 MapFragmentConnectivity requiredEdges,
                                                 MapFragmentConnectivity allowedEdges)
        {
            tmp.Clear();
            tmp.AddRange(source.Where(fr => (fr.Info.Connectivity & requiredEdges) == requiredEdges &&
                                            (fr.Info.Connectivity & ~allowedEdges) == MapFragmentConnectivity.None));
            if (tmp.Count == 0)
            {
                f = default;
                return false;
            }

            f = tmp[rng.Next(0, tmp.Count)];
            return true;
        }

        public static bool IsValidConnection(MapFragmentConnectivity fr,
                                             MapFragmentConnectivity requiredEdges,
                                             MapFragmentConnectivity allowedEdges)
        {
            return (fr & requiredEdges) == requiredEdges &&
                   (fr & ~allowedEdges) == MapFragmentConnectivity.None;
        }

        public static bool TrySelectForPlacement(this ReadOnlyListWrapper<MapFragmentPlacement> source, Func<double> rng, out MapFragmentPlacement f,
                                                 MapFragmentConnectivity requiredEdges,
                                                 MapFragmentConnectivity allowedEdges)
        {
            tmpFragmentPlacement.Clear();
            foreach (var fr in source)
            {
                if (IsValidConnection(fr.Connectivity, requiredEdges, allowedEdges))
                {
                    tmpFragmentPlacement.Add(fr);
                }
            }

            if (tmpFragmentPlacement.Count == 0)
            {
                log.Verbose("There are no tiles that fit requirement of Required({RequiredEdges}) and Acceptable({AllowedEdges})", requiredEdges, allowedEdges);
                f = default;
                return false;
            }

            f = tmpFragmentPlacement[rng.Next(0, tmpFragmentPlacement.Count)];
            return true;
        }

        public static bool TrySelectReplacement(this ReadOnlyListWrapper<MapFragment> source, Func<double> rng, out MapFragment f, MapFragmentConnectivity c)
        {
            tmp.Clear();
            tmp.AddRange(source.Where(fr => fr.Info.Connectivity == c));
            if (tmp.Count == 0)
            {
                f = default;
                return false;
            }

            f = tmp[rng.Next(0, tmp.Count)];
            return true;
        }

        public static MapFragmentConnectivity ToConnectionFlags(this Direction d)
        {
            if (d == Direction.North)
            {
                return MapFragmentConnectivity.North;
            }

            if (d == Direction.South)
            {
                return MapFragmentConnectivity.South;
            }

            if (d == Direction.West)
            {
                return MapFragmentConnectivity.West;
            }

            if (d == Direction.East)
            {
                return MapFragmentConnectivity.East;
            }

            return MapFragmentConnectivity.None;
        }

        public static List<(Direction d, Coord c)> Targets(this MapFragmentConnectivity c, int x, int y)
        {
            var l = new List<(Direction, Coord)>();
            var coord = new Coord(x, y);
            if (c.HasFlags(MapFragmentConnectivity.North))
            {
                var p = coord + Direction.North;
                l.Add((Direction.North, p));
            }

            if (c.HasFlags(MapFragmentConnectivity.South))
            {
                l.Add((Direction.South, coord + Direction.South));
            }

            if (c.HasFlags(MapFragmentConnectivity.East))
            {
                l.Add((Direction.East, coord + Direction.East));
            }

            if (c.HasFlags(MapFragmentConnectivity.West))
            {
                l.Add((Direction.West, coord + Direction.West));
            }

            return l;
        }
    }
}