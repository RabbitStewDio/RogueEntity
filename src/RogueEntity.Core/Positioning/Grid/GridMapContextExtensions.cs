namespace RogueEntity.Core.Positioning.Grid
{
    public static class GridMapContextExtensions
    {
        public static bool IsValid<TGameContext, TItemId, TPosition>(this IGridMapDataContext<TGameContext, TItemId> context,
                                                                     TPosition p)
            where TPosition : IPosition
        {
            if (p.IsInvalid) return false;
            if (p.GridX < 0 || p.GridX >= context.Width) return false;
            if (p.GridY < 0 || p.GridY >= context.Height) return false;
            return true;
        }
    }
}