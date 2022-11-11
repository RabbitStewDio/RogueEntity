using RogueEntity.Api.Utils;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace RogueEntity.Core.Utils.DataViews
{
    /// <summary>
    ///   An aggregation event used when listing to chunk changes in a 3D pooled data view.
    ///   Instead of having to subscribe to each individual data layer this event aggregates
    ///   all inner events into a single event stream enriched with layer index information. 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public readonly struct PooledDataView3DChunkEventArgs<T>
    {
        public readonly int LayerIndex;
        public readonly TileIndex Key;
        public readonly IReadOnlyBoundedDataView<T> Data;
        public readonly IReadOnlyDynamicDataView2D<T> LayerView;

        public PooledDataView3DChunkEventArgs(int layerIndex, 
                                              TileIndex key, 
                                              IReadOnlyBoundedDataView<T> data, 
                                              IReadOnlyDynamicDataView2D<T> layerView)
        {
            LayerIndex = layerIndex;
            Key = key;
            Data = data;
            LayerView = layerView;
        }
    }

    public class PooledDynamicDataView3D<T> : IDynamicDataView3D<T>, IPooledDataViewControl3D
    {
        readonly IBoundedDataViewPool<T> pool;
        readonly Dictionary<int, EventForwarder> index;

        /// <summary>
        ///    Fired when a data view instance / z-layer has been created.
        /// </summary>
        public event EventHandler<DynamicDataView3DEventArgs<T>>? ViewCreated;
        /// <summary>
        ///    Fired when a data view / z-layer has been cleared / its state reset.
        /// </summary>
        public event EventHandler<DynamicDataView3DEventArgs<T>>? ViewReset;
        /// <summary>
        ///    Fired when a data view instance has been removed.
        /// </summary>
        public event EventHandler<DynamicDataView3DEventArgs<T>>? ViewExpired;
        
        public event EventHandler<PooledDataView3DChunkEventArgs<T>>? ViewChunkCreated;
        public event EventHandler<PooledDataView3DChunkEventArgs<T>>? ViewChunkExpired;
        
        public int OffsetX => pool.TileConfiguration.OffsetX;
        public int OffsetY => pool.TileConfiguration.OffsetY;
        public int TileSizeX => pool.TileConfiguration.TileSizeX;
        public int TileSizeY => pool.TileConfiguration.TileSizeY;

        public PooledDynamicDataView3D(IBoundedDataViewPool<T> pool)
        {
            this.pool = pool ?? throw new ArgumentNullException(nameof(pool));
            this.index = new Dictionary<int, EventForwarder>();
        }

        public bool RemoveView(int z)
        {
            if (index.TryGetValue(z, out var rdata))
            {
                ViewExpired?.Invoke(this, new DynamicDataView3DEventArgs<T>(z, rdata.View));
                rdata.View.ExpireAll();
                rdata.Dispose();
                index.Remove(z);
                return true;
            }

            return false;
        }

        /// <summary>
        ///   Expires all content. This does not remove the actual data view, just resets it
        ///   to its initial empty state.
        /// </summary>
        public void ExpireAll()
        {
            var length = index.Count;
            var k = ArrayPool<int>.Shared.Rent(length);
            index.Keys.CopyTo(k, 0);
            for (var i = 0; i < length; i++)
            {
                var z = k[i];
                if (index.TryGetValue(z, out var rdata))
                {
                    ViewReset?.Invoke(this, new DynamicDataView3DEventArgs<T>(z, rdata.View));
                    rdata.View.ExpireAll();
                }
            }

            ArrayPool<int>.Shared.Return(k);
        }

        public void Clear()
        {
            foreach (var v in index.Values)
            {
                v.View.Clear();
            }
        }

        public void PrepareFrame(long time)
        {
            foreach (var v in index.Values)
            {
                v.View.PrepareFrame(time);
            }
        }

        public void ExpireFrames(long age)
        {
            foreach (var v in index.Values)
            {
                v.View.ExpireFrames(age);
            }
        }

        public bool TryGetView(int z, [MaybeNullWhen(false)] out IReadOnlyDynamicDataView2D<T> view)
        {
            if (TryGetWritableView(z, out var raw))
            {
                view = raw;
                return true;
            }

            view = default;
            return false;
        }

        public bool TryGetWritableView(int z, [MaybeNullWhen(false)] out IDynamicDataView2D<T> data, DataViewCreateMode mode = DataViewCreateMode.Nothing)
        {
            if (index.TryGetValue(z, out var rdata))
            {
                data = rdata.View;
                return true;
            }

            if (mode == DataViewCreateMode.Nothing)
            {
                data = default;
                return false;
            }

            var eventHandler = new EventForwarder(this, z, CreateView(pool));
            index[z] = eventHandler;
            ViewCreated?.Invoke(this, new DynamicDataView3DEventArgs<T>(z, eventHandler.View));
            data = eventHandler.View;
            return true;
        }

        protected virtual PooledDynamicDataView2D<T> CreateView(IBoundedDataViewPool<T> pool)
        {
            return new PooledDynamicDataView2D<T>(pool);
        }

        public BufferList<int> GetActiveLayers(BufferList<int>? buffer = null)
        {
            buffer = BufferList.PrepareBuffer(buffer);

            foreach (var b in index.Keys)
            {
                buffer.Add(b);
            }

            return buffer;
        }

        protected void FireViewChunkCreatedEvent(int z, TileIndex idx, IReadOnlyDynamicDataView2D<T> data, IReadOnlyBoundedDataView<T> viewChunk)
        {
            ViewChunkCreated?.Invoke(this, new PooledDataView3DChunkEventArgs<T>(z, idx, viewChunk, data));
        }
        
        protected void FireViewChunkExpiredEvent(int z, TileIndex idx, IReadOnlyDynamicDataView2D<T> data, IReadOnlyBoundedDataView<T> viewChunk)
        {
            ViewChunkExpired?.Invoke(this, new PooledDataView3DChunkEventArgs<T>(z, idx, viewChunk, data));
        }
        
        class EventForwarder: IDisposable
        {
            readonly int z;
            readonly PooledDynamicDataView3D<T> parent;
            public readonly PooledDynamicDataView2D<T> View;

            public EventForwarder(PooledDynamicDataView3D<T> parent, int z, PooledDynamicDataView2D<T> view)
            {
                this.z = z;
                this.parent = parent;
                this.View = view;
                this.View.ViewChunkCreated += OnViewChunkCreated;
                this.View.ViewChunkExpired += OnViewChunkExpired;
            }

            void OnViewChunkCreated(object sender, DynamicDataView2DEventArgs<T> e)
            {
                parent.FireViewChunkCreatedEvent(z, e.Key, View, e.Data);
            }
            
            void OnViewChunkExpired(object sender, DynamicDataView2DEventArgs<T> e)
            {
                parent.FireViewChunkCreatedEvent(z, e.Key, View, e.Data);
            }

            public void Dispose()
            {
                this.View.ViewChunkCreated -= OnViewChunkCreated;
                this.View.ViewChunkExpired -= OnViewChunkExpired;
            }
        }
    }
}
