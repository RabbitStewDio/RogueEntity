using System;
using System.Collections.Generic;
using RogueEntity.Core.Infrastructure.Positioning.Grid;
using RogueEntity.Core.Utils;
using RogueEntity.Generator.MapFragments;
using ValionRL.Core.MapFragments;

namespace ValionRL.Core.Generator
{
    public class NodePlacementGeneratorState<TGameContext> : GeneratorStateBase<TGameContext>,
                                                             INodeConnectivitySource,
                                                             IConnectionWeightData 
    {
        Dictionary<MapFragmentPlacement, int> connectionWeightData;
        readonly List<MapFragmentPlacement> connectionWeights;
        bool connectionWeightsDirty;

        protected NodePlacementGeneratorState(NodePlacementGeneratorState<TGameContext> copy) : base(copy)
        {
            this.connectionWeightData = new Dictionary<MapFragmentPlacement, int>(copy.connectionWeightData);
            this.connectionWeights = new List<MapFragmentPlacement>(copy.connectionWeights);
            this.connectionWeightsDirty = copy.connectionWeightsDirty;
        }

        public NodePlacementGeneratorState(GeneratorConfig<TGameContext> config,
                                           Func<double> randomGenerator,
                                           EntityGridPosition startingPosition,
                                           Coord gridStartPos,
                                           MapFragment startFragment): 
            base(config, randomGenerator, startingPosition, gridStartPos)
        {
            this.connectionWeights = new List<MapFragmentPlacement>();
            this.connectionWeightsDirty = true;

            this.connectionWeightData = new Dictionary<MapFragmentPlacement, int>(config.ConnectionWeights);

            PlaceNode(MapFragmentPlacement.ToPlacementTemplate(startFragment.Info), gridStartPos.X, gridStartPos.Y, -1, out var startNode);
            startNode.SelectedTile = startFragment;
            OpenList.Enqueue(startNode);
        }

        public void UpdateConnectionWeight(MapFragmentConnectivity c, int weight)
        {
            var affectedItems = new List<MapFragmentPlacement>();
            foreach (var w in Config.ConnectionWeights)
            {
                if (w.Key.Connectivity == c)
                {
                    affectedItems.Add(w.Key);
                }
            }

            foreach (var i in affectedItems)
            {
                connectionWeightData[i] = Math.Max(0, weight);
                connectionWeightsDirty = true;
            }
        }

        public ReadOnlyListWrapper<MapFragmentPlacement> WeightedConnectionTypes
        {
            get
            {
                if (connectionWeightsDirty)
                {
                    connectionWeights.Clear();
                    foreach (var c in connectionWeightData)
                    {
                        for (int x = 0; x < c.Value; x += 1)
                        {
                            connectionWeights.Add(c.Key);
                        }
                    }

                    connectionWeightsDirty = false;
                }

                return connectionWeights;
            }
        }

        public bool PlaceNode(MapFragmentPlacement mf, int x, int y, int sourceDistanceFromStart, out Node node)
        {
            if (Nodes[x, y] != null)
            {
                node = default;
                return false;
            }

            PlaceNodeInternal(mf, x, y, sourceDistanceFromStart, out node);
            return true;
        }

        public bool CanConnectTo(int x, int y, MapFragmentConnectivity edge, bool whenNoNode = true)
        {
            if (x < 0 || y < 0)
            {
                return false;
            }

            if (x >= Width || y >= Height)
            {
                return false;
            }

            var n = Nodes[x, y];
            if (n == null)
            {
                return whenNoNode;
            }

            return (n.Connections.Connectivity & edge) == edge;
        }

        public bool CanPlace(MapFragmentPlacement nodeConnections)
        {
            return connectionWeightData.TryGetValue(nodeConnections, out var weight) && weight > 0;
        }
    }
}