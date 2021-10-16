using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Positioning.MapLayers;
using RogueEntity.Core.Utils.DataViews;
using System;
using System.Collections.Generic;

namespace RogueEntity.Core.MapLoading.Builder
{
    public class MapBuilder
    {
        public event EventHandler<MapRegionDirtyEventArgs> MapLayerDirty;

        readonly Dictionary<byte, EntityMapBuilderLayer> layerProcessors;
        readonly List<MapLayer> mapLayers;

        public MapBuilder()
        {
            mapLayers = new List<MapLayer>();
            layerProcessors = new Dictionary<byte, EntityMapBuilderLayer>();
        }

        public bool TryGetItemRegistry(MapLayer layer, out IItemRegistry reg)
        {
            if (layerProcessors.TryGetValue(layer.LayerId, out var layerData))
            {
                reg = layerData.ItemRegistry;
                return true;
            }

            reg = default;
            return false;
        }

        public MapBuilder WithLayer<T>(in MapLayer mapLayer,
                                       IItemResolver<T> itemResolver,
                                       IGridMapContext<T> gridContext,
                                       IItemPlacementServiceContext<T> placementService)
            where T : IEntityKey
        {
            if (mapLayer == MapLayer.Indeterminate)
            {
                throw new ArgumentException();
            }

            if (!layerProcessors.TryGetValue(mapLayer.LayerId, out _) &&
                gridContext.TryGetGridDataFor(mapLayer, out var gridData) &&
                placementService.TryGetItemPlacementService(mapLayer, out var ps))
            {
                layerProcessors.Add(mapLayer.LayerId, new MapBuilderLayer<T>(mapLayer, itemResolver, gridData, ps));
                gridData.RegionDirty += OnMapRegionDirty;
                gridData.ViewExpired += OnViewLayerDirty;
                gridData.ViewReset += OnViewLayerDirty;
                mapLayers.Add(mapLayer);
                mapLayers.Sort(MapLayer.LayerIdComparer);
            }

            return this;
        }

        void OnViewLayerDirty<T>(object sender, DynamicDataView3DEventArgs<T> e)
            where T : IEntityKey
        {
            MapLayerDirty?.Invoke(this, new MapRegionDirtyEventArgs(e.ZLevel, e.ZLevel));
        }

        void OnMapRegionDirty(object sender, MapRegionDirtyEventArgs e)
        {
            MapLayerDirty?.Invoke(this, e);
        }

        public ReadOnlyListWrapper<MapLayer> Layers => mapLayers;

        public bool Instantiate(ItemDeclarationId item, Position pos, IMapBuilderInstantiationLifter postProcessor = null)
        {
            if (pos.IsInvalid)
            {
                return false;
            }

            if (!layerProcessors.TryGetValue(pos.LayerId, out var layer))
            {
                return false;
            }

            return layer.Instantiate(item, pos, postProcessor);
        }

        public bool Clear(Position pos, IMapBuilderInstantiationLifter postProcessor = null)
        {
            if (pos.IsInvalid)
            {
                return false;
            }

            if (!layerProcessors.TryGetValue(pos.LayerId, out var layer))
            {
                return false;
            }

            return layer.Clear(pos, postProcessor);
        }
    }
}
