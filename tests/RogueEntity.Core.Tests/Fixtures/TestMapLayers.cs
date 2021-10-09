using RogueEntity.Core.Positioning.MapLayers;
using System.Diagnostics.CodeAnalysis;

namespace RogueEntity.Core.Tests.Fixtures
{
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public static class TestMapLayers
    {
        public static readonly MapLayer Indeterminate;
        public static readonly MapLayerRegistry Registry;
        public static readonly MapLayer One;
        public static readonly MapLayer Two;
        public static readonly MapLayer Three;
        public static readonly MapLayer Four;
        public static readonly MapLayer Five;

        public static MapLayer Ground => One;
        public static MapLayer Structure => Two;
        public static MapLayer Items => Three;
        public static MapLayer Actors => Four;
        public static MapLayer Effects => Five;
        
        static TestMapLayers()
        {
            Registry = new MapLayerRegistry();
            Indeterminate = Registry.Indeterminate;
            One = Registry.Create("Layer 1 (Floor)");
            Two = Registry.Create("Layer 2 (Structure)");
            Three = Registry.Create("Layer 3 (Items)");
            Four = Registry.Create("Layer 4 (Actors)");
            Five = Registry.Create("Layer 5 (Effects)");
        }

    }
}