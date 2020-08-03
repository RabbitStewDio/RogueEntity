using System;

namespace ValionRL.Core.Generator
{
    public readonly struct Coord
    {
        public readonly int X;
        public readonly int Y;

        public Coord(int x, int y)
        {
            X = x;
            Y = y;
        }

        public static Coord operator +(Coord c, Direction d)
        {
            switch (d)
            {
                case Direction.North:
                    return new Coord(c.X, c.Y - 1);
                case Direction.East:
                    return new Coord(c.X + 1, c.Y);
                case Direction.South:
                    return new Coord(c.X, c.Y + 1);
                case Direction.West:
                    return new Coord(c.X - 1, c.Y);
                default:
                    throw new ArgumentOutOfRangeException(nameof(d), d, null);
            }
        }
    }
}