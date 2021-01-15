using EnTTSharp.Entities;
using RogueEntity.Core.Positioning.MapLayers;

namespace RogueEntity.Core.Positioning.Continuous
{
    public class ContinuousPositionCacheControlSystem<TItemId>
        where TItemId : IEntityKey
    {
        readonly IMapLayerContext mapLayerContext;
        readonly IContinuousMapContext<TItemId> mapContext;

        public ContinuousPositionCacheControlSystem(IMapLayerContext mapLayerContext, IContinuousMapContext<TItemId> mapContext)
        {
            this.mapLayerContext = mapLayerContext;
            this.mapContext = mapContext;
        }

        public void ApplyContinuousMovementChangeTracker(IEntityViewControl<TItemId> v,
                                                         TItemId k,
                                                         in ContinuousMapPositionChangedMarker oldPosition)
        {
            if (oldPosition.PreviousPosition != ContinuousMapPosition.Invalid)
            {
                if (mapLayerContext.MapLayerRegistry.TryGetValue(oldPosition.PreviousPosition.LayerId, out var layer) &&
                    mapContext.TryGetContinuousDataFor(layer, out var mapData))
                {
                    mapData.MarkDirty(oldPosition.PreviousPosition);
                }
            }

            if (v.GetComponent(k, out ContinuousMapPosition p))
            {
                if (mapLayerContext.MapLayerRegistry.TryGetValue(p.LayerId, out var layer) &&
                    mapContext.TryGetContinuousDataFor(layer, out var mapData))
                {
                    mapData.MarkDirty(p);
                }
            }
        }
    }
}
