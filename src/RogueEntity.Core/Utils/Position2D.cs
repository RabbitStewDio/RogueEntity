using System;
using System.Runtime.Serialization;
using MessagePack;
using RogueEntity.Core.Utils.Algorithms;

namespace RogueEntity.Core.Utils
{
    public interface IPosition2D<TPosition> 
        where TPosition: IPosition2D<TPosition>
    {
        public int X { get; }
        public int Y { get; }
        
        public void Deconstruct(out int x, out int y);
        public TPosition With(int x, int y);
        public TPosition Add(Position2D d);
    }
    
    [DataContract]
    [MessagePackObject]
    public readonly struct Position2D : IEquatable<Position2D>, IComparable<Position2D>, IComparable, IPosition2D<Position2D>
    {
        [DataMember(Order = 0)]
        [Key(0)]
        public readonly int X;
        [DataMember(Order = 1)]
        [Key(1)]
        public readonly int Y;

        [SerializationConstructor]
        public Position2D(int x, int y)
        {
            X = x;
            Y = y;
        }

        [IgnoreMember]
        [IgnoreDataMember]
        int IPosition2D<Position2D>.X => X;

        [IgnoreMember]
        [IgnoreDataMember]
        int IPosition2D<Position2D>.Y => Y;

        public Position2D With(int x, int y)
        {
            return From(x, y);
        }

        public Position2D Add(Position2D d)
        {
            return this + d;
        }

        public bool Equals(Position2D other)
        {
            return X == other.X && Y == other.Y;
        }

        public override bool Equals(object obj)
        {
            return obj is Position2D other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (X * 397) ^ Y;
            }
        }

