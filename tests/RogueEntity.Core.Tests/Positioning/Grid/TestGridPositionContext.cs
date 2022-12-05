using RogueEntity.Api.Utils;
using RogueEntity.Core.Meta.EntityKeys;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.MapLayers;
using RogueEntity.Core.Utils.DataViews;

namespace RogueEntity.Core.Tests.Positioning.Grid
{
    public class TestGridPositionContext : IMapContext<ItemReference>
    {
        readonly DefaultMapContext<ItemReference> map;

        public TestGridPositionContext()
        {
            map = new DefaultMapContext<ItemReference>(DynamicDataViewConfiguration.Default16X16);
        }

        public TestGridPositionContext WithMapLayer(MapLayer layer, IMapDataContext<ItemReference> data)
        {
            map.WithMapLayer(layer, data);
            return this;
        }

        public DynamicDataViewConfiguration Config => map.Config;

        public ReadOnlyListWrapper<MapLayer> Layers()
        {
            return map.Layers();
        }

        public bool TryGetMapDataFor(MapLayer layer, out IMapDataContext<ItemReference> data)
        {
            return map.TryGetMapDataFor(layer, out data);
        }

        public bool TryGetMapDataFor(byte layerId, out IMapDataContext<ItemReference> data)
        {
            return map.TryGetMapDataFor(layerId, out data);
        }
    }
}