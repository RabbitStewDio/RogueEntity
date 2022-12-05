using JetBrains.Annotations;
using RogueEntity.Api.ItemTraits;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Positioning.MapLayers;
using System;

namespace RogueEntity.SadCons.MapRendering
{
    public class GridEntityToTagConverter<TMapData> : IEntityToTagConverter
    {
        readonly IMapDataContext<TMapData> mapData;
        readonly Func<TMapData, WorldEntityTag> tagConverter;
        BufferList<(TMapData, EntityGridPosition)> buffer;

        public GridEntityToTagConverter(MapLayer layer,
                                        [NotNull] IMapDataContext<TMapData> mapData,
                                        [NotNull] Func<TMapData, WorldEntityTag> tagConverter)
        {
            this.mapData = mapData ?? throw new ArgumentNullException(nameof(mapData));
            this.tagConverter = tagConverter ?? throw new ArgumentNullException(nameof(tagConverter));
            this.Layer = layer;
            this.buffer = new BufferList<(TMapData, EntityGridPosition)>();
        }

        public MapLayer Layer { get; }

        public bool TryFetchTag(Position p, out WorldEntityTag t)
        {
            foreach (var (data, _) in mapData.QueryItemTile(EntityGridPosition.From(p), buffer))
            {
                t = tagConverter(data);
                return true;
            }

            t = default;
            return false;
        }
    }
}
