using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.Algorithms;

namespace RogueEntity.Core.Positioning
{
    public static class PositionExtensions
    {
        public static Position2D ToPosition2D(this Direction d)
        {
            var c = d.ToCoordinates();
            return new Position2D(c.X, c.Y);
        }

        public static Position2D ToGridXY<TPos>(this TPos p)
            where TPos: IPosition
        {
            return new Position2D(p.GridX, p.GridY);
        }
    }
}