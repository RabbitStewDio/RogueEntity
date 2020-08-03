using System;
using RogueEntity.Core.Infrastructure.Positioning.Grid;
using ValionRL.Core.MapFragments;

namespace ValionRL.Core.Generator
{
    public class InitialGeneratorState<TGameContext>
    {
        public readonly Func<double> RandomGenerator;
        public readonly GeneratorConfig<TGameContext> Config;
        public readonly MapFragment SelectedStartNode;
        public readonly Coord StartPosition;
        public readonly EntityGridPosition StartingMapPosition;
        public readonly int Width;
        public readonly int Height;

        InitialGeneratorState(GeneratorConfig<TGameContext> config,
                              Func<double> randomGenerator,
                              MapFragment startNode,
                              Coord startPosition, 
                              EntityGridPosition startingMapPosition)
        {
            this.RandomGenerator = randomGenerator ?? throw new ArgumentNullException();
            this.Config = config;
            var width = config.GridWidth;
            var height = config.GridHeight;

            this.Width = width;
            this.Height = height;
            this.SelectedStartNode = startNode;
            this.StartPosition = startPosition;
            this.StartingMapPosition = startingMapPosition;
        }

        public static InitialGeneratorState<TGameContext> Create(in GeneratorConfig<TGameContext> gs, 
                                                                 EntityGridPosition startingPoint, 
                                                                 Func<double> rng)
        {
            var startTileX = startingPoint.GridX / gs.TileWidth;
            var startTileY = startingPoint.GridY / gs.TileHeight;

            var startConnections = 
                new InitialConnectivitySource(gs.GridWidth, gs.GridHeight)
                .ComputeAcceptableConnectivity(startTileY, startTileY);

            if (!gs.EntryZones.TrySelectForPlacement(rng, out var startingFragment, 
                                                     MapFragmentConnectivity.None, startConnections) &&
                !gs.EntryZones.TrySelectAny(rng, out startingFragment))
            {
                throw new InvalidOperationException("Unable to place start node");
            }

            return new InitialGeneratorState<TGameContext>(gs, rng, startingFragment, new Coord(startTileX, startTileY), startingPoint);
        }

        class InitialConnectivitySource : INodeConnectivitySource
        {
            public readonly int Width;
            public readonly int Height;

            public InitialConnectivitySource(int width, int height)
            {
                Width = width;
                Height = height;
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

                return whenNoNode;
            }
        }

    }
}