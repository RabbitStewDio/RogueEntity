using RogueEntity.Api.Utils;
using System;
using System.Collections.Generic;

namespace RogueEntity.Core.Utils.DataViews
{
    public class PooledDynamicDataView3D<T>: IDynamicDataView3D<T>
    {
        readonly IBoundedDataViewPool<T> pool;
        readonly Dictionary<int, PooledDynamicDataView2D<T>> index;

        public event EventHandler<DynamicDataView3DEventArgs<T>> ViewCreated;
        public event EventHandler<DynamicDataView3DEventArgs<T>> ViewExpired;
        public int OffsetX => pool.TileConfiguration.OffsetX;
        public int OffsetY => pool.TileConfiguration.OffsetY;
        public int TileSizeX => pool.TileConfiguration.TileSizeX;
        public int TileSizeY => pool.TileConfiguration.TileSizeY;

        public PooledDynamicDataView3D(IBoundedDataViewPool<T> pool)
        {
            this.pool = pool ?? throw new ArgumentNullException(nameof(pool));
            this.index = new Dictionary<int, PooledDynamicDataView2D<T>>();
        }

        public bool RemoveView(int z)
        {
            if (index.TryGetValue(z, out var rdata))
            {
                ViewExpired?.Invoke(this, new DynamicDataView3DEventArgs<T>(z, rdata));
                index.Remove(z);
                return true;
            }

            return false;
        }

        public bool TryGetView(int z, out IReadOnlyDynamicDataView2D<T> view)
        {
            if (TryGetWritableView(z, out var raw))
            {
                view = raw;
                return true;
            }

            view = default;
            return false;
        }

        public bool TryGetWritableView(int z, out IDynamicDataView2D<T> data, DataViewCreateMode mode = DataViewCreateMode.Nothing)
        {
            if (index.TryGetValue(z, out var rdata))
            {
                data = rdata;
                return true;
            }

            if (mode == DataViewCreateMode.Nothing)
            {
                data = default;
                return false;
            }

            rdata = new PooledDynamicDataView2D<T>(pool);
            index[z] = rdata;
            ViewCreated?.Invoke(this, new DynamicDataView3DEventArgs<T>(z, rdata));
            data = rdata;
            return true;
        }        

        public BufferList<int> GetActiveLayers(BufferList<int> buffer = null)
        {
            buffer = BufferList.PrepareBuffer(buffer);

            foreach (var b in index.Keys)
            {
                buffer.Add(b);
            }

            return buffer;
        }
    }
}
