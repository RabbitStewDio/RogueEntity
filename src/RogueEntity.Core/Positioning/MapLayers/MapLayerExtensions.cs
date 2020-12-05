namespace RogueEntity.Core.Positioning.MapLayers
{
    public static class MapLayerExtensions
    {
        public static bool IsAcceptable<TPosition>(this MapLayerPreference pref, 
                                                   TPosition p, 
                                                   out MapLayer layerId) 
            where TPosition: IPosition<TPosition>
        {
            foreach (var l in pref.AcceptableLayers)
            {
                if (l.LayerId == p.LayerId)
                {
                    layerId = l;
                    return true;
                }
            }

            layerId = default;
            return false;
        }
    }
}