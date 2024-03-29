using System;

namespace RogueEntity.Core.Positioning.Algorithms
{
    /// <summary>
    ///   A marker for defining how movement is performed. This both controls the movement style.
    ///
    ///   * Manhattan: grid movement limited to cardinal directions (like a rook in chess)
    ///   * Chebyshev: grid movement both along cardinal and diagonal directions (like the queen in chess)
    ///   * Euclid: freeform movement unrestrained by a grid. 
    /// </summary>
    public enum DistanceCalculation: byte
    {
        Euclid = 0,
        Manhattan = 1,
        Chebyshev = 2
    }

    public static class DistanceCalculationExtensions
    {
        static readonly float sqrt = (float) Math.Sqrt(2);

        public static bool IsOtherMoreAccurate(this DistanceCalculation current, DistanceCalculation replacement)
        {
            return (int)replacement < (int)current;
        }
        
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
                    return sqrt;
                case DistanceCalculation.Manhattan:
                    return 1;
                case DistanceCalculation.Chebyshev:
                    return 1;
                default:
                    throw new ArgumentOutOfRangeException(nameof(c), c, null);
            }
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