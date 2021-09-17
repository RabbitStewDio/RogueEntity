using JetBrains.Annotations;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Positioning.MapLayers;
using RogueEntity.Core.Utils.DataViews;
using System;

namespace RogueEntity.SadCons.MapRendering
{
    public class GridEntityToTagConverter<TMapData> : IEntityToTagConverter
    {
        readonly IGridMapDataContext<TMapData> mapData;
        readonly Func<TMapData, WorldEntityTag> tagConverter;
        Optional<int> cachedLayer;
        IReadOnlyDynamicDataView2D<TMapData> cachedMapData;

        public GridEntityToTagConverter(MapLayer layer,
                                        [NotNull] IGridMapDataContext<TMapData> mapData,
                                        [NotNull] Func<TMapData, WorldEntityTag> tagConverter)
        {
            this.mapData = mapData ?? throw new ArgumentNullException(nameof(mapData));
            this.tagConverter = tagConverter ?? throw new ArgumentNullException(nameof(tagConverter));
            this.Layer = layer;
        }

        public MapLayer Layer { get; }

        public bool TryFetchTag(Position p, out WorldEntityTag t)
        {
            if (!cachedLayer.TryGetValue(out var cachedZLayer) || p.GridZ != cachedZLayer)
            {
                if (!mapData.TryGetView(p.GridZ, out cachedMapData))
                {
                    t = default;
                    return false;
                }

                cachedLayer = p.GridZ;
            }

            if (!cachedMapData.TryGet(p.GridX, p.GridY, out var data))
            {
                t = default;
                return false;
            }

            t = tagConverter(data);
            return true;
        }
    }
}
