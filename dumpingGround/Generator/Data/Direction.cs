using System;

namespace ValionRL.Core.Generator
{
    public enum Direction
    {
        North = 0, East = 1, South = 2, West = 3
    }

    public static class DirectionExtensions
    {
        public static bool IsHorizontal(this Direction d)
        {
            return d == Direction.West || d == Direction.East;
        }

        public static Direction Invert(this Direction direction)
        {
            if (direction == Direction.North)
                return Direction.South;
            if (direction == Direction.South)
                return Direction.North;
            if (direction == Direction.East)
                return Direction.West;
            if (direction == Direction.West)
                return Direction.East;
            throw new ArgumentException();
        }
    }
}