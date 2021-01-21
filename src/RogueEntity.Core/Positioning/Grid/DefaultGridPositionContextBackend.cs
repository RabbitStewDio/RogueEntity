using System;
using System.Collections.Generic;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Positioning.MapLayers;
using RogueEntity.Core.Utils.DataViews;

namespace RogueEntity.Core.Positioning.Grid
{
    public class DefaultGridPositionContextBackend<TItemId> : IGridMapContext<TItemId>
    {
        readonly Dictionary<byte, IGridMapDataContext<TItemId>> mapLayerData;
        readonly List<MapLayer> mapLayers;

        public DefaultGridPositionContextBackend(): this(0, 0, 32, 32)
        {
        }

        public DefaultGridPositionContextBackend(IGridMapConfiguration<TItemId> config): this(config.OffsetX, config.OffsetY, config.TileSizeX, config.TileSizeY)
        {
        }

        public DefaultGridPositionContextBackend(in DynamicDataViewConfiguration config): this(config.OffsetX, config.OffsetY, config.TileSizeX, config.TileSizeY)
        {
        }

        public DefaultGridPositionContextBackend(int tileSizeX, int tileSizeY): this(0, 0, tileSizeX, tileSizeY)
        {
        }

        public DefaultGridPositionContextBackend(int offsetX, int offsetY, int tileSizeX, int tileSizeY)
        {
            OffsetX = offsetX;
            OffsetY = offsetY;
            TileSizeX = tileSizeX;
            TileSizeY = tileSizeY;
            mapLayers = new List<MapLayer>();
            mapLayerData = new Dictionary<byte, IGridMapDataContext<TItemId>>();
        }

        public int OffsetX { get; }
        public int OffsetY { get; }
        public int TileSizeX { get; }
        public int TileSizeY { get; }

        public DefaultGridPositionContextBackend<TItemId> WithMapLayer(MapLayer layer, IGridMapDataContext<TItemId> data)
        {
            if (layer == MapLayer.Indeterminate)
            {
                throw new ArgumentException();
            }
            
            if (mapLayerData.ContainsKey(layer.LayerId))
            {
                throw new ArgumentException($"Layer {layer} has already been declared.");
            }

            mapLayers.Add(layer);
            mapLayerData[layer.LayerId] = data;
            return this;
        }

        public DefaultGridPositionContextBackend<TItemId> WithDefaultMapLayer(MapLayer layer)
        {
            return WithMapLayer(layer, new DefaultGridMapDataContext<TItemId>(layer, OffsetX, OffsetY, TileSizeX, TileSizeY));
        }

        public DefaultGridPositionContextBackend<TItemId> WithDefaultMapLayer(MapLayer layer, DynamicDataViewConfiguration conf)
        {
            return WithMapLayer(layer, new DefaultGridMapDataContext<TItemId>(layer, conf));
        }
        
        public DefaultGridPositionContextBackend<TItemId> WithDefaultMapLayer(MapLayer layer, int offsetX, int offsetY, int tileWidth, int tileHeight)
        {
            return WithMapLayer(layer, new DefaultGridMapDataContext<TItemId>(layer, offsetX, offsetY, tileWidth, tileHeight));
        }
        
        public DefaultGridPositionContextBackend<TItemId> WithDefaultMapLayer(MapLayer layer, int tileWidth, int tileHeight)
        {
            return WithMapLayer(layer, new DefaultGridMapDataContext<TItemId>(layer, tileWidth, tileHeight));
        }

        public ReadOnlyListWrapper<MapLayer> GridLayers()
        {
            return mapLayers;
        }

        public bool TryGetGridDataFor(MapLayer layer, out IGridMapDataContext<TItemId> data)
        {
            return mapLayerData.TryGetValue(layer.LayerId, out data);
        }

        public bool TryGetGridDataFor(byte layerId, out IGridMapDataContext<TItemId> data)
        {
            return mapLayerData.TryGetValue(layerId, out data);
        }
    }
}