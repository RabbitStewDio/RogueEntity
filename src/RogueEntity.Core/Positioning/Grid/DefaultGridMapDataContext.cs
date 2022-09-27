using System;
using JetBrains.Annotations;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Positioning.MapLayers;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.DataViews;

namespace RogueEntity.Core.Positioning.Grid
{
    public sealed class DefaultGridMapDataContext<TItemId> : PooledDynamicDataView3D<TItemId>, IGridMapDataContext<TItemId>, IGridMapContextInitializer<TItemId>
    {
        public event EventHandler<PositionDirtyEventArgs>? PositionDirty;
        public event EventHandler<MapRegionDirtyEventArgs>? RegionDirty;

        public DefaultGridMapDataContext(MapLayer layer, int tileWidth, int tileHeight) : this(layer, 0, 0, tileWidth, tileHeight) { }

        public DefaultGridMapDataContext(MapLayer layer, int offsetX, int offsetY, int tileSizeX, int tileSizeY):
            this(layer, new DynamicDataViewConfiguration(offsetX, offsetY, tileSizeX, tileSizeY))
        {
        }
        
        public DefaultGridMapDataContext(MapLayer layer, DynamicDataViewConfiguration conf): base(new DefaultBoundedDataViewPool<TItemId>(conf))
        {
            this.Layer = layer;
            this.ViewExpired += OnViewExpired;
        }

        void OnViewExpired(object sender, DynamicDataView3DEventArgs<TItemId> e)
        {
            MarkRegionDirty(e.ZLevel, e.ZLevel);
        }

        public void ResetLevel(int z)
        {
            RemoveView(z);
        }

        public void ResetState()
        {
            ExpireAll();
        }

        [UsedImplicitly] 
        public MapLayer Layer { get; }

        public void MarkDirty<TPosition>(in TPosition position)
            where TPosition : IPosition<TPosition>
        {
            PositionDirty?.Invoke(this, new PositionDirtyEventArgs(Position.From(position)));
        }

        public void MarkRegionDirty(int zPositionFrom, int zPositionTo, Optional<Rectangle> layerArea = default)
        {
            RegionDirty?.Invoke(this, new MapRegionDirtyEventArgs(zPositionFrom, zPositionTo, layerArea));
        }
    }
}