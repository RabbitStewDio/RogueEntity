using System;
using System.Collections.Generic;
using RogueEntity.Core.GridProcessing.Transforms;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.MapLayers;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.Algorithms;
using RogueEntity.Core.Utils.DataViews;

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

        public MazeGenerator WithSeed(int seed)
        {
            return new MazeGenerator(seed, bounds);
        }

        public MazeGenerator WithBounds(in Rectangle bounds)
        {
            return new MazeGenerator(seed, bounds);
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
            var ruleSet = ParseRuleString(ruleString);

            var sys = new MazeGridTransformSystem<TEntity>(new DynamicDataViewConfiguration(0, 0, 64, 64), ruleSet, spaceEntity, wallEntity);
            if (!sys.ResultView.TryGetWritableView(0, out var baseView, DataViewCreateMode.CreateMissing))
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
                sys.MarkDirty(Position.Of(MapLayer.Indeterminate, x, y, 0));
            }

            baseView[bounds.Center.X, bounds.Center.Y] = spaceEntity;

            for (int i = 0; i < iterations; i += 1)
            {
                sys.ProcessAndSwap();
            }

            if (sys.ResultView.TryGetView(0, out var resultView))
            {
                return resultView;
            }

            throw new Exception();
        }

        CellTransitions[] ParseRuleString(string ruleString)
        {
            var cellRule = new CellTransitions[10];
            Array.Fill(cellRule, CellTransitions.Dead);

            if (ruleString.Length == 0)
            {
                return cellRule;
            }

            var bsSyntax = (ruleString[0] == 'b' || ruleString[0] == 'B');
            // B{n}/S{n} syntax.
            var isBornFlag = bsSyntax;
            // state == true => set born flag 
            // state == false => clear death flag
            foreach (var c in ruleString)
            {
                if (c == '/')
                {
                    isBornFlag = !isBornFlag;
                }
                else if (char.IsDigit(c))
                {
                    var idx = c - '0';
                    if (isBornFlag)
                    {
                        // born rule
                        cellRule[idx] |= CellTransitions.Born;
                    }
                    else
                    {
                        // survival rule
                        cellRule[idx] &= ~CellTransitions.Dead;
                    }
                }
            }

            return cellRule;
        }
    }

    class MazeGridTransformSystem<TEntity> : GridTransformSystemBase<TEntity, TEntity>
    {
        static readonly EqualityComparer<TEntity> EqualityComparer = EqualityComparer<TEntity>.Default;

        readonly CellTransitions[] rules;
        protected override IReadOnlyDynamicDataView3D<TEntity> SourceData => sourceView;
        protected override IDynamicDataView3D<TEntity> TargetData => targetView;

        IDynamicDataView3D<TEntity> sourceView;
        IDynamicDataView3D<TEntity> targetView;
        readonly TEntity cellAliveMarker;
        readonly TEntity cellDeadMarker;

        public MazeGridTransformSystem(DynamicDataViewConfiguration config,
                                       CellTransitions[] rules,
                                       TEntity cellAliveMarker,
                                       TEntity cellDeadMarker) : base(config)
        {
            this.rules = rules;
            this.cellAliveMarker = cellAliveMarker;
            this.cellDeadMarker = cellDeadMarker;
            sourceView = new DynamicDataView3D<TEntity>();
            targetView = new DynamicDataView3D<TEntity>();
        }

        public IDynamicDataView3D<TEntity> ResultView => sourceView;

        public void ProcessAndSwap()
        {
            base.Process();

            var tmp = sourceView;
            sourceView = targetView;
            targetView = tmp;
        }

        protected override void ProcessTile(ProcessingParameters args)
        {
            bool dirty = false;
            foreach (var (x, y) in args.Bounds.Contents)
            {
                var source = args.SourceTile[x, y];
                var alive = EqualityComparer.Equals(cellAliveMarker, source);
                var n = CountNeighbours(args.SourceTile, args.SourceLayer, x, y) + (alive ? 1 : 0);
                if (rules[n].HasFlags(CellTransitions.Dead))
                {
                    args.ResultTile[x, y] = cellDeadMarker;
                    dirty |= alive;
                }
                else if (rules[n].HasFlags(CellTransitions.Born))
                {
                    args.ResultTile[x, y] = cellAliveMarker;
                    dirty |= !alive;
                }
                else
                {
                    args.ResultTile[x, y] = source;
                }
            }

            if (dirty)
            {
                MarkDirty(Position.Of(MapLayer.Indeterminate, args.Bounds.X, args.Bounds.Y, args.ZPosition));
            }
        }

        // bool IsCellAlive(IReadOnlyDynamicDataView2D<TEntity> v, int x, int y) => EqualityComparer.Equals(v[x, y], cellAliveMarker);

        int CountNeighbours(IReadOnlyBoundedDataView<TEntity> b,
                            IReadOnlyDynamicDataView2D<TEntity> n,
                            int x,
                            int y)
        {
            var result = 0;
            foreach (var d in AdjacencyRule.EightWay.DirectionsOfNeighbors())
            {
                var c = d.ToCoordinates();
                var tx = x + c.X;
                var ty = y + c.Y;
                if (!b.TryGet(tx, ty, out var content))
                {
                    content = n[tx, ty];
                }

                if (EqualityComparer.Equals(content, cellAliveMarker))
                {
                    result += 1;
                }
            }

            return result;
        }
    }

    [Flags]
    public enum CellTransitions
    {
        Unchanged = 0,
        Born = 1,
        Dead = 2
    }
}