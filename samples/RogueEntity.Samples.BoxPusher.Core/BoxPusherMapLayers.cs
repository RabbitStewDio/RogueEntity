using RogueEntity.Core.Positioning.MapLayers;

namespace RogueEntity.Samples.BoxPusher.Core
{
    public static class BoxPusherMapLayers
    {
        public static readonly IMapLayerRegistry Registry;

        public static readonly MapLayer Floor;
        public static readonly MapLayer Items;
        public static readonly MapLayer Actors;
        
        static BoxPusherMapLayers()
        {
            var reg =  new MapLayerRegistry();
            Floor = reg.Create("Floor");
            Items = reg.Create("Items");
            Actors = reg.Create("Actors");
            Registry = reg;
        }
    }
}