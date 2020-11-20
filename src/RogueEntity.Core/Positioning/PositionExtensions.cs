using RogueEntity.Core.Utils;

namespace RogueEntity.Core.Positioning
{
    public static class PositionExtensions
    {
        public static Position2D ToGridXY<TPos>(this TPos p)
            where TPos: IPosition
        {
            return new Position2D(p.GridX, p.GridY);
        }
    }
}