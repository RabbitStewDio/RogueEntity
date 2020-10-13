namespace RogueEntity.Core.Positioning.Grid
{
    public static class GridMapContextExtensions
    {
        public static bool IsValid<TGameContext, TItemId, TPosition>(this IGridMapDataContext<TGameContext, TItemId> context,
                                                                     TPosition p)
            where TPosition : IPosition
        {
            if (p.IsInvalid) return false;
            if (!context.TryGetMap(p.GridZ, out var data))
            {
                return false;
            }
            
            if (p.GridX < 0 || p.GridX >= data.Width) return false;
            if (p.GridY < 0 || p.GridY >= data.Height) return false;
            return true;
        }
    }
}