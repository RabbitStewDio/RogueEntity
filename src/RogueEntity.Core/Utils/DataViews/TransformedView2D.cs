using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using RogueEntity.Api.Utils;

namespace RogueEntity.Core.Utils.DataViews
{
    public class TransformedView2D<TSource, TTarget> : IReadOnlyDynamicDataView2D<TTarget>,
                                                       IDisposable
    {
        public event EventHandler<DynamicDataView2DEventArgs<TTarget>> ViewCreated;
        public event EventHandler<DynamicDataView2DEventArgs<TTarget>> ViewExpired;
        readonly IReadOnlyDynamicDataView2D<TSource> source;
        readonly Func<TSource, TTarget> transformation;
        readonly Dictionary<Position2D, TransformedBoundedDataView<TSource, TTarget>> index;

        public TransformedView2D([NotNull] IReadOnlyDynamicDataView2D<TSource> source,
                                 [NotNull] Func<TSource, TTarget> transformation)
        {
            this.index = new Dictionary<Position2D, TransformedBoundedDataView<TSource, TTarget>>();
            this.source = source ?? throw new ArgumentNullException(nameof(source));
            this.transformation = transformation ?? throw new ArgumentNullException(nameof(transformation));

            this.source.ViewExpired += OnViewExpired;
        }

        ~TransformedView2D()
        {
            ReleaseUnmanagedResources();
        }

        void ReleaseUnmanagedResources()
        {
            this.source.ViewExpired += OnViewExpired;
        }

        public void Dispose()
        {
            ReleaseUnmanagedResources();
            GC.SuppressFinalize(this);
        }

        void OnViewExpired(object sender, DynamicDataView2DEventArgs<TSource> e)
        {
            var dataBounds = e.Key;
            if (index.TryGetValue(dataBounds, out var ownData))
            {
                ViewExpired?.Invoke(this, new DynamicDataView2DEventArgs<TTarget>(dataBounds, ownData));
                index.Remove(dataBounds);
            }
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

        public BufferList<Rectangle> GetActiveTiles(BufferList<Rectangle> data = null)
        {
            return source.GetActiveTiles(data);
        }

        public Rectangle GetActiveBounds()
        {
            return source.GetActiveBounds();
        }

        public bool TryGetData(int x, int y, out IReadOnlyBoundedDataView<TTarget> raw)
        {
            var dx = DataViewPartitions.TileSplit(x, OffsetX, TileSizeX);
            var dy = DataViewPartitions.TileSplit(y, OffsetY, TileSizeY);
            var key = new Position2D(dx, dy);
            if (index.TryGetValue(key, out var targetRaw))
            {
                raw = targetRaw;
                return true;
            }
            
            if (source.TryGetData(x, y, out var sourceRaw))
            {
                targetRaw = new TransformedBoundedDataView<TSource,TTarget>(sourceRaw, transformation);
                index[key] = targetRaw;
                raw = targetRaw;
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