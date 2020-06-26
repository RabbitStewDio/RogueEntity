using System;

namespace RogueEntity.Core.Infrastructure.Meta
{
    /// <summary>
    ///  Actors and items have weight. For container types (like actors or chests etc)
    ///  the weight is the weight of the base item and the sum of its contents.
    /// </summary>
    public struct Weight : IComparable<Weight>, IComparable, IEquatable<Weight>
    {
        const long UnlimitedWeight = 1L << 40;

        public static readonly Weight Empty = new Weight();
        // Something very large (1,000,000,000 kg) while still giving 
        // enough head space to not run into overflows.
        public static readonly Weight Unlimited = new Weight(UnlimitedWeight);

        // weight is expressed in grams.
        //
        // This results in a maximum weight of 9,007,199,254,740 kg for any particular item.
        // I hope that is a large enough margin of error here.
        public readonly long WeightInGrams;

        public Weight(long weightInGrams)
        {
            this.WeightInGrams = Math.Max(0, Math.Min(weightInGrams, UnlimitedWeight));
        }

        public float AsKilogram => WeightInGrams / 1000f;

        public static Weight OfKiloGram(float kg)
        {
            var ceiling = (long) Math.Ceiling(kg * 1000L);
            return new Weight(ceiling);
        }

        public static Weight OfGram(float g)
        {
            return new Weight((long) Math.Ceiling(g));
        }

        public int CompareTo(Weight other)
        {
            return WeightInGrams.CompareTo(other.WeightInGrams);
        }

        public int CompareTo(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return 1;
            }

            return obj is Weight other ? CompareTo(other) : throw new ArgumentException($"Object must be of type {nameof(Weight)}");
        }

        public static bool operator <(Weight left, Weight right)
        {
            return left.CompareTo(right) < 0;
        }

        public static bool operator >(Weight left, Weight right)
        {
            return left.CompareTo(right) > 0;
        }

        public static bool operator <=(Weight left, Weight right)
        {
            return left.CompareTo(right) <= 0;
        }

        public static bool operator >=(Weight left, Weight right)
        {
            return left.CompareTo(right) >= 0;
        }

        public static Weight operator +(Weight left, Weight right)
        {
            return new Weight(left.WeightInGrams + right.WeightInGrams);
        }

        public static Weight operator -(Weight left, Weight right)
        {
            return new Weight(left.WeightInGrams - right.WeightInGrams);
        }

        public static Weight operator *(Weight left, int right)
        {
            return new Weight(left.WeightInGrams * right);
        }

        public static int operator /(Weight left, Weight right)
        {
            return (int) (left.WeightInGrams / right.WeightInGrams);
        }

        public bool Equals(Weight other)
        {
            return WeightInGrams == other.WeightInGrams;
        }

        public override bool Equals(object obj)
        {
            return obj is Weight other && Equals(other);
        }

        public override int GetHashCode()
        {
            return WeightInGrams.GetHashCode();
        }

        public static bool operator ==(Weight left, Weight right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Weight left, Weight right)
        {
            return !left.Equals(right);
        }

        public override string ToString()
        {
            return $"Weight({WeightInGrams / 1000f}kg)";
        }
    }
}