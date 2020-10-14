using System;

namespace RogueEntity.DndKit.DiceNotation
{
    public static class GeneratorExtensions
    {
        public static int Next(this Func<double> g, int lowerBound, int upperBoundExclusive)
        {
            if (lowerBound >= upperBoundExclusive)
            {
                return lowerBound;
            }

            var delta = upperBoundExclusive - lowerBound;
            var floor = (int)(g() * delta);
            if (floor == delta)
            {
                // cheap hack. When using float the system happily 
                // rounds up.
                floor = delta - 1;
            }
            return floor + lowerBound;
        }

        public static Func<double> MinRandom { get; } = () => 0;
        const double MaxValue = ((int.MaxValue - 1) * (1.0 / int.MaxValue));
        

        /// <summary>
        ///  Close to zero but not zero.
        /// </summary>
        public static Func<double> MaxRandom { get; } = () => MaxValue;

        public static Func<double> FromSeed(int seed) => new Random(seed).NextDouble;
        public static Func<double> Default() => new Random().NextDouble;
    }
}