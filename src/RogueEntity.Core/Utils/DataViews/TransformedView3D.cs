using System;
using System.Collections.Generic;

namespace RogueEntity.Core.Utils.DataViews
{
    public class TransformedView3D<TSource, TTarget> : IReadOnlyDynamicDataView3D<TTarget>
    {
        readonly IReadOnlyDynamicDataView3D<TSource> source;
        readonly Func<TSource, TTarget> transformation;

        public TransformedView3D(IReadOnlyDynamicDataView3D<TSource> source,
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

        public List<int> GetActiveLayers(List<int> buffer = null)
        {
            return source.GetActiveLayers(buffer);
        }

        public bool TryGetView(int z, out IReadOnlyDynamicDataView2D<TTarget> view)
        {
            if (source.TryGetView(z, out var sourceRaw))
            {
                view = new TransformedView2D<TSource,TTarget>(sourceRaw, transformation);
                return true;
            }
            
            view = default;
            return false;
        }
    }
}