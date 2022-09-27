using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Positioning.MapLayers;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace RogueEntity.Core.MapLoading.Builder
{
    public class MapBuilder
    {
        readonly Dictionary<byte, IMapBuilderLayer> layerProcessors;
        readonly List<MapLayer> mapLayers;

        public MapBuilder()
        {
            mapLayers = new List<MapLayer>();
            layerProcessors = new Dictionary<byte, IMapBuilderLayer>();
        }

        public bool TryGetItemRegistry(MapLayer layer, [MaybeNullWhen(false)] out IItemRegistry reg)
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
            where T : struct, IEntityKey
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
                mapLayers.Add(mapLayer);
                mapLayers.Sort(MapLayer.LayerIdComparer);
            }

            return this;
        }

        public ReadOnlyListWrapper<MapLayer> Layers => mapLayers;

        public bool Instantiate(ItemDeclarationId item, Position pos, IMapBuilderInstantiationLifter? postProcessor = null)
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

        public bool Clear(Position pos, IMapBuilderInstantiationLifter? postProcessor = null)
        {
            if (pos.IsInvalid)
            {
                return false;
            }

            // Indeterminate layer
            if (pos.LayerId == 0)
            {
                var result = true;
                foreach (var l in layerProcessors)
                {
                    if (!l.Value.Clear(pos.WithLayer(l.Key), postProcessor))
                    {
                        result = false;
                    }
                }

                return result;
            }
            
            if (!layerProcessors.TryGetValue(pos.LayerId, out var layer))
            {
                return true;
            }

            return layer.Clear(pos, postProcessor);
        }
    }
}
