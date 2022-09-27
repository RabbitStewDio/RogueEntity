using System;
using System.Collections.Generic;
using RogueEntity.Api.Utils;
using System.Diagnostics.CodeAnalysis;


namespace RogueEntity.Core.Utils.DataViews
{
    public class TransformedView2D<TSource, TTarget> : IReadOnlyDynamicDataView2D<TTarget>,
                                                       IDisposable
    {
#pragma warning disable CS0067 
        public event EventHandler<DynamicDataView2DEventArgs<TTarget>>? ViewChunkCreated;
        public event EventHandler<DynamicDataView2DEventArgs<TTarget>>? ViewChunkExpired;
#pragma warning restore CS0067 
        readonly IReadOnlyDynamicDataView2D<TSource> source;
        readonly Func<TSource, TTarget> transformation;
        readonly Dictionary<TileIndex, TransformedBoundedDataView<TSource, TTarget>> index;

        public TransformedView2D(IReadOnlyDynamicDataView2D<TSource> source,
                                 Func<TSource, TTarget> transformation)
        {
            this.index = new Dictionary<TileIndex, TransformedBoundedDataView<TSource, TTarget>>();
            this.source = source ?? throw new ArgumentNullException(nameof(source));
            this.transformation = transformation ?? throw new ArgumentNullException(nameof(transformation));

            this.source.ViewChunkExpired += OnViewChunkExpired;
        }

        ~TransformedView2D()
        {
            ReleaseUnmanagedResources();
        }

        void ReleaseUnmanagedResources()
        {
            this.source.ViewChunkExpired += OnViewChunkExpired;
        }

        public void Dispose()
        {
            ReleaseUnmanagedResources();
            GC.SuppressFinalize(this);
        }

        void OnViewChunkExpired(object sender, DynamicDataView2DEventArgs<TSource> e)
        {
            var dataBounds = e.Key;
            if (index.TryGetValue(dataBounds, out var ownData))
            {
                ViewChunkExpired?.Invoke(this, new DynamicDataView2DEventArgs<TTarget>(dataBounds, ownData));
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

        public BufferList<Rectangle> GetActiveTiles(BufferList<Rectangle>? data = null)
        {
            return source.GetActiveTiles(data);
        }

        public Rectangle GetActiveBounds()
        {
            return source.GetActiveBounds();
        }

        public bool TryGetData(int x, int y, [MaybeNullWhen(false)] out IReadOnlyBoundedDataView<TTarget> raw)
        {
            var dx = DataViewPartitions.TileSplit(x, OffsetX, TileSizeX);
            var dy = DataViewPartitions.TileSplit(y, OffsetY, TileSizeY);
            var key = new TileIndex(dx, dy);
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
    }
}