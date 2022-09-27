using EnTTSharp.Entities;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Services;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Positioning.MapLayers;
using System;

namespace RogueEntity.SadCons.MapRendering
{
    public static class RenderLayers
    {
        public readonly struct GridMapLayerBuilder<TMapData>
        {
            readonly IGridMapContext<TMapData> map;
            readonly MapLayer layer;
            readonly Func<TMapData, WorldEntityTag> tagConverter;

            public GridMapLayerBuilder(IGridMapContext<TMapData> map) : this()
            {
                this.map = map;
                layer = default;
                tagConverter = default;
            }

            GridMapLayerBuilder(IGridMapContext<TMapData> map, MapLayer layer, Func<TMapData, WorldEntityTag> tagConverter)
            {
                this.map = map;
                this.layer = layer;
                this.tagConverter = tagConverter;
            }

            public GridMapLayerBuilder<TMapData> ForMapLayer(MapLayer l)
            {
                return new GridMapLayerBuilder<TMapData>(map, l, tagConverter);
            }

            public GridMapLayerBuilder<TMapData> WithConverter(Func<TMapData, WorldEntityTag> converter)
            {
                return new GridMapLayerBuilder<TMapData>(map, layer, converter);
            }

            public IEntityToTagConverter Build()
            {
                if (!map.TryGetGridDataFor(layer, out var data))
                {
                    throw new ArgumentException();
                }

                return new GridEntityToTagConverter<TMapData>(layer, data, tagConverter);
            }
        }

        public static Func<TMapData, WorldEntityTag> StandardTagConverter<TMapData>(IServiceResolver r)
            where TMapData : struct, IEntityKey
        {
            var itemResolver = r.Resolve<IItemResolver<TMapData>>();
            return (t) =>
            {
                if (t.IsEmpty)
                {
                    return default;
                }
                
                if (itemResolver.TryQueryData(t, out WorldEntityTag tag))
                {
                    return tag;
                }

                return default;
            };
        }

        public static GridMapLayerBuilder<TMapData> FromGrid<TMapData>(IGridMapContext<TMapData> map)
        {
            return new GridMapLayerBuilder<TMapData>(map);
        }
    }
}
