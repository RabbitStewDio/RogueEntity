using System.Collections.Generic;

namespace RogueEntity.Core.Utils.Maps
{
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