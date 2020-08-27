using EnTTSharp.Entities;
using RogueEntity.Core.Positioning.Continuous;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Sensing.CacheControl;

namespace RogueEntity.Core.Sensing.Maps
{
    /// <summary>
    ///   Marks cache entries as dirty when an item or entity has moved on the map.
    /// </summary>
    public static class SenseMappingSystems
    {
        public static void ApplyGridMovementChangeTracker<TItemId, TGameContext>(IEntityViewControl<TItemId> v,
                                                                                 TGameContext context,
                                                                                 TItemId k,
                                                                                 in EntityGridPositionChangedMarker oldPosition)
            where TItemId : IEntityKey
            where TGameContext : IMapCacheControlProvider
        {
            if (oldPosition.PreviousPosition != EntityGridPosition.Invalid)
            {
                context.MapCacheControl.MarkDirty(oldPosition.PreviousPosition);
            }

            if (v.GetComponent(k, out EntityGridPosition p))
            {
                context.MapCacheControl.MarkDirty(p);
            }
        }

        public static void ApplyContinuousMovementChangeTracker<TItemId, TGameContext>(IEntityViewControl<TItemId> v,
                                                                                       TGameContext context,
                                                                                       TItemId k,
                                                                                       in ContinuousMapPositionChangedMarker c)
            where TItemId : IEntityKey
            where TGameContext : IMapCacheControlProvider
        {
            if (c.PreviousPosition != ContinuousMapPosition.Invalid)
            {
                context.MapCacheControl.MarkDirty(c.PreviousPosition);
            }

            if (v.GetComponent(k, out ContinuousMapPosition p))
            {
                context.MapCacheControl.MarkDirty(p);
            }
        }
    }
}