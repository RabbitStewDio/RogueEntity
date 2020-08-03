using System;
using System.Collections.Generic;
using System.Text;
using RogueEntity.Core.Infrastructure.Positioning.Grid;
using RogueEntity.Core.Utils;
using Serilog;
using ValionRL.Core.MapFragments;

namespace ValionRL.Core.Generator
{
    public static class RegularGridMapGeneratorChain
    {
        static readonly ILogger log = SLog.ForContext(typeof(RegularGridMapGeneratorChain));

        public static T WithConnectionWeight<T>(this T g, MapFragmentConnectivity c, int weight)
            where T : IConnectionWeightData
        {
            g.UpdateConnectionWeight(c, weight);
            return g;
        }

        public static T WithStraightConnectionWeight<T>(this T g, int w)
            where T : IConnectionWeightData
        {
            g.UpdateConnectionWeight(MapFragmentConnectivity.South | MapFragmentConnectivity.North, w);
            g.UpdateConnectionWeight(MapFragmentConnectivity.East | MapFragmentConnectivity.West, w);
            return g;
        }

        public static T WithDeadEndWeight<T>(this T g, int w)
            where T : IConnectionWeightData
        {
            g.UpdateConnectionWeight(MapFragmentConnectivity.South, w);
            g.UpdateConnectionWeight(MapFragmentConnectivity.North, w);
            g.UpdateConnectionWeight(MapFragmentConnectivity.West, w);
            g.UpdateConnectionWeight(MapFragmentConnectivity.East, w);
            return g;
        }

        public static T WithCornerWeight<T>(this T g, int w)
            where T : IConnectionWeightData
        {
            g.UpdateConnectionWeight(MapFragmentConnectivity.South | MapFragmentConnectivity.East, w);
            g.UpdateConnectionWeight(MapFragmentConnectivity.South | MapFragmentConnectivity.West, w);
            g.UpdateConnectionWeight(MapFragmentConnectivity.North | MapFragmentConnectivity.East, w);
            g.UpdateConnectionWeight(MapFragmentConnectivity.North | MapFragmentConnectivity.West, w);
            return g;
        }

        public static T WithJunctionWeight<T>(this T g, int w)
            where T : IConnectionWeightData
        {
            g.UpdateConnectionWeight(MapFragmentConnectivity.South | MapFragmentConnectivity.East | MapFragmentConnectivity.West, w);
            g.UpdateConnectionWeight(MapFragmentConnectivity.North | MapFragmentConnectivity.East | MapFragmentConnectivity.West, w);
            g.UpdateConnectionWeight(MapFragmentConnectivity.North | MapFragmentConnectivity.South | MapFragmentConnectivity.West, w);
            g.UpdateConnectionWeight(MapFragmentConnectivity.North | MapFragmentConnectivity.South | MapFragmentConnectivity.East, w);
            return g;
        }

        internal static Predicate<MapFragment> HasTag(string tag)
        {
            return mf => mf.Info.Tags.Contains(tag);
        }

        public static InitialGeneratorState<TGameContext> SelectStartingNode<TGameContext>(this GeneratorConfig<TGameContext> gs,
                                                                                           EntityGridPosition startPos, Func<double> rng)
        {
            return InitialGeneratorState<TGameContext>.Create(gs, startPos, rng);
        }

        public static NodePlacementGeneratorState<TGameContext> PlaceNodes<TGameContext>(this InitialGeneratorState<TGameContext> runtimeState, int maxNodes = int.MaxValue)
        {
            var rs = new NodePlacementGeneratorState<TGameContext>(runtimeState.Config,
                                                                   runtimeState.RandomGenerator,
                                                                   runtimeState.StartingMapPosition,
                                                                   runtimeState.StartPosition,
                                                                   runtimeState.SelectedStartNode);
            return rs.PlaceNodes(maxNodes);
        }

