using System;
using GoRogue;

namespace RogueEntity.Core.Movement
{
    [Flags]
    public enum MovementDirection: byte
    {
        North = 1,
        NorthEast = 2,
        East = 4,
        SouthEast = 8,
        South = 16,
        SouthWest = 32,
        West = 64,
        NorthWest = 128,
    }

    public static class MovementDirectionExtensions
    {
        public static MovementDirection ToMovementDirection(this Direction d)
        {
            switch (d)
            {
                case Direction.Up:
                    return MovementDirection.North;
                case Direction.UpRight:
                    return MovementDirection.NorthEast;
                case Direction.Right:
                    return MovementDirection.East;
                case Direction.DownRight:
                    return MovementDirection.SouthEast;
                case Direction.Down:
                    return MovementDirection.South;
                case Direction.DownLeft:
                    return MovementDirection.SouthWest;
                case Direction.Left:
                    return MovementDirection.West;
                case Direction.UpLeft:
                    return MovementDirection.NorthWest;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}