        public static bool operator ==(Position2D left, Position2D right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Returns the coordinate (c1.X - c2.X, c1.Y - c2.Y)
        /// </summary>
        /// <param name="c1"></param>
        /// <param name="c2"></param>
        /// <returns>The coordinate(<paramref name="c1"/> - <paramref name="c2"/>).</returns>
        public static Position2D operator -(Position2D c1, Position2D c2) => new Position2D(c1.X - c2.X, c1.Y - c2.Y);

        /// <summary>
        /// Subtracts scalar <paramref name="i"/> from the x and y values of <paramref name="c"/>.
        /// </summary>
        /// <param name="c"></param>
        /// <param name="i"></param>
        /// <returns>The coordinate (c.X - <paramref name="i"/>, c.Y - <paramref name="i"/>)</returns>
        public static Position2D operator -(Position2D c, int i) => new Position2D(c.X - i, c.Y - i);

        /// <summary>
        /// True if either the x-values or y-values are not equal.
        /// </summary>
        /// <param name="c1"></param>
        /// <param name="c2"></param>
        /// <returns>
        /// True if either the x-values or y-values are not equal, false if they are both equal.
        /// </returns>
        public static bool operator !=(Position2D c1, Position2D c2) => !(c1 == c2);

        /// <summary>
        /// Multiplies the x and y of <paramref name="c"/> by <paramref name="i"/>.
        /// </summary>
        /// <param name="c"></param>
        /// <param name="i"></param>
        /// <returns>Coordinate (c.X * <paramref name="i"/>, c.Y * <paramref name="i"/>)</returns>
        public static Position2D operator *(Position2D c, int i) => new Position2D(c.X * i, c.Y * i);

        /// <summary>
        /// Multiplies the x and y value of <paramref name="c"/> by <paramref name="i"/>, rounding
        /// the result to the nearest integer.
        /// </summary>
        /// <param name="c"></param>
        /// <param name="i"></param>
        /// <returns>
        /// Coordinate (c.X * <paramref name="i"/>, c.Y * <paramref name="i"/>), with the resulting values
        /// rounded to nearest integer.
        /// </returns>
        public static Position2D operator *(Position2D c, double i) =>
            new Position2D((int)Math.Round(c.X * i, MidpointRounding.AwayFromZero), (int)Math.Round(c.Y * i, MidpointRounding.AwayFromZero));

        /// <summary>
        /// Divides the x and y of <paramref name="c"/> by <paramref name="i"/>, rounding resulting values
        /// to the nearest integer.
        /// </summary>
        /// <param name="c"></param>
        /// <param name="i"></param>
        /// <returns>(c.X / <paramref name="i"/>, c.Y / <paramref name="i"/>), with the resulting values rounded to the nearest integer.</returns>
        public static Position2D operator /(Position2D c, int i) =>
            new Position2D((int)Math.Round(c.X / (double)i, MidpointRounding.AwayFromZero), (int)Math.Round(c.Y / (double)i, MidpointRounding.AwayFromZero));

        /// <summary>
        /// Divides the x and y of <paramref name="c"/> by <paramref name="i"/>, rounding resulting values
        /// to the nearest integer.
        /// </summary>
        /// <param name="c"></param>
        /// <param name="i"></param>
        /// <returns>(c.X / <paramref name="i"/>, c.Y / <paramref name="i"/>), with the resulting values rounded to the nearest integer.</returns>
        public static Position2D operator /(Position2D c, double i) =>
            new Position2D((int)Math.Round(c.X / i, MidpointRounding.AwayFromZero), (int)Math.Round(c.Y / i, MidpointRounding.AwayFromZero));

        /// <summary>
        /// Returns the coordinate (c1.X + c2.X, c1.Y + c2.Y).
        /// </summary>
        /// <param name="c1"></param>
        /// <param name="c2"></param>
        /// <returns>The coordinate (c1.X + c2.X, c1.Y + c2.Y)</returns>
        public static Position2D operator +(Position2D c1, Position2D c2) => new Position2D(c1.X + c2.X, c1.Y + c2.Y);

        /// <summary>
        /// Adds scalar i to the x and y values of <paramref name="c"/>.
        /// </summary>
        /// <param name="c"></param>
        /// <param name="i"></param>
        /// <returns>Coordinate (c.X + <paramref name="i"/>, c.Y + <paramref name="i"/>.</returns>
        public static Position2D operator +(Position2D c, int i) => new Position2D(c.X + i, c.Y + i);

        /// <summary>
        /// Translates the given coordinate by one unit in the given direction.
        /// </summary>
        /// <param name="c"></param>
        /// <param name="d"></param>
        /// <returns>
        /// Coordinate (c.X + d.DeltaX, c.Y + d.DeltaY)
        /// </returns>
        public static Position2D operator +(Position2D c, Direction d)
        {
            var dc = d.ToCoordinates();
            return new Position2D(c.X + dc.X, c.Y + dc.Y);
        }

        public override string ToString()
        {
            return $"({X}, {Y})";
        }

        public void Deconstruct(out int x, out int y)
        {
            x = X;
            y = Y;
        }

        public static implicit operator Position2D((int x, int y) p)
        {
            return new Position2D(p.x, p.y);
        }

        public static implicit operator Position2D(ShortPosition2D p)
        {
            return new Position2D(p.X, p.Y);
        }

        public int ToLinearIndex(int lineWidth) => Y * lineWidth + X;
        
        public static Position2D From(int linearIndex, int lineWidth)
        {
            var y = Math.DivRem(linearIndex, lineWidth, out var x);
            return new Position2D(x, y);
        }

        public int CompareTo(Position2D other)
        {
            var xComparison = X.CompareTo(other.X);
            if (xComparison != 0)
            {
                return xComparison;
            }

            return Y.CompareTo(other.Y);
        }

        public int CompareTo(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return 1;
            }

            return obj is Position2D other ? CompareTo(other) : throw new ArgumentException($"Object must be of type {nameof(Position2D)}");
        }

        public static bool operator <(Position2D left, Position2D right)
        {
            return left.CompareTo(right) < 0;
        }

        public static bool operator >(Position2D left, Position2D right)
        {
            return left.CompareTo(right) > 0;
        }

        public static bool operator <=(Position2D left, Position2D right)
        {
            return left.CompareTo(right) <= 0;
        }

        public static bool operator >=(Position2D left, Position2D right)
        {
            return left.CompareTo(right) >= 0;
        }
    }
}