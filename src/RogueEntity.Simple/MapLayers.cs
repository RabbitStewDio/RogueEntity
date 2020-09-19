using RogueEntity.Core.Positioning.MapLayers;

namespace RogueEntity.Simple
{
    public static class MapLayers
    {
        public static readonly MapLayer Any = new MapLayer(0, "map.layer.ground");
        public static readonly MapLayer Ground = new MapLayer(1, "map.layer.ground");
        public static readonly MapLayer Items = new MapLayer(2, "map.layer.items");
        public static readonly MapLayer Actor = new MapLayer(3, "map.layer.actor");
        public static readonly MapLayer StatusEffects = new MapLayer(4, "map.layer.status-fx");
    }
}