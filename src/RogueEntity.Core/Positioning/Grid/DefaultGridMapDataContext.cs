using System;
using JetBrains.Annotations;
using RogueEntity.Core.Positioning.MapLayers;
using RogueEntity.Core.Utils.DataViews;

namespace RogueEntity.Core.Positioning.Grid
{
    public sealed class DefaultGridMapDataContext<TItemId> : DynamicDataView3D<TItemId>, IGridMapDataContext<TItemId>
    {
        public event EventHandler<PositionDirtyEventArgs> PositionDirty;

        public DefaultGridMapDataContext(MapLayer layer, int tileWidth, int tileHeight) : this(layer, 0, 0, tileWidth, tileHeight) { }

        public DefaultGridMapDataContext(MapLayer layer, int offsetX, int offsetY, int tileSizeX, int tileSizeY):
            base(offsetX, offsetY, tileSizeX, tileSizeY)
        {
            this.Layer = layer;
        }
        
        public DefaultGridMapDataContext(MapLayer layer, DynamicDataViewConfiguration conf): base(conf)
        {
            this.Layer = layer;
        }

        [UsedImplicitly] 
        public MapLayer Layer { get; }

        public void MarkDirty<TPosition>(in TPosition position)
            where TPosition : IPosition<TPosition>
        {
            PositionDirty?.Invoke(this, new PositionDirtyEventArgs(Position.From(position)));
        }
    }
}