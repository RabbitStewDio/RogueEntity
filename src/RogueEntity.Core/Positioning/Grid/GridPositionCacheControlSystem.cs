using EnTTSharp.Entities;
using RogueEntity.Core.Positioning.MapLayers;

namespace RogueEntity.Core.Positioning.Grid
{
    public static class GridPositionCacheControlSystem
    {
        public static void ApplyGridMovementChangeTracker<TItemId, TGameContext>(IEntityViewControl<TItemId> v,
                                                                                 TGameContext context,
                                                                                 TItemId k,
                                                                                 in EntityGridPositionChangedMarker oldPosition)
            where TItemId : IEntityKey
            where TGameContext : IMapLayerContext, IGridMapContext<TGameContext, TItemId>
        {
            if (oldPosition.PreviousPosition != EntityGridPosition.Invalid)
            {
                if (context.MapLayerRegistry.TryGetValue(oldPosition.PreviousPosition.LayerId, out var layer) &&
                    context.TryGetGridDataFor(layer, out var mapData))
                {
                    mapData.MarkDirty(oldPosition.PreviousPosition);
                }
            }

            if (v.GetComponent(k, out EntityGridPosition p))
            {
                if (context.MapLayerRegistry.TryGetValue(p.LayerId, out var layer) &&
                    context.TryGetGridDataFor(layer, out var mapData))
                {
                    mapData.MarkDirty(p);
                }
            }
        }
    }
}