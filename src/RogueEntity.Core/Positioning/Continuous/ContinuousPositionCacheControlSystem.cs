using EnTTSharp.Entities;
using RogueEntity.Core.Positioning.MapLayers;

namespace RogueEntity.Core.Positioning.Continuous
{
    public static class ContinuousPositionCacheControlSystem
    {
        public static void ApplyContinuousMovementChangeTracker<TItemId, TGameContext>(IEntityViewControl<TItemId> v,
                                                                                       TGameContext context,
                                                                                       TItemId k,
                                                                                       in ContinuousMapPositionChangedMarker oldPosition)
            where TItemId : IEntityKey
            where TGameContext : IMapLayerContext, IContinuousMapContext<TGameContext, TItemId>
        {
            if (oldPosition.PreviousPosition != ContinuousMapPosition.Invalid)
            {
                if (context.MapLayerRegistry.TryGetValue(oldPosition.PreviousPosition.LayerId, out var layer) &&
                    context.TryGetContinuousDataFor(layer, out var mapData))
                {
                    mapData.MarkDirty(oldPosition.PreviousPosition);
                }
            }

            if (v.GetComponent(k, out ContinuousMapPosition p))
            {
                if (context.MapLayerRegistry.TryGetValue(p.LayerId, out var layer) &&
                    context.TryGetContinuousDataFor(layer, out var mapData))
                {
                    mapData.MarkDirty(p);
                }
            }
        }

    }
}