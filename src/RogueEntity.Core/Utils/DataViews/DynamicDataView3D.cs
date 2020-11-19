using System;
using System.Collections.Generic;

namespace RogueEntity.Core.Utils.DataViews
{
    public class DynamicDataView3D<T>: IDynamicDataView3D<T>
    {
        public event EventHandler<DynamicDataView3DEventArgs<T>> ViewCreated;
        public event EventHandler<DynamicDataView3DEventArgs<T>> ViewExpired;
        public int OffsetX { get; }
        public int OffsetY { get; }
        public int TileSizeX { get; }
        public int TileSizeY { get; }
        readonly Dictionary<int, IDynamicDataView2D<T>> index;

        public DynamicDataView3D(): this(0, 0, 64, 64)
        {
        }

        public DynamicDataView3D(int tileSizeX, int tileSizeY): this(0, 0, tileSizeX, tileSizeY)
        {
            
        }
        
        public DynamicDataView3D(int offsetX, int offsetY, int tileSizeX, int tileSizeY)
        {
            OffsetX = offsetX;
            OffsetY = offsetY;
            TileSizeX = tileSizeX;
            TileSizeY = tileSizeY;
            
            index = new Dictionary<int, IDynamicDataView2D<T>>();
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

            rdata = new DynamicDataView2D<T>(OffsetX, OffsetY, TileSizeX, TileSizeY);
            index[z] = rdata;
            ViewCreated?.Invoke(this, new DynamicDataView3DEventArgs<T>(z, rdata));
            data = rdata;
            return true;
        }        

        public List<int> GetActiveLayers(List<int> buffer = null)
        {
            if (buffer == null)
            {
                buffer = new List<int>();
            }
            else
            {
                buffer.Clear();
            }

            foreach (var b in index.Keys)
            {
                buffer.Add(b);
            }

            return buffer;
        }
    }
}