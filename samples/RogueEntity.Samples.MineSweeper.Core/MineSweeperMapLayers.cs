using RogueEntity.Core.Positioning.MapLayers;

namespace RogueEntity.Samples.MineSweeper.Core
{
    public static class MineSweeperMapLayers
    {
        static readonly MapLayerRegistry RegistryImpl;
        public static IMapLayerRegistry Registry => RegistryImpl;
        public static MapLayer Items;
        public static MapLayer Flags;

        static MineSweeperMapLayers()
        {
            RegistryImpl = new MapLayerRegistry();
            Items = RegistryImpl.Create("Items");
            Flags = RegistryImpl.Create("Flags");
        }
    }
}
