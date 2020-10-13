using RogueEntity.Core.Positioning.MapLayers;

namespace RogueEntity.Simple
{
    public static class MapLayers
    {
        public static readonly MapLayer Any;
        public static readonly MapLayer Ground;
        public static readonly MapLayer Items;
        public static readonly MapLayer Actor;
        public static readonly MapLayer StatusEffects;

        static MapLayers()
        {
            StatusEffects = new MapLayer(4, "map.layer.status-fx");
            Any = new MapLayer(0, "map.layer.ground");
            Ground = new MapLayer(1, "map.layer.ground");
            Items = new MapLayer(2, "map.layer.items");
            Actor = new MapLayer(3, "map.layer.actor");
        }
    }
}