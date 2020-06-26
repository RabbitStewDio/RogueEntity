using RogueEntity.Core.Utils.Maps;

namespace RogueEntity.Core.Infrastructure.Positioning
{
    public static class PositionExtensions
    {
        public static bool IsAcceptable<TPosition>(this MapLayerPreference pref, 
                                                   TPosition p, out MapLayer layerId) 
            where TPosition: IPosition
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