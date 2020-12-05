using RogueEntity.Core.Positioning.Algorithms;
using RogueEntity.Core.Utils;

namespace RogueEntity.Core.Positioning
{
    public static class PositionExtensions
    {
        public static Position2D ToGridXY<TPos>(this TPos p)
            where TPos: IPosition<TPos>
        {
            return new Position2D(p.GridX, p.GridY);
        }
        

        public static double Calculate(this DistanceCalculation c, Position p1, Position p2)
        {
            var dx = p1.X - p2.X;
            var dy = p1.Y - p2.Y;
            var dz = p1.Z - p2.Z;
            return c.Calculate(dx, dy, dz);
        }

        public static double Calculate<TPosition>(this DistanceCalculation c, TPosition p1, TPosition p2)
            where TPosition : IPosition<TPosition>
        {
            var dx = p1.X - p2.X;
            var dy = p1.Y - p2.Y;
            var dz = p1.Z - p2.Z;
            return c.Calculate(dx, dy, dz);
        }

        public static double Calculate2D<TPosition>(this DistanceCalculation c, TPosition p1, TPosition p2)
            where TPosition : IPosition2D<TPosition>
        {
            var dx = p1.X - p2.X;
            var dy = p1.Y - p2.Y;
            return c.Calculate(dx, dy, 0);
        }

        public static double Calculate<TPosition>(this DistanceCalculation c, TPosition p1)
            where TPosition : IPosition<TPosition>
        {
            var dx = p1.X;
            var dy = p1.Y;
            var dz = p1.Z;
            return c.Calculate(dx, dy, dz);
        }

        public static double Calculate2D<TPosition>(this DistanceCalculation c, TPosition p1)
            where TPosition : IPosition2D<TPosition>
        {
            var dx = p1.X;
            var dy = p1.Y;
            return c.Calculate(dx, dy, 0);
        }
    }
}