        public static NodePlacementGeneratorState<TGameContext> PlaceNodes<TGameContext>(this NodePlacementGeneratorState<TGameContext> rs, int maxNodes = int.MaxValue)
        {
            var nodesPlaced = 0;

            log.Debug("Restarting processing with (" + string.Join(",", rs.OpenList) + ")");
            var count = rs.PendingList.Count;
            if (count > 0)
            {
                log.Debug("Processing pending nodes");
                while (count > 0 && rs.PendingList.TryDequeue(out var p))
                {
                    count -= 1;
                    Log.Debug("  Node: {Position}", new Coord(p.X, p.Y));
                    AttemptPlacement(rs, p.DistanceFromStart, p.X, p.Y);
                }
            }

            while (nodesPlaced < maxNodes &&
                   rs.OpenList.TryDequeue(out var node))
            {
                var connectivity = node.Connections;
                log.Debug("Processing placement at {Position} with connections to {Connections}", node.GridPosition, connectivity);
                var targets = connectivity.Connectivity.Targets(node.GridPosition.X, node.GridPosition.Y);
                foreach (var (d, c) in targets)
                {
                    var x = c.X;
                    var y = c.Y;
                    if (rs.Nodes[x, y] != null)
                    {
                        log.Debug("  Skipping {Position} to {Direction}; {PlacementPos}", node.GridPosition, d, new Coord(x, y));
                        continue;
                    }

                    nodesPlaced += 1;

                    log.Debug("  Checking {Position} to {Direction}; {PlacementPos}", node.GridPosition, d, new Coord(x, y));
                    AttemptPlacement(rs, node.DistanceFromStart, x, y);
                }
            }

            return rs;
        }

        static void AttemptPlacement<TGameContext>(this NodePlacementGeneratorState<TGameContext> rs, int distanceFromStart, int x, int y)
        {
            if (!rs.AttemptPlacement(x, y, out var selectedConnection))
            {
                rs.PendingList.Enqueue(new PendingPlacementRecord(x, y, distanceFromStart));
                return;
            }

            if (rs.PlaceNode(selectedConnection, x, y, distanceFromStart, out var nextNode))
            {
                log.Verbose("  Add connection as ({Connection}) at {Position}", selectedConnection, new Coord(x, y));
                rs.OpenList.Enqueue(nextNode);
            }
            else
            {
                log.Debug("Unable to place node at {Position}", new Coord(x, y));
            }
        }

        internal static bool AttemptPlacement<TGameContext>(this NodePlacementGeneratorState<TGameContext> rs,
                                                            int x, int y, out MapFragmentPlacement selectedConnection)
        {
            var connectionTypes = rs.WeightedConnectionTypes;
            var requiredConnections = rs.ComputeRequiredConnectivity(x, y);
            var allowedConnections = rs.ComputeAcceptableConnectivity(x, y);
            if (!connectionTypes.TrySelectForPlacement(rs.RandomGenerator,
                                                       out selectedConnection,
                                                       requiredConnections,
                                                       allowedConnections))
            {
                log.Verbose("Unable to find suitable tile for placement at {Position} with required connection ({RequiredConnection}) and allowed connection ({AllowedConnection})",
                            new Coord(x, y), requiredConnections, allowedConnections);
                return false;
            }

            return true;
        }

        public static PopulatedTilesGeneratorState<TGameContext> PopulateTiles<TGameContext>(this NodePlacementGeneratorState<TGameContext> runtimeState)
        {
            var s = new PopulatedTilesGeneratorState<TGameContext>(runtimeState);
            var tiles = s.Config.Tiles;

            foreach (var n in s.Nodes)
            {
                if (n == null)
                {
                    continue;
                }

                if (n.SelectedTile.HasValue)
                {
                    continue;
                }

                if (!tiles.TrySelectReplacement(s.RandomGenerator, out var fragment, n.Connections.Connectivity))
                {
                    throw new InvalidOperationException("Unable to find matching fragment. This should not happen.");
                }

                n.SelectedTile = fragment;
            }

            return s;
        }


        public static bool TryPlaceExitNode<TGameContext>(this PopulatedTilesGeneratorState<TGameContext> runtimeState,
                                                          out ExitNodePlacementGeneratorState<TGameContext> s)
        {
            var exitState = new ExitNodePlacementGeneratorState<TGameContext>(runtimeState);
            if (!exitState.PlaceExitNode())
            {
                s = default;
                return false;
            }

            s = exitState;
            return true;
        }

