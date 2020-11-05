using System;
using System.Collections.Generic;

namespace RogueEntity.Core.Utils.DataViews
{
    public class TransformedView2D<TSource, TTarget> : IReadOnlyDynamicDataView2D<TTarget>
    {
        readonly IReadOnlyDynamicDataView2D<TSource> source;
        readonly Func<TSource, TTarget> transformation;

        public TransformedView2D(IReadOnlyDynamicDataView2D<TSource> source,
                                 Func<TSource, TTarget> transformation)
        {
            this.source = source;
            this.transformation = transformation;
        }

        public int OffsetX
        {
            get { return source.OffsetX; }
        }

        public int OffsetY
        {
            get { return source.OffsetY; }
        }

        public int TileSizeX
        {
            get { return source.TileSizeX; }
        }

        public int TileSizeY
        {
            get { return source.TileSizeY; }
        }

        public List<Rectangle> GetActiveTiles(List<Rectangle> data = null)
        {
            return source.GetActiveTiles(data);
        }

        public Rectangle GetActiveBounds()
        {
            return source.GetActiveBounds();
        }

        public bool TryGetData(int x, int y, out IReadOnlyBoundedDataView<TTarget> raw)
        {
            if (source.TryGetData(x, y, out var sourceRaw))
            {
                raw = new TransformedBoundedDataView<TSource,TTarget>(sourceRaw, transformation);
                return true;
            }
            
            raw = default;
            return false;
        }

        public bool TryGet(int x, int y, out TTarget data)
        {
            if (source.TryGet(x, y, out var sourceData))
            {
                data = transformation(sourceData);
                return true;
            }

            data = default;
            return false;
        }

        public TTarget this[int x, int y]
        {
            get
            {
                if (TryGet(x, y, out var r))
                {
                    return r;
                }

                return default;
            }
        }
    }
}