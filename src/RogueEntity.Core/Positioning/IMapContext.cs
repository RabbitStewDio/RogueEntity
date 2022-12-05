using RogueEntity.Api.Utils;
using RogueEntity.Core.Positioning.MapLayers;
using RogueEntity.Core.Utils.DataViews;

namespace RogueEntity.Core.Positioning
{
    public interface IMapContext<TItemId>
    {
        ReadOnlyListWrapper<MapLayer> Layers();
        bool TryGetMapDataFor(MapLayer layer, out IMapDataContext<TItemId> data);
        bool TryGetMapDataFor(byte layerId, out IMapDataContext<TItemId> data);
    }

    public interface IConfigurableMapContext<TItemId> : IMapContext<TItemId>
    {
        public DynamicDataViewConfiguration Config { get; }
        public IConfigurableMapContext<TItemId> WithMapLayer(MapLayer layer, IMapDataContext<TItemId> data);
    }
}