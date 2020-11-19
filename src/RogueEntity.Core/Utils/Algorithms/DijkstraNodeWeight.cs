using System;

namespace RogueEntity.Core.Utils.Algorithms
{
    /// <summary>
    ///   A weight that adds a small preference for cardinal directions to the
    ///   algorithm result without interfering with the real weight system.
    ///
    ///   This makes the resulting paths a lot more human if the weight system
    ///   uses zero-costs and the adjacency rule also uses equal cost movements
    ///   (Cheby, I'm looking at you here!). 
    /// </summary>
    readonly struct DijkstraNodeWeight : IComparable<DijkstraNodeWeight>, IComparable
    {
        readonly float weight;
        readonly float heuristic;

        public DijkstraNodeWeight(float weight, float heuristic)
        {
            this.weight = weight;
            this.heuristic = heuristic;
        }

        public int CompareTo(DijkstraNodeWeight other)
        {
            var weightComparison = weight.CompareTo(other.weight);
            if (weightComparison != 0)
            {
                return weightComparison;
            }

            return heuristic.CompareTo(other.heuristic);
        }

        public int CompareTo(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return 1;
            }

            return obj is DijkstraNodeWeight other ? CompareTo(other) : throw new ArgumentException($"Object must be of type {nameof(DijkstraNodeWeight)}");
        }

        public static bool operator <(DijkstraNodeWeight left, DijkstraNodeWeight right)
        {
            return left.CompareTo(right) < 0;
        }

        public static bool operator >(DijkstraNodeWeight left, DijkstraNodeWeight right)
        {
            return left.CompareTo(right) > 0;
        }

        public static bool operator <=(DijkstraNodeWeight left, DijkstraNodeWeight right)
        {
            return left.CompareTo(right) <= 0;
        }

        public static bool operator >=(DijkstraNodeWeight left, DijkstraNodeWeight right)
        {
            return left.CompareTo(right) >= 0;
        }
    }
}