        public static MapFragmentConnectivity ComputeRequiredConnectivity(this INodeConnectivitySource s, int x, int y)
        {
            return ComputeConnectivity(s, x, y, false);
        }

        public static MapFragmentConnectivity ComputeAcceptableConnectivity(this INodeConnectivitySource s, int x, int y)
        {
            return ComputeConnectivity(s, x, y, true);
        }

        static MapFragmentConnectivity ComputeConnectivity(this INodeConnectivitySource s, int x, int y, bool whenEmpty)
        {
            var m = MapFragmentConnectivity.None;
            if (s.CanConnectTo(x - 1, y, MapFragmentConnectivity.East, whenEmpty))
            {
                m |= MapFragmentConnectivity.West;
            }

            if (s.CanConnectTo(x + 1, y, MapFragmentConnectivity.West, whenEmpty))
            {
                m |= MapFragmentConnectivity.East;
            }

            if (s.CanConnectTo(x, y - 1, MapFragmentConnectivity.South, whenEmpty))
            {
                m |= MapFragmentConnectivity.North;
            }

            if (s.CanConnectTo(x, y + 1, MapFragmentConnectivity.North, whenEmpty))
            {
                m |= MapFragmentConnectivity.South;
            }

            return m;
        }

        public static void FinalizeMap<TGameContext>(this ExitNodePlacementGeneratorState<TGameContext> state)
        {
            var tw = state.Config.TileWidth;
            var th = state.Config.TileHeight;
            foreach (var n in state.Nodes)
            {
                if (n == null)
                {
                    continue;
                }

                if (n.SelectedTile.TryGetValue(out var tile))
                {
                    log.Information("Copy " + n);
                    var pos = EntityGridPosition.Of(default,
                                                    n.GridPosition.X * tw,
                                                    n.GridPosition.Y * th,
                                                    state.StartingMapPosition.GridZ);
                    state.Config.Context.CopyItemsToMap(tile, pos);
                    state.Config.Context.CopyActorsToMap(tile, pos);
                }
            }
        }

        public static string PrintConnectivityMap<TGameContext>(GeneratorStateBase<TGameContext> c) 
        {
            var b = new StringBuilder();
            b.AppendLine();
            for (int y = 0; y < c.Height; y += 1)
            {
                for (int x = 0; x < c.Width; x += 1)
                {
                    var cc = c.Nodes[x, y]?.Connections.Connectivity ?? MapFragmentConnectivity.None;
                    b.Append(cc.ToBoxDrawing());
                }

                b.AppendLine("");
            }

            return b.ToString();
        }


        public static NodePlacementGeneratorState<TGameContext> Print<TGameContext>(this NodePlacementGeneratorState<TGameContext> c)
        {
            log.Information("{ConnectionMap}", PrintConnectivityMap(c));
            return c;
        }

        public static ExitNodePlacementGeneratorState<TGameContext> Print<TGameContext>(this ExitNodePlacementGeneratorState<TGameContext> c)
        {
            log.Information("{ConnectionMap}", PrintConnectivityMap(c));
            return c;
        }

        public static void PrintStatistics<TGameContext>(this ExitNodePlacementGeneratorState<TGameContext> state)
        {
            Dictionary<string, int> counts = new Dictionary<string, int>();
            foreach (var stateNode in state.Nodes)
            {
                if (stateNode == null)
                {
                    continue;
                }

                if (!stateNode.SelectedTile.TryGetValue(out var fragment))
                {
                    continue;
                }

                foreach (var c in fragment.Info.Properties.TryGetValues("Class"))
                {
                    if (counts.TryGetValue(c, out var cnt))
                    {
                        counts[c] = cnt + 1;
                    }
                    else
                    {
                        counts[c] = 1;
                    }
                }
            }

            log.Information("MapSize: {Width}, {Height}; Node Counts: {Counts}", state.Width, state.Height, counts);
        }
    }
}