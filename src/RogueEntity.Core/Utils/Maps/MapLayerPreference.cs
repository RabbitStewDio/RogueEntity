using System.Collections.Generic;

namespace RogueEntity.Core.Utils.Maps
{
    /// <summary>
    ///   Defines an actor's or item's preferred and acceptable map-layers.
    ///   This information is a design time contract and should be stored as
    ///   part of the traits system. It should never be serialized into network
    ///   or save-game streams.
    /// </summary>
    public readonly struct MapLayerPreference
    {
        public readonly MapLayer PreferredLayer;
        public readonly ReadOnlyListWrapper<MapLayer> AcceptableLayers;

        public MapLayerPreference(MapLayer preferredLayer, 
                                  params MapLayer[] acceptableLayers) 
        {
            PreferredLayer = preferredLayer;
            var l = new List<MapLayer>();
            l.Add(preferredLayer);
            foreach (var a in acceptableLayers)
            {
                if (!l.Contains(a))
                {
                    l.Add(a);
                }
            }

            AcceptableLayers = l;
        }
    }
}