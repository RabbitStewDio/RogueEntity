using System;
using RogueEntity.Core.Utils;

namespace RogueEntity.Core.Positioning.Algorithms
{
    /// <summary>
    /// Represents the concept of a "direction" on a grid. Y always increases downwards, so Direction.Down
    /// is defined as (0, +1) and Direction.Up is defined as (0, -1).
    /// </summary>
    public enum Direction: byte
    {
        None = 0,
        Up = 1,
        UpRight = 2,
        Right = 3,
        DownRight = 4,
        Down = 5,
        DownLeft = 6,
        Left = 7,
        UpLeft = 8
    }

    public static class Directions
    {
        static readonly Direction[] moveDirections;
        static readonly Direction[] inverseDirections;
        static readonly ShortGridPosition2D[] deltas;

        static Directions()
        {
            deltas = new ShortGridPosition2D[9];
            deltas[(int) Direction.None] = new ShortGridPosition2D(0, 0);
            deltas[(int) Direction.Up] = new ShortGridPosition2D(0, -1);
            deltas[(int) Direction.UpRight] = new ShortGridPosition2D(1, -1);
            deltas[(int) Direction.Right] = new ShortGridPosition2D(1, 0);
            deltas[(int) Direction.DownRight] = new ShortGridPosition2D(1, 1);
            deltas[(int) Direction.Down] = new ShortGridPosition2D(0, 1);
            deltas[(int) Direction.DownLeft] = new ShortGridPosition2D(-1, 1);
            deltas[(int) Direction.Left] = new ShortGridPosition2D(-1, 0);
            deltas[(int) Direction.UpLeft] = new ShortGridPosition2D(-1, -1);

            moveDirections = new []
            {
                Direction.Up, 
                Direction.UpRight, 
                Direction.Right, 
                Direction.DownRight, 
                Direction.Down, 
                Direction.DownLeft, 
                Direction.Left, 
                Direction.UpLeft
            };
            inverseDirections = new []
            {
                Direction.None, 
                Direction.Down, 
                Direction.DownLeft, 
                Direction.Left, 
                Direction.UpLeft, 
                Direction.Up, 
                Direction.UpRight, 
                Direction.Right, 
                Direction.DownRight
            };
        }

        public static Direction Inverse(this Direction d)
        {
            return inverseDirections[(int)d];
        }
        
        public static ShortGridPosition2D ToCoordinates(this Direction d)
        {
            return deltas[(int)d];
        }

        public static bool IsCardinal(this Direction d) => ((int)d % 2) == 1;

        /// <summary>
        /// Returns the cardinal direction that most closely matches the degree heading of the given
        /// line. Rounds clockwise if the heading is exactly on a diagonal direction. Similar to
        /// <see cref="GetDirection{TPosition}"/>, except this function returns only cardinal directions.
        /// </summary>
        /// <param name="start">Starting coordinate of the line.</param>
        /// <param name="end">Ending coordinate of the line.</param>
        /// <returns>
        /// The cardinal direction that most closely matches the heading indicated by the given line.
        /// </returns>
        public static Direction GetCardinalDirection(GridPosition2D start, GridPosition2D end) => GetCardinalDirection(end.X - start.X, end.Y - start.Y);

        /// <summary>
        /// Returns the cardinal direction that most closely matches the degree heading of the given
        /// line. Rounds clockwise if the heading is exactly on a diagonal direction. Similar to
        /// <see cref="GetDirection(int, int, int, int)"/>, except this function returns only cardinal directions.
        /// </summary>
        /// <param name="startX">X-coordinate of the starting position of the line.</param>
        /// <param name="startY">Y-coordinate of the starting position of the line.</param>
        /// <param name="endX">X-coordinate of the ending position of the line.</param>
        /// <param name="endY">Y-coordinate of the ending position of the line.</param>
        /// <returns>
        /// The cardinal direction that most closely matches the heading indicated by the given line.
        /// </returns>
        public static Direction GetCardinalDirection(int startX, int startY, int endX, int endY) => GetCardinalDirection(endX - startX, endY - startY);

        /// <summary>
        /// Returns the cardinal direction that most closely matches the degree heading of a line
        /// with the given delta-change values. Rounds clockwise if exactly on a diagonal. Similar to
        /// <see cref="GetDirection{TPosition}"/>, except this function returns only cardinal directions.
        /// </summary>
        /// <param name="deltaChange">
        /// Vector representing the change in x and change in y across the line (deltaChange.X is the
        /// change in x, deltaChange.Y is the change in y).
        /// </param>
        /// <returns>
        /// The cardinal direction that most closely matches the degree heading of the given line.
        /// </returns>
        public static Direction GetCardinalDirection(GridPosition2D deltaChange) => GetCardinalDirection(deltaChange.X, deltaChange.Y);

