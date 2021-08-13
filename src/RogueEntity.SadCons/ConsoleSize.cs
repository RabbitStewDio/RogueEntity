using RogueEntity.Core.Utils;
using SadConsole;
using System;

namespace RogueEntity.SadCons
{
    public readonly struct ConsoleSize : IEquatable<ConsoleSize>
    {
        public readonly Rectangle BoundsInPixels;
        public readonly Dimension CellSize;

        public ConsoleSize(Rectangle boundsInPixels, Dimension cellSize)
        {
            BoundsInPixels = boundsInPixels;
            CellSize = cellSize;
        }

        public Rectangle BoundsInCells() => BoundsFor(CellSize);
        
        public Rectangle BoundsFor(Font font) => BoundsFor(new Dimension(font.Size.X, font.Size.Y));
        
        public Rectangle BoundsFor(Dimension font)
        {
            var fontWidth = font.Width;
            var fontHeight = font.Height;

            var xCeil = (BoundsInPixels.X + fontWidth - 1) / fontWidth;
            var yCeil = (BoundsInPixels.Y + fontHeight - 1) / fontHeight;

            return new Rectangle(xCeil, yCeil, BoundsInPixels.Width / fontWidth, BoundsInPixels.Height / fontHeight);
        } 
        
        public override string ToString()
        {
            return $"{nameof(ConsoleSize)}({nameof(BoundsInPixels)}: {BoundsInPixels}, {nameof(CellSize)}: {CellSize})";
        }

        public bool Equals(ConsoleSize other)
        {
            return BoundsInPixels.Equals(other.BoundsInPixels) && CellSize.Equals(other.CellSize);
        }

        public override bool Equals(object obj)
        {
            return obj is ConsoleSize other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (BoundsInPixels.GetHashCode() * 397) ^ CellSize.GetHashCode();
            }
        }

        public static bool operator ==(ConsoleSize left, ConsoleSize right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ConsoleSize left, ConsoleSize right)
        {
            return !left.Equals(right);
        }
    }
}
