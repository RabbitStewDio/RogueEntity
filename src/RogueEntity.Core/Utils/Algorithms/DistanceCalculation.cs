using System;
using RogueEntity.Core.Positioning;

namespace RogueEntity.Core.Utils.Algorithms
{
    public enum DistanceCalculation
    {
        Euclid = 0,
        Manhattan = 1,
        Chebyshev = 2
    }

    public static class DistanceCalculationExtensions
    {
        static readonly float Sqrt = (float) Math.Sqrt(2);
        
        public static AdjacencyRule AsAdjacencyRule(this DistanceCalculation c)
        {
            switch (c)
            {
                case DistanceCalculation.Euclid:
                    return AdjacencyRule.EightWay;
                case DistanceCalculation.Manhattan:
                    return AdjacencyRule.Cardinals;
                case DistanceCalculation.Chebyshev:
                    return AdjacencyRule.EightWay;
                default:
                    throw new ArgumentOutOfRangeException(nameof(c), c, null);
            }
        }

        public static float MaximumStepDistance(this DistanceCalculation c)
        {
            switch (c)
            {
                case DistanceCalculation.Euclid:
                    return Sqrt;
                case DistanceCalculation.Manhattan:
                    return 1;
                case DistanceCalculation.Chebyshev:
                    return 1;
                default:
                    throw new ArgumentOutOfRangeException(nameof(c), c, null);
            }
        }

        public static double Calculate<TPosition>(this DistanceCalculation c, TPosition p1, TPosition p2)
            where TPosition : IPosition
        {
            var dx = p1.X - p2.X;
            var dy = p1.Y - p2.Y;
            var dz = p1.Z - p2.Z;
            return c.Calculate(dx, dy, dz);
        }

        public static double Calculate<TPosition>(this DistanceCalculation c, TPosition p1)
            where TPosition : IPosition
        {
            var dx = p1.X;
            var dy = p1.Y;
            var dz = p1.Z;
            return c.Calculate(dx, dy, dz);
        }

        public static double Calculate(this DistanceCalculation c, Position2D p1)
        {
            var dx = p1.X;
            var dy = p1.Y;
            return c.Calculate(dx, dy, 0);
        }

        public static double Calculate(this DistanceCalculation c, double dx, double dy, double dz)
        {
            switch (c)
            {
                case DistanceCalculation.Euclid:
                {
                    return Math.Sqrt(dx * dx + dy * dy + dz * dz);
                }
                case DistanceCalculation.Manhattan:
                {
                    dx = Math.Abs(dx);
                    dy = Math.Abs(dy);
                    dz = Math.Abs(dz);
                    return (dx + dy + dz); // Simply manhattan distance
                }
                case DistanceCalculation.Chebyshev:
                {
                    dx = Math.Abs(dx);
                    dy = Math.Abs(dy);
                    dz = Math.Abs(dz);
                    return Math.Max(dx, Math.Max(dy, dz)); // Radius is the longest axial distance
                }
                default:
                    throw new ArgumentOutOfRangeException(nameof(c), c, null);
            }
        }
    }
}