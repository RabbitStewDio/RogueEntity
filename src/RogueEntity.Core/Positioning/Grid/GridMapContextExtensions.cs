using System;

namespace RogueEntity.Core.Positioning.Grid
{
    public static class GridMapContextExtensions
    {
        [Obsolete]
        public static bool IsValid<TItemId, TPosition>(this IGridMapDataContext<TItemId> context, TPosition p)
            where TPosition : IPosition<TPosition>
        {
            if (p.IsInvalid) return false;
            if (!context.TryGetView(p.GridZ, out _))
            {
                return false;
            }

            return true;
        }
    }
}