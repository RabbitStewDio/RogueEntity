using RogueEntity.Core.Positioning.MapLayers;

namespace RogueEntity.Simple.Demo.BoxPusher
{
    public static class BoxPusherMapLayers
    {
        public static readonly MapLayerRegistry Registry = new MapLayerRegistry();

        public static readonly MapLayer Floor;
        public static readonly MapLayer Items;
        public static readonly MapLayer Actors;
        
        static BoxPusherMapLayers()
        {
            Floor = Registry.Create("Floor");
            Items = Registry.Create("Items");
            Actors = Registry.Create("Actors");
        }
    }
}