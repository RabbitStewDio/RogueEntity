using System.Diagnostics.CodeAnalysis;
using RogueEntity.Core.Positioning.MapLayers;

namespace RogueEntity.Core.Tests
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