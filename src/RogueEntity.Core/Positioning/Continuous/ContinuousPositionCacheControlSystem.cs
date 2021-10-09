using EnTTSharp.Entities;
using RogueEntity.Core.Positioning.MapLayers;

namespace RogueEntity.Core.Positioning.Continuous
{
    public class ContinuousPositionCacheControlSystem<TItemId>
        where TItemId : IEntityKey
    {
        readonly IMapLayerRegistry mapLayers;
        readonly IContinuousMapContext<TItemId> mapContext;

        public ContinuousPositionCacheControlSystem(IMapLayerRegistry mapLayers, IContinuousMapContext<TItemId> mapContext)
        {
            this.mapLayers = mapLayers;
            this.mapContext = mapContext;
        }

        public void ApplyContinuousMovementChangeTracker(IEntityViewControl<TItemId> v,
                                                         TItemId k,
                                                         in ContinuousMapPositionChangedMarker oldPosition)
        {
            if (oldPosition.PreviousPosition != ContinuousMapPosition.Invalid)
            {
                if (mapLayers.TryGetValue(oldPosition.PreviousPosition.LayerId, out var layer) &&
                    mapContext.TryGetContinuousDataFor(layer, out var mapData))
                {
                    mapData.MarkDirty(oldPosition.PreviousPosition);
                }
            }

            if (v.GetComponent(k, out ContinuousMapPosition p))
            {
                if (mapLayers.TryGetValue(p.LayerId, out var layer) &&
                    mapContext.TryGetContinuousDataFor(layer, out var mapData))
                {
                    mapData.MarkDirty(p);
                }
            }
        }
    }
}
