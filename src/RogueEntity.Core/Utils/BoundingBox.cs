using System;

namespace RogueEntity.Core.Utils
{
    public readonly struct BoundingBox : IEquatable<BoundingBox>
    {
        public readonly int Top;
        public readonly int Left;
        public readonly int Bottom;
        public readonly int Right;

        public int MinExtentX => Left;
        public int MaxExtentX => Right;
        public int MinExtentY => Top;
        public int MaxExtentY => Bottom;
        
        BoundingBox(int left, int top, int right, int bottom)
        {
            Top = top;
            Left = left;
            Bottom = bottom;
            Right = right;
        }

        public static implicit operator Rectangle(BoundingBox b)
        {
            return new Rectangle(new GridPosition2D(b.Top, b.Left), new GridPosition2D(b.Bottom, b.Right));
        }

        public static implicit operator BoundingBox(Rectangle b)
        {
            return new BoundingBox(b.MinExtentY, b.MinExtentX, b.MaxExtentY, b.MaxExtentX);
        }

        public bool Equals(BoundingBox other)
        {
            return Top == other.Top && Left == other.Left && Bottom == other.Bottom && Right == other.Right;
        }

        public override bool Equals(object obj)
        {
            return obj is BoundingBox other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Top;
                hashCode = (hashCode * 397) ^ Left;
                hashCode = (hashCode * 397) ^ Bottom;
                hashCode = (hashCode * 397) ^ Right;
                return hashCode;
            }
        }

        public static bool operator ==(BoundingBox left, BoundingBox right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(BoundingBox left, BoundingBox right)
        {
            return !left.Equals(right);
        }
        
        /// <summary>
        /// Returns whether or not the given rectangle intersects the current one.
        /// </summary>
        /// <param name="other">The rectangle to check.</param>
        /// <returns>True if the given rectangle intersects with the current one, false otherwise.</returns>
        public bool Intersects(BoundingBox other)
        {
            return (Right >= other.Left && Left <= other.Right && Bottom >= other.Top && Top <= other.Bottom);
        }

        public override string ToString()
        {
            return $"BoundingBox({nameof(Top)}: {Top}, {nameof(Left)}: {Left}, {nameof(Bottom)}: {Bottom}, {nameof(Right)}: {Right})";
        }

        public static BoundingBox From(Rectangle b)
        {
            return new BoundingBox(b.MinExtentX, b.MinExtentY, b.MaxExtentX, b.MaxExtentY);
        }

        public static BoundingBox From(GridPosition2D pos)
        {
            return new BoundingBox(pos.X, pos.Y, pos.X, pos.Y);
        }

        public static BoundingBox From(int x, int y)
        {
            return new BoundingBox(x, y, x, y);
        }

        public static BoundingBox From(int x, int y, int width, int height)
        {
            
            return new BoundingBox(x, y, x + Math.Max(1, width) - 1, y + Math.Max(1, height) - 1);
        }

        public static BoundingBox From(GridPosition2D topLeft, GridPosition2D bottomRight)
        {
            return new BoundingBox(topLeft.X, topLeft.Y, bottomRight.X, bottomRight.Y);
        }
    }
}
