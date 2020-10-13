using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using RogueEntity.Core.Utils;
using RogueEntity.Generator;
using RogueEntity.Generator.MapFragments;
using ValionRL.Core.MapFragments;

namespace ValionRL.Core.Generator
{
    public readonly struct GeneratorConfig<TGameContext> 
    {
        public readonly ReadOnlyListWrapper<MapFragment> EntryZones;
        public readonly ReadOnlyListWrapper<MapFragment> ExitZones;
        public readonly ReadOnlyListWrapper<MapFragment> Tiles;
        public readonly ReadOnlyDictionary<MapFragmentPlacement, int> ConnectionWeights;
        public readonly int TileWidth;
        public readonly int TileHeight;
        public readonly int GridWidth;
        public readonly int GridHeight;
        public readonly TGameContext Context;

        public GeneratorConfig(ReadOnlyListWrapper<MapFragment> entryZones,
                               ReadOnlyListWrapper<MapFragment> exitZones,
                               ReadOnlyListWrapper<MapFragment> tiles,
                               ReadOnlyDictionary<MapFragmentPlacement, int> connectionWeights,
                               int tileWidth, int tileHeight,
                               int gridWidth, int gridHeight,
                               TGameContext context)
        {
            EntryZones = entryZones;
            ExitZones = exitZones;
            Tiles = tiles;
            ConnectionWeights = connectionWeights;
            TileWidth = tileWidth;
            TileHeight = tileHeight;
            GridWidth = gridWidth;
            GridHeight = gridHeight;
            this.Context = context;
        }

        public static GeneratorConfig<TGameContext> Create(RegularGridMapGenerator<TGameContext> g)
        {
            if (g.GridX == 0 || g.GridY == 0)
            {
                throw new InvalidOperationException("TileSize is 0");
            }

            if (g.Fragments.Count == 0)
            {
                throw new InvalidOperationException("No MapFragments available");
            }

            var mapGridSizeX = (g.Width - 2) / g.GridX;
            var mapGridSizeY = (g.Height - 2) / g.GridY;
            if (mapGridSizeX == 0 || mapGridSizeY == 0)
            {
                throw new InvalidOperationException("Resulting GridSize is 0");
            }

            var fragments = g.Fragments;
            var startZone = fragments.FindAll(RegularGridMapGeneratorChain.HasTag("EntryPoint"));
            var endZone = fragments.FindAll(RegularGridMapGeneratorChain.HasTag("ExitPoint"));
            var remainingTiles = fragments.FindAll(t => !t.Info.Tags.Contains("EntryPoint") &&
                                                        !t.Info.Tags.Contains("ExitPoint"));

            var connectionTypes = remainingTiles.Select(c => MapFragmentPlacement.ToPlacementTemplate(c.Info))
                                                .Distinct()
                                                .ToList();
            var weightedConnections = new Dictionary<MapFragmentPlacement, int>();
            foreach (var w in connectionTypes)
            {
                if (g.ConnectionWeights.TryGetValue(w.Connectivity, out var weight))
                {
                    weightedConnections[w] = weight;
                }
                else
                {
                    weightedConnections[w] = 1;
                }
            }

            return new GeneratorConfig<TGameContext>(startZone, endZone, remainingTiles,
                                                     weightedConnections.AsReadOnly(),
                                                     g.GridX, g.GridY,
                                                     mapGridSizeX, mapGridSizeY, g.Context);
        }
    }
}