        /// <summary>
        /// Returns the cardinal direction that most closely matches the degree heading of a line
        /// with the given dx and dy values. Rounds clockwise if exactly on a diagonal direction.
        /// Similar to <see cref="GetDirection(int, int)"/>, except this function returns only cardinal directions.
        /// </summary>
        /// <param name="dx">The change in x-values across the line.</param>
        /// <param name="dy">The change in x-values across the line.</param>
        /// <returns>
        /// The cardinal direction that most closely matches the degree heading of the given line.
        /// </returns>
        public static Direction GetCardinalDirection(int dx, int dy)
        {
            if (dx == 0 && dy == 0)
                return Direction.None;

            float angle = (float)Math.Atan2(dy, dx);
            float degree = MathHelpers.ToDegree(angle);
            degree += 450; // Rotate angle such that it's all positives, and such that 0 is up.
            degree %= 360; // Normalize angle to 0-360

            if (degree < 45.0)
                return Direction.Up;
            if (degree < 135.0)
                return Direction.Right;
            if (degree < 225.0)
                return Direction.Down;
            if (degree < 315.0)
                return Direction.Left;

            return Direction.Up;
        }

        /// <summary>
        /// Returns the direction that most closely matches the degree heading of the given line.
        /// Rounds clockwise if the heading is exactly between two directions.
        /// </summary>
        /// <param name="start">Starting coordinate of the line.</param>
        /// <param name="end">Ending coordinate of the line.</param>
        /// <returns>
        /// The direction that most closely matches the heading indicated by the given line.
        /// </returns>
        public static Direction GetDirection<TPosition>(in TPosition start, in TPosition end)
            where TPosition : IGridPosition2D<TPosition>
            => GetDirection(end.X - start.X, end.Y - start.Y);

        /// <summary>
        /// Returns the direction that most closely matches the degree heading of the given line.
        /// Rounds clockwise if the heading is exactly between two directions.
        /// </summary>
        /// <param name="startX">X-coordinate of the starting position of the line.</param>
        /// <param name="startY">Y-coordinate of the starting position of the line.</param>
        /// <param name="endX">X-coordinate of the ending position of the line.</param>
        /// <param name="endY">Y-coordinate of the ending position of the line.</param>
        /// <returns>
        /// The direction that most closely matches the heading indicated by the given line.
        /// </returns>
        public static Direction GetDirection(int startX, int startY, int endX, int endY) => GetDirection(endX - startX, endY - startY);

        /// <summary>
        /// Returns the direction that most closely matches the degree heading of a line with the
        /// given delta-change values. Rounds clockwise if the heading is exactly between two directions.
        /// </summary>
        /// <param name="deltaChange">
        /// Vector representing the change in x and change in y across the line (deltaChange.X is the
        /// change in x, deltaChange.Y is the change in y).
        /// </param>
        /// <returns>
        /// The direction that most closely matches the heading indicated by the given input.
        /// </returns>
        public static Direction GetDirection(in GridPosition2D deltaChange) => GetDirection(deltaChange.X, deltaChange.Y);

        /// <summary>
        /// Returns the direction that most closely matches the degree heading of a line with the
        /// given dx and dy values. Rounds clockwise if the heading is exactly between two directions.
        /// </summary>
        /// <param name="dx">The change in x-values across the line.</param>
        /// <param name="dy">The change in y-values across the line.</param>
        /// <returns>
        /// The direction that most closely matches the heading indicated by the given input.
        /// </returns>
        public static Direction GetDirection(int dx, int dy)
        {
            if (dx == 0 && dy == 0)
                return Direction.None;

            float angle = (float)Math.Atan2(dy, dx);
            float degree = MathHelpers.ToDegree(angle);
            degree += 450; // Rotate angle such that it's all positives, and such that 0 is up.
            degree %= 360; // Normalize angle to 0-360

            if (degree < 22.5)
                return Direction.Up;
            if (degree < 67.5)
                return Direction.UpRight;
            if (degree < 112.5)
                return Direction.Right;
            if (degree < 157.5)
                return Direction.DownRight;
            if (degree < 202.5)
                return Direction.Down;
            if (degree < 247.5)
                return Direction.DownLeft;
            if (degree < 292.5)
                return Direction.Left;
            if (degree < 337.5)
                return Direction.UpLeft;

            return Direction.Up;
        }

        /// <summary>
        /// Moves the direction counter-clockwise <paramref name="i"/> times.
        /// </summary>
        /// <param name="d"/>
        /// <param name="i"/>
        /// <returns>
        /// The given direction moved counter-clockwise <paramref name="i"/> times.
        /// </returns>
        public static Direction MoveCounterClockwise (this Direction d, int i = 1) => (d == Direction.None) ? Direction.None : moveDirections[MathHelpers.WrapAround((int)d - i - 1, 8)];

        /// <summary>
        /// Moves the direction clockwise <paramref name="i"/> times.
        /// </summary>
        /// <param name="d"/>
        /// <param name="i"/>
        /// <returns>
        /// The given direction moved clockwise <paramref name="i"/> times.
        /// </returns>
        public static Direction MoveClockwise (this Direction d, int i = 1) => (d == Direction.None) ? Direction.None : moveDirections[MathHelpers.WrapAround((int)d + i - 1, 8)];
    }
}