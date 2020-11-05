using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using RogueEntity.Core.Positioning.MapLayers;
using RogueEntity.Core.Utils.DataViews;

namespace RogueEntity.Core.Positioning.Grid
{
    public sealed class DefaultGridMapDataContext<TItemId> : IGridMapDataContext<TItemId>
    {
        public event EventHandler<PositionDirtyEventArgs> PositionDirty;

        readonly int offsetX;
        readonly int offsetY;
        readonly int tileSizeX;
        readonly int tileSizeY;
        readonly Dictionary<int, IDynamicDataView2D<TItemId>> mapDataByDepth;

        public DefaultGridMapDataContext(MapLayer layer, int tileWidth, int tileHeight) : this(layer, 0, 0, tileWidth, tileHeight) { }

        public DefaultGridMapDataContext(MapLayer layer, int offsetX, int offsetY, int tileSizeX, int tileSizeY)
        {
            this.Layer = layer;
            this.offsetX = offsetX;
            this.offsetY = offsetY;
            this.tileSizeX = tileSizeX;
            this.tileSizeY = tileSizeY;
            mapDataByDepth = new Dictionary<int, IDynamicDataView2D<TItemId>>();
        }
        
        [UsedImplicitly] 
        public MapLayer Layer { get; }

        public int OffsetX => offsetX;

        public int OffsetY => offsetY;

        public int TileSizeX => tileSizeX;

        public int TileSizeY => tileSizeY;

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

            foreach (var b in mapDataByDepth.Keys)
            {
                buffer.Add(b);
            }

            return buffer;
        }

        public bool TryGetView(int z, out IReadOnlyDynamicDataView2D<TItemId> view)
        {
            if (TryGetWritableView(z, out var raw))
            {
                view = raw;
                return true;
            }

            view = default;
            return false;
        }

        public bool TryGetWritableView(int z, out IDynamicDataView2D<TItemId> data, DataViewCreateMode mode = DataViewCreateMode.Nothing)
        {
            if (mapDataByDepth.TryGetValue(z, out var rdata))
            {
                data = rdata;
                return true;
            }

            if (mode == DataViewCreateMode.Nothing)
            {
                data = default;
                return false;
            }

            rdata = new DynamicDataView<TItemId>(offsetX, offsetY, tileSizeX, tileSizeY);
            mapDataByDepth[z] = rdata;
            data = rdata;
            return true;
        }

        public void MarkDirty<TPosition>(in TPosition position)
            where TPosition : IPosition
        {
            PositionDirty?.Invoke(this, new PositionDirtyEventArgs(Position.From(position)));
        }
    }
}