using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace RogueEntity.Core.Utils.DataViews
{
    public class TransformedView3D<TSource, TTarget> : IReadOnlyDynamicDataView3D<TTarget>, IDisposable
    {
        public event EventHandler<DynamicDataView3DEventArgs<TTarget>> ViewCreated;
        public event EventHandler<DynamicDataView3DEventArgs<TTarget>> ViewExpired;

        readonly IReadOnlyDynamicDataView3D<TSource> source;
        readonly Func<TSource, TTarget> transformation;
        readonly Dictionary<int, TransformedView2D<TSource, TTarget>> index;

        public TransformedView3D([NotNull] IReadOnlyDynamicDataView3D<TSource> source,
                                 [NotNull] Func<TSource, TTarget> transformation)
        {
            this.source = source ?? throw new ArgumentNullException(nameof(source));
            this.transformation = transformation ?? throw new ArgumentNullException(nameof(transformation));
            this.index = new Dictionary<int, TransformedView2D<TSource, TTarget>>();

            this.source.ViewExpired += OnViewExpired;
        }

        ~TransformedView3D()
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

        void OnViewExpired(object sender, DynamicDataView3DEventArgs<TSource> e)
        {
            var dataBounds = e.ZLevel;
            if (index.TryGetValue(dataBounds, out var ownData))
            {
                ViewExpired?.Invoke(this, new DynamicDataView3DEventArgs<TTarget>(dataBounds, ownData));
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

        public List<int> GetActiveLayers(List<int> buffer = null)
        {
            return source.GetActiveLayers(buffer);
        }

        public bool TryGetView(int z, out IReadOnlyDynamicDataView2D<TTarget> view)
        {
            if (index.TryGetValue(z, out var targetRaw))
            {
                view = targetRaw;
                return true;
            }
            
            if (source.TryGetView(z, out var sourceRaw))
            {
                targetRaw = new TransformedView2D<TSource,TTarget>(sourceRaw, transformation);
                index[z] = targetRaw;
                view = targetRaw;
                return true;
            }
            
            view = default;
            return false;
        }
    }
}