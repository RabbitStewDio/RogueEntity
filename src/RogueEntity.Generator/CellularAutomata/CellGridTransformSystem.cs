using System;
using System.Collections.Generic;
using RogueEntity.Core.GridProcessing.Transforms;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.MapLayers;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.Algorithms;
using RogueEntity.Core.Utils.DataViews;

namespace RogueEntity.Generator.CellularAutomata
{
    public class CellGridTransformSystem<TEntity> : GridTransformSystemBase<TEntity, TEntity>
    {
        static readonly EqualityComparer<TEntity> EqualityComparer = EqualityComparer<TEntity>.Default;

        readonly CARules rules;
        protected override IReadOnlyDynamicDataView3D<TEntity> SourceData => sourceView;
        protected override IDynamicDataView3D<TEntity> TargetData => targetView;

        IDynamicDataView3D<TEntity> sourceView;
        IDynamicDataView3D<TEntity> targetView;
        readonly TEntity cellAliveMarker;
        readonly TEntity cellDeadMarker;

        public CellGridTransformSystem(DynamicDataViewConfiguration config,
                                       CARules rules,
                                       TEntity cellAliveMarker,
                                       TEntity cellDeadMarker) : base(config)
        {
            this.rules = rules;
            this.cellAliveMarker = cellAliveMarker;
            this.cellDeadMarker = cellDeadMarker;
            sourceView = new DynamicDataView3D<TEntity>();
            targetView = new DynamicDataView3D<TEntity>();
            
        }
        
        public IDynamicDataView3D<TEntity> DataView => sourceView;

        public bool ProcessAndSwap()
        {
            if (base.Process())
            {
                var tmp = sourceView;
                sourceView = targetView;
                targetView = tmp;
                return true;
            }

            return false;
        }

        public bool ProcessAndSwap(Rectangle bounds)
        {
            if (base.Process(bounds))
            {
                var tmp = sourceView;
                sourceView = targetView;
                targetView = tmp;
                return true;
            }

            return false;
        }

        protected override void ProcessTile(ProcessingParameters args)
        {
            bool dirty = false;
            foreach (var (x, y) in args.Bounds.Contents)
            {
                var source = args.SourceTile[x, y];
                var alive = EqualityComparer.Equals(cellAliveMarker, source);
                var n = CountNeighbours(args.SourceTile, args.SourceLayer, x, y);

                TEntity result;
                if (alive)
                {
                    if (!rules.Surive(n))
                    {
                        result = cellDeadMarker;
                        //Console.WriteLine($"({x}, {y}) => {n} dead");
                        dirty = true;
                    }
                    else
                    {
                        result = cellAliveMarker;
                    }
                }
                else
                {
                    if (!rules.Birth(n))
                    {
                        result = cellDeadMarker;
                    }
                    else
                    {
                        result = cellAliveMarker;
                     //   Console.WriteLine($"({x}, {y}) => {n} alive");
                        dirty = true;
                    }
                }
                args.ResultTile[x, y] = result;
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
}