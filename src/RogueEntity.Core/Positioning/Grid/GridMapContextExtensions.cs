namespace RogueEntity.Core.Positioning.Grid
{
    public static class GridMapContextExtensions
    {
        public static bool IsValid<TGameContext, TItemId, TPosition>(this IGridMapDataContext<TGameContext, TItemId> context,
                                                                     TPosition p)
            where TPosition : IPosition
        {
            if (p.IsInvalid) return false;
            if (!context.TryGetMap(p.GridZ, out _))
            {
                return false;
            }
            
            return true;
        }
    }
}