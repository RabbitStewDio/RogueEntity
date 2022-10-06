using System.Collections.Generic;
using RogueEntity.Core.GridProcessing.Transforms;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Algorithms;
using RogueEntity.Core.Positioning.MapLayers;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.DataViews;

namespace RogueEntity.Generator.CellularAutomata
{
    public class CellGridTransformSystem<TEntity> : GridTransformSystemBase<TEntity, TEntity>
    {
        static readonly EqualityComparer<TEntity> equalityComparer = EqualityComparer<TEntity>.Default;

        readonly CARules rules;
        protected override IReadOnlyDynamicDataView3D<TEntity> SourceData => sourceView;
        protected override IDynamicDataView3D<TEntity> TargetData => targetView;

        DynamicDataView3D<TEntity> sourceView;
        DynamicDataView3D<TEntity> targetView;
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

        protected override void RemoveTargetDataLayer(int z)
        {
            targetView.RemoveView(z);
        }

        public IDynamicDataView3D<TEntity> DataView => sourceView;

        protected override void FireViewProcessedEvent(int zPosition, 
                                                       IReadOnlyDynamicDataView2D<TEntity> sourceLayer, 
                                                       IBoundedDataView<TEntity> resultTile)
        {
        }

        public bool ProcessAndSwap()
        {
            if (base.Process())
            {
                (sourceView, targetView) = (targetView, sourceView);
                return true;
            }

            return false;
        }
        
        public bool ProcessAndSwap(Rectangle bounds)
        {
            if (base.Process(bounds))
            {
                (sourceView, targetView) = (targetView, sourceView);
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
                var alive = equalityComparer.Equals(cellAliveMarker, source);
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
                args.TargetTile[x, y] = result;
            }

            if (dirty)
            {
                MarkDirty(Position.Of(MapLayer.Indeterminate, args.Bounds.X, args.Bounds.Y, args.ZPosition));
            }
        }

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

                if (equalityComparer.Equals(content, cellAliveMarker))
                {
                    result += 1;
                }
            }

            return result;
        }
    }
}