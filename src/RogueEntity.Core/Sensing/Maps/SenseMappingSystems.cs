using EnttSharp.Entities;
using RogueEntity.Core.Infrastructure;
using RogueEntity.Core.Infrastructure.Positioning.Continuous;
using RogueEntity.Core.Infrastructure.Positioning.Grid;

namespace RogueEntity.Core.Sensing.Maps
{
    public static class SenseMappingSystems
    {
        public static void ApplyMovementChangeTracker<TGameContext, TItemId>(IEntityViewControl<TItemId> v,
                                                                    TGameContext context,
                                                                    TItemId k,
                                                                    in EntityGridPositionChangedMarker oldPosition)
            where TItemId : IEntityKey
            where TGameContext: IMapCacheControlProvider
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

        public static void ApplyMovementChangeTracker<TGameContext, TItemId>(IEntityViewControl<TItemId> v,
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