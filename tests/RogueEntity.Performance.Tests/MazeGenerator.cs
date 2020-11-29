using System;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.MapLayers;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.DataViews;
using RogueEntity.Generator.CellularAutomata;

namespace RogueEntity.Performance.Tests
{
    public readonly struct MazeGenerator
    {
        readonly int seed;
        readonly Rectangle bounds;

        public MazeGenerator(int seed, Rectangle bounds)
        {
            this.seed = seed;
            this.bounds = bounds;
        }

        public MazeGenerator WithSeed(int seedValue)
        {
            return new MazeGenerator(seedValue, bounds);
        }

        public MazeGenerator WithBounds(in Rectangle boundsValue)
        {
            return new MazeGenerator(seed, boundsValue);
        }

        Random ProduceRandomGenerator()
        {
            if (seed == 0)
            {
                return new Random();
            }

            return new Random(seed);
        }

        public IReadOnlyDynamicDataView2D<TEntity> Generate<TEntity>(TEntity spaceEntity, TEntity wallEntity, int iterations = 100)
        {
            var rng = ProduceRandomGenerator();

            var ruleString = "B3/S12345";
            var ruleSet = CARuleParser.ParseRuleString(ruleString);

            var sys = new CellGridTransformSystem<TEntity>(new DynamicDataViewConfiguration(0, 0, 64, 64), ruleSet, spaceEntity, wallEntity);
            if (!sys.DataView.TryGetWritableView(0, out var baseView, DataViewCreateMode.CreateMissing))
            {
                throw new ArgumentException();
            }

            foreach (var (x, y) in bounds.Contents)
            {
                baseView[x, y] = wallEntity; // rng.NextDouble() >= 0.5 ? wallEntity : spaceEntity;
            }

            foreach (var (x, y) in Rectangle.WithRadius(bounds.Center.X, bounds.Center.Y, 3, 3).Contents)
            {
                baseView[x, y] = rng.NextDouble() >= 0.5 ? wallEntity : spaceEntity;
                sys.MarkDirty(Position.Of(MapLayer.Indeterminate, x, y));
            }

            baseView[bounds.Center.X, bounds.Center.Y] = spaceEntity;

            for (int i = 0; i < iterations; i += 1)
            {
                sys.ProcessAndSwap();
            }

            if (sys.DataView.TryGetView(0, out var resultView))
            {
                return resultView;
            }

            throw new Exception();
        }

    }


}