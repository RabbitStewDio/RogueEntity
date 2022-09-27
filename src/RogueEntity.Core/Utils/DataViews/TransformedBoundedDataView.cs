using System;
using System.Diagnostics.CodeAnalysis;

namespace RogueEntity.Core.Utils.DataViews
{
    public class TransformedBoundedDataView<TSource, TTarget> : IReadOnlyBoundedDataView<TTarget>
    {
        readonly IReadOnlyBoundedDataView<TSource> source;
        readonly Func<TSource, TTarget> transformation;

        public TransformedBoundedDataView(IReadOnlyBoundedDataView<TSource> source, Func<TSource, TTarget> transformation)
        {
            this.source = source;
            this.transformation = transformation;
        }

        public bool TryGet(int x, int y, [MaybeNullWhen(false)] out TTarget data)
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

                return default!;
            }
        }

        public Rectangle Bounds => source.Bounds;

        public bool Contains(int x, int y)
        {
            return source.Contains(x, y);
        }
    }
}