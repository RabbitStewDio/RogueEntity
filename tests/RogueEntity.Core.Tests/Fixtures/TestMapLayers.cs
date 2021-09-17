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

        public static MapLayer Ground => One;
        public static MapLayer Items => Two;
        public static MapLayer Actors => Three;
        public static MapLayer Effects => Four;
        
        static TestMapLayers()
        {
            Registry = new MapLayerRegistry();
            Indeterminate = Registry.Indeterminate;
            One = Registry.Create("Layer 1");
            Two = Registry.Create("Layer 2");
            Three = Registry.Create("Layer 3");
            Four = Registry.Create("Layer 4");
        }

    }
}