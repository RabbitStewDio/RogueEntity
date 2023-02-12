﻿using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using RogueEntity.Core.Positioning.Algorithms;

namespace RogueEntity.Core.Utils
{
    /// <summary>
    /// Represents a rectangle in terms of grid squares. Provides numerous static
    /// functions that enable creation and common operations involving rectangles,
    /// as well as implicit conversion operators that enable interoperability with
    /// rectangle representations from other libraries.
    /// </summary>
    /// <remarks>
    /// Rectangle is designed to be extremely efficient and interoperable with
    /// equivalent representations in other libraries, so in general, in an
    /// environment where you have multiple rectangle representations defined, it
    /// is best to prefer Rectangle where possible, as something that accepts or
    /// works with Rectangle will generally work with other supported types as
    /// well.
    /// </remarks>
    [Serializable]
    [DataContract]
    public readonly struct Rectangle : IEquatable<Rectangle>,
                                       IEquatable<(int x, int y, int width, int height)>
    {
        /// <summary>
        /// The empty rectangle. Has origin of (0, 0) with 0 width and height.
        /// </summary>
        [NonSerialized]
        public static readonly Rectangle Empty = new Rectangle(0, 0, 0, 0);

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="x">Minimum x coordinate that is inside the rectangle.</param>
        /// <param name="y">Minimum y coordinate that is inside the rectangle.</param>
        /// <param name="width">Width of the rectangle.</param>
        /// <param name="height">Height of the rectangle.</param>
        public Rectangle(int x, int y, int width, int height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="minExtent">Minimum x and y values that are considered inside the rectangle.</param>
        /// <param name="maxExtent">Maximum x and y values that are considered inside the rectangle.</param>
        public Rectangle(GridPosition2D minExtent, GridPosition2D maxExtent)
        {
            X = minExtent.X;
            Y = minExtent.Y;
            Width = maxExtent.X - X + 1;
            Height = maxExtent.Y - Y + 1;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="center">The center point of the rectangle.</param>
        /// <param name="horizontalRadius">
        /// Number of units to the left and right of the center point that are included within the rectangle.
        /// </param>
        /// <param name="verticalRadius">
        /// Number of units to the top and bottom of the center point that are included within the rectangle.
        /// </param>
        public Rectangle(GridPosition2D center, int horizontalRadius, int verticalRadius)
        {
            X = center.X - horizontalRadius;
            Y = center.Y - verticalRadius;
            Width = 2 * horizontalRadius + 1;
            Height = 2 * verticalRadius + 1;
        }

        /// <summary>
        /// Calculates the area of the rectangle.
        /// </summary>
        public int Area
        {
            get => Width * Height;
        }

        /// <summary>
        /// The center coordinate of the rectangle, rounded up if the exact center is between two
        /// positions. The center of a rectangle with width/height 1 is its <see cref="Position"/>.
        /// </summary>
        public GridPosition2D Center
        {
            get => new GridPosition2D(X + (Width / 2), Y + (Height / 2));
        }

        /// <summary>
        /// The height of the rectangle.
        /// </summary>
        [DataMember]
        public readonly int Height;

        /// <summary>
        /// Whether or not this rectangle is empty (has width and height of 0).
        /// </summary>
        public bool IsEmpty
        {
            get => (Width == 0 && Height == 0);
        }

        /// <summary>
        /// The maximum X and Y coordinates that are included in the rectangle.
        /// </summary>
        public GridPosition2D MaxExtent
        {
            get => new GridPosition2D(MaxExtentX, MaxExtentY);
        }

        /// <summary>
        /// The maximum X-coordinate that is included in the rectangle.
        /// </summary>
        public int MaxExtentX
        {
            get => X + Width - 1;
        }

        /// <summary>
        /// The maximum Y-coordinate that is included in the rectangle.
        /// </summary>
        public int MaxExtentY
        {
            get => Y + Height - 1;
        }

        /// <summary>
        /// Minimum extent of the rectangle (minimum x and y values that are included within it).
        /// Identical to <see cref="Position"/> because we define the rectangle's position by its
        /// minimum extent.
        /// </summary>
        public GridPosition2D MinExtent
        {
            get => new GridPosition2D(X, Y);
        }

        /// <summary>
        /// X-value of the minimum extent of the rectangle (minimum x value that is included within
        /// it). Identical to the <see cref="X"/> value because we define the rectangle's position
        /// by its minimum extent.
        /// </summary>
        public int MinExtentX
        {
            get => X;
        }

        /// <summary>
        /// Y-value of the minimum extent of the rectangle (minimum y value that is included within
        /// it). Identical to the <see cref="Y"/> value because we define the rectangle's position
        /// by its minimum extent.
        /// </summary>
        public int MinExtentY
        {
            get => Y;
        }

        /// <summary>
        /// Calculates the perimeter length of the rectangle.  This is equal to the mathematical
        /// perimeter, NOT the number of grid squares around the perimiter.
        /// </summary>
        public int Perimeter
        {
            get => (2 * Width) + (2 * Height);
        }

        /// <summary>
        /// Coord representing the position (min x- and y-values) of the rectangle.
        /// </summary>
        public GridPosition2D Position
        {
            get => new GridPosition2D(X, Y);
        }

        /// <summary>
        /// Returns a coordinate (Width, Height), which represents the size of the rectangle.
        /// </summary>
        public GridPosition2D Size
        {
            get => new GridPosition2D(Width, Height);
        }

        /// <summary>
        /// The width of the rectangle.
        /// </summary>
        [DataMember]
        public readonly int Width;

        /// <summary>
        /// X-coordinate of position of the rectangle.
        /// </summary>
        [DataMember]
        public readonly int X;

        /// <summary>
        /// Y-coordinate of position of the rectangle.
        /// </summary>
        [DataMember]
        public readonly int Y;

        /// <summary>
        /// Creates a rectangle with the given minimum and maximum extents. Effectively a
        /// constructor, but with extra overloads not possible to provide in constructors alone.
        /// </summary>
        /// <param name="minX">Minimum x coordinate that is inside the rectangle.</param>
        /// <param name="minY">Minimum y coordinate that is inside the rectangle.</param>
        /// <param name="maxX">Maximum x coordinate that is inside the rectangle.</param>
        /// <param name="maxY">Maximum y coordinate that is inside the rectangle.</param>
        /// <returns>A new Rectangle with the given minimum and maximum extents.</returns>
        public static Rectangle WithExtents(int minX, int minY, int maxX, int maxY) =>
            new Rectangle(minX, minY, maxX - minX + 1, maxY - minY + 1);

        /// <summary>
        /// Creates a rectangle with the given minimum and maximum extents. Effectively a
        /// constructor, but with extra overloads not possible to provide in constructors alone.
        /// </summary>
        /// <param name="minExtent">Minimum (x, y) coordinates that are inside the rectangle.</param>
        /// <param name="maxExtent">Maximum (x, y) coordinates that are inside the rectangle.</param>
        /// <returns>A new Rectangle with the given minimum and maximum extents.</returns>
        public static Rectangle WithExtents(GridPosition2D minExtent, GridPosition2D maxExtent) => new Rectangle(minExtent, maxExtent);

        /// <summary>
        /// Creates a rectangle centered on the given position, with the given horizontal and
        /// vertical radius values. Effectively a constructor, but with extra overloads not possible
        /// to provide in constructors alone.
        /// </summary>
        /// <param name="centerX">X-value of the center of the rectangle.</param>
        /// <param name="centerY">Y-value of the center of the rectangle.</param>
        /// <param name="horizontalRadius">
        /// Number of units to the left and right of the center point that are included within the rectangle.
        /// </param>
        /// <param name="verticalRadius">
        /// Number of units to the top and bottom of the center point that are included within the rectangle.
        /// </param>
        /// <returns>A new rectangle with the given center point and radius values.</returns>
        public static Rectangle WithRadius(int centerX, int centerY, int horizontalRadius, int verticalRadius)
            => new Rectangle(centerX - horizontalRadius, centerY - verticalRadius, 2 * horizontalRadius + 1,
                             2 * verticalRadius + 1);

        /// <summary>
        /// Creates a rectangle centered on the given position, with the given horizontal and
        /// vertical radius values. Effectively a constructor, but with extra overloads not possible
        /// to provide in constructors alone.
        /// </summary>
        /// <param name="center">Center of the rectangle.</param>
        /// <param name="horizontalRadius">
        /// Number of units to the left and right of the center point that are included within the rectangle.
        /// </param>
        /// <param name="verticalRadius">
        /// Number of units to the top and bottom of the center point that are included within the rectangle.
        /// </param>
        /// <returns>A new rectangle with the given center point and radius values.</returns>
        public static Rectangle WithRadius(GridPosition2D center, int horizontalRadius, int verticalRadius) =>
            new Rectangle(center, horizontalRadius, verticalRadius);

        /// <summary>
        /// Creates a rectangle with the given position and size. Effectively a constructor, but with
        /// extra overloads not possible to provide in constructors alone.
        /// </summary>
        /// <param name="x">Minimum x coordinate that is inside the rectangle.</param>
        /// <param name="y">Minimum y coordinate that is inside the rectangle.</param>
        /// <param name="width">Width of the rectangle.</param>
        /// <param name="height">Height of the rectangle.</param>
        /// <returns>A new rectangle at the given position with the given width and height.</returns>
        public static Rectangle WithPositionAndSize(int x, int y, int width, int height) =>
            new Rectangle(x, y, width, height);

        /// <summary>
        /// Creates a rectangle with the given position and size. Effectively a constructor, but with
        /// extra overloads not possible to provide in constructors alone.
        /// </summary>
        /// <param name="position">Minimum (x, y) values that are inside the resulting rectangle.</param>
        /// <param name="size">The size of the rectangle, in form (width, height).</param>
        /// <returns>A new rectangle at the given position with the given size.</returns>
        public static Rectangle WithPositionAndSize(GridPosition2D position, Dimension size) =>
            new Rectangle(position.X, position.Y, size.Width, size.Height);

        public static Rectangle WithCenterAndSize(GridPosition2D position, Dimension size) =>
            new Rectangle(position.X - size.Width / 2,
                          position.Y - size.Height / 2,
                          size.Width,
                          size.Height
            );

        /// <summary>
        /// Returns whether or not the rectangles differ in either their positions or extents.
        /// </summary>
        /// <param name="r1"/>
        /// <param name="r2"/>
        /// <returns>true if the rectangles do NOT encompass the same area, false otherwise.</returns>
        public static bool operator !=(Rectangle r1, Rectangle r2)
        {
            return !(r1 == r2);
        }

        /// <summary>
        /// Returns whether or not the rectangles have the same position and extents.
        /// </summary>
        /// <param name="r1"/>
        /// <param name="r2"/>
        /// <returns>
        /// true if the area of the two rectangles encompass the exact same area, false otherwise.
        /// </returns>
        public static bool operator ==(Rectangle r1, Rectangle r2)
        {
            return r1.X == r2.X && r1.Y == r2.Y && r1.Width == r2.Width && r1.Height == r2.Height;
        }


        /// <summary>
        /// Creates and returns a new rectangle that is the same size as the current one, but with
        /// the center moved to the given position.
        /// </summary>
        /// <param name="center">The center-point for the new rectangle.</param>
        /// <returns>
        /// A new rectangle that is the same size as the current one, but with the center moved to
        /// the given location.
        /// </returns>
        public Rectangle WithCenter(GridPosition2D center)
            => new Rectangle(center.X - (Width / 2), center.Y - (Height / 2), Width, Height);

        /// <summary>
        /// Creates and returns a new rectangle that is the same size as the current one, but with
        /// the center moved to the given position.
        /// </summary>
        /// <param name="x">X-value for the center-point of the new rectangle.</param>
        /// <param name="y">Y-value for the center-point of the new rectangle.</param>
        /// <returns>
        /// A new rectangle that is the same size as the current one, but with the center moved to
        /// the given location.
        /// </returns>
        public Rectangle WithCenter(int x, int y) => new Rectangle(x - (Width / 2), y - (Height / 2), Width, Height);

        /// <summary>
        /// Creates and returns a new rectangle whose position is the same as the current one, but
        /// has its height changed by the given delta-change value.
        /// </summary>
        /// <param name="deltaHeight">Delta-change for the height of the new rectangle.</param>
        /// <returns>A new rectangle whose height is modified by the given delta-change value.</returns>
        public Rectangle ChangeHeight(int deltaHeight)
            => new Rectangle(X, Y, Width, Height + deltaHeight);

        /// <summary>
        /// Creates and returns a new rectangle whose position is the same as the current one, but
        /// has its width and height changed by the given delta-change values.
        /// </summary>
        /// <param name="deltaWidth">Delta-change for the width of the new rectangle.</param>
        /// <param name="deltaHeight">Delta-change for the height of the new rectangle.</param>
        /// <returns>
        /// A new rectangle whose width/height are modified by the given delta-change values.
        /// </returns>
        public Rectangle ChangeSize(int deltaWidth, int deltaHeight)
            => new Rectangle(X, Y, Width + deltaWidth, Height + deltaHeight);

        /// <summary>
        /// Creates and returns a new rectangle whose position is the same as the current one, but
        /// has its width and height changed by the given delta-change values.
        /// </summary>
        /// <param name="deltaChange">
        /// Vector (deltaWidth, deltaHeight) specifying the delta-change values for the width/height
        /// of the new Rectangle.
        /// </param>
        /// <returns>
        /// A new rectangle whose width/height are modified by the given delta-change values.
        /// </returns>
        public Rectangle ChangeSize(in GridPosition2D deltaChange)
            => new Rectangle(X, Y, Width + deltaChange.X, Height + deltaChange.Y);

        /// <summary>
        /// Creates and returns a new rectangle whose position is the same as the current one, but
        /// has its width changed by the given delta-change value.
        /// </summary>
        /// <param name="deltaWidth">Delta-change for the width of the new rectangle.</param>
        /// <returns>A new rectangle whose width is modified by the given delta-change value.</returns>
        public Rectangle ChangeWidth(int deltaWidth)
            => new Rectangle(X, Y, Width + deltaWidth, Height);

        /// <summary>
        /// Returns whether or not the specified point is considered within the rectangle.
        /// </summary>
        /// <param name="position">The position to check.</param>
        /// <returns>Whether or not the specified point is considered within the rectangle.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(in GridPosition2D position)
        {
            return (position.X >= X && position.X < (X + Width) && position.Y >= Y && position.Y < (Y + Height));
        }

        /// <summary>
        /// Returns whether or not the specified point is considered within the rectangle.
        /// </summary>
        /// <param name="x">The x-value position to check.</param>
        /// <param name="y">The y-value position to check.</param>
        /// <returns>Whether or not the specified point is considered within the rectangle.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(int x, int y) => (x >= X && x < (X + Width) && y >= Y && y < (Y + Height));

        /// <summary>
        /// Returns whether or not the specified rectangle is considered completely contained within
        /// the current one.
        /// </summary>
        /// <param name="other">The rectangle to check.</param>
        /// <returns>
        /// True if the given rectangle is completely contained within the current one, false otherwise.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(in Rectangle other)
        {
            return (X <= other.X && other.X + other.Width <= X + Width && Y <= other.Y &&
                    other.Y + other.Height <= Y + Height);
        }


        /// <summary>
        /// Compares based upon whether or not the areas contained within the rectangle are identical
        /// in both position and extents.
        /// </summary>
        /// <param name="other"/>
        /// <returns>
        /// true if the area of the two rectangles encompass the exact same area, false otherwise.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Rectangle other)
        {
            return Height == other.Height && Width == other.Width && X == other.X && Y == other.Y;
        }

        /// <summary>
        /// Compares to an arbitrary object.
        /// </summary>
        /// <param name="obj"/>
        /// <returns>
        /// true if the object specified is a rectangle instance and encompasses the same area, false otherwise.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(Object obj)
        {
            return obj is Rectangle other && Equals(other);
        }

        /// <summary>
        /// Returns a new rectangle, expanded to include the additional specified number of tiles on
        /// the left/right and top/bottom.
        /// </summary>
        /// <param name="horizontalChange">
        /// Number of additional tiles to include on the left/right of the rectangle.
        /// </param>
        /// <param name="verticalChange">
        /// Number of additional tiles to include on the top/bottom of the rectangle.
        /// </param>
        /// <returns>A new rectangle, expanded appropriately.</returns>
        public Rectangle Expand(int horizontalChange, int verticalChange)
            => new Rectangle(X - horizontalChange, Y - verticalChange, Width + (2 * horizontalChange),
                             Height + (2 * verticalChange));

        /// <summary>
        /// Returns whether or not the given rectangle intersects the current one.
        /// </summary>
        /// <param name="other">The rectangle to check.</param>
        /// <returns>True if the given rectangle intersects with the current one, false otherwise.</returns>
        public bool Intersects(Rectangle other)
        {
            return (other.X < X + Width && X < other.X + other.Width && other.Y < Y + Height &&
                    Y < other.Y + other.Height);
        }

        /// <summary>
        /// Simple hashing.
        /// </summary>
        /// <returns>Hash code for rectangle.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Height;
                hashCode = (hashCode * 397) ^ Width;
                hashCode = (hashCode * 397) ^ X;
                hashCode = (hashCode * 397) ^ Y;
                return hashCode;
            }
        }

        /// <summary>
        /// Creates and returns a new rectangle that has its <see cref="Position"/> moved to the given position.
        /// </summary>
        /// <param name="position">The position for the new rectangle.</param>
        /// <returns>A new rectangle that has its position changed to the given value.</returns>
        public Rectangle WithPosition(GridPosition2D position)
            => new Rectangle(position.X, position.Y, Width, Height);

        /// <summary>
        /// Creates and returns a new rectangle that has its position moved to the given position.
        /// </summary>
        /// <param name="x">X-value for the position of the new rectangle.</param>
        /// <param name="y">Y-value for the position of the new rectangle.</param>
        /// <returns>A new rectangle with the position changed to the given value.</returns>
        public Rectangle WithPosition(int x, int y) => WithPosition(new GridPosition2D(x, y));

        /// <summary>
        /// Creates and returns a new rectangle that has its position moved in the given direction.
        /// </summary>
        /// <param name="direction">The direction to move the new rectangle in.</param>
        /// <returns>A new rectangle that has its position moved in the given direction.</returns>
        public Rectangle Translate(Direction direction)
        {
            var newPos = Position + direction;
            return new Rectangle(newPos.X, newPos.Y, Width, Height);
        }

        /// <summary>
        /// Creates and returns a new rectangle that has its X value moved to the given x-coordinate.
        /// </summary>
        /// <param name="x">The X value for the new rectangle.</param>
        /// <returns>A new rectangle with X changed to the given value.</returns>
        public Rectangle WithX(int x) => new Rectangle(x, Y, Width, Height);

        /// <summary>
        /// Creates and returns a new rectangle that has its Y value moved to the given y-coordinate.
        /// </summary>
        /// <param name="y">The Y value for the new rectangle.</param>
        /// <returns>A new rectangle with Y changed to the given value.</returns>
        public Rectangle WithY(int y) => new Rectangle(X, y, Width, Height);

        /// <summary>
        /// Creates and returns a new rectangle that has the same position and width as the current
        /// one, but with the height changed to the given value.
        /// </summary>
        /// <param name="height">The height for the new rectangle.</param>
        /// <returns>A new rectangle with its height changed to the given value.</returns>
        public Rectangle WithHeight(int height)
            => new Rectangle(X, Y, Width, height);

        /// <summary>
        /// Creates and returns a new rectangle that has been shrunk/expanded as necessary, such that
        /// the maximum extent is the specified value.
        /// </summary>
        /// <param name="maxExtent">The maximum extent of the new rectangle.</param>
        /// <returns>A new rectangle that has its maximum extent adjusted to the specified value.</returns>
        public Rectangle WithMaxExtent(GridPosition2D maxExtent)
            => new Rectangle(MinExtent, maxExtent);

        /// <summary>
        /// Creates and returns a new rectangle that has been shrunk/expanded as necessary, such that
        /// the maximum extent is the specified value.
        /// </summary>
        /// <param name="x">The x-value for the minimum extent of the new rectangle.</param>
        /// <param name="y">The y-value for the minimum extent of the new rectangle.</param>
        /// <returns>A new rectangle that has its maximum extent adjusted to the specified value.</returns>
        public Rectangle WithMaxExtent(int x, int y)
            => new Rectangle(MinExtent, new GridPosition2D(x, y));

        /// <summary>
        /// Creates and returns a new rectangle that has been shrunk/expanded as necessary, such that
        /// the x-value of maximum extent is changed to the specified value.
        /// </summary>
        /// <param name="x">The x-coordinate for the maximum extent of the new rectangle.</param>
        /// <returns>A new rectangle, with its <see cref="MaxExtentX"/> adjusted to the specified value.</returns>
        public Rectangle WithMaxExtentX(int x)
            => new Rectangle(MinExtent, new GridPosition2D(x, MaxExtentY));

        /// <summary>
        /// Creates and returns a new rectangle that has been shrunk/expanded as necessary, such that
        /// the y-value of maximum extent is changed to the specified value.
        /// </summary>
        /// <param name="y">The y-coordinate for the maximum extent of the new rectangle.</param>
        /// <returns>A new rectangle, with its <see cref="MaxExtentY"/> adjusted to the specified value.</returns>
        public Rectangle WithMaxExtentY(int y)
            => new Rectangle(MinExtent, new GridPosition2D(MaxExtentX, y));

        /// <summary>
        /// Creates and returns a new rectangle that has been shrunk/expanded as necessary, such that
        /// the minimum extent is the specified value.
        /// </summary>
        /// <param name="minExtent">The minimum extent of the new rectangle.</param>
        /// <returns>A new rectangle that has its minimum extent adjusted to the specified value.</returns>
        public Rectangle WithMinExtent(GridPosition2D minExtent)
            => new Rectangle(minExtent, MaxExtent);

        /// <summary>
        /// Creates and returns a new rectangle that has been shrunk/expanded as necessary, such that
        /// the minimum extent is the specified value.
        /// </summary>
        /// <param name="x">The x-value for the minimum extent of the new rectangle.</param>
        /// <param name="y">The y-value for the minimum extent of the new rectangle.</param>
        /// <returns>A new rectangle that has its minimum extent adjusted to the specified value.</returns>
        public Rectangle WithMinExtent(int x, int y)
            => new Rectangle(new GridPosition2D(x, y), MaxExtent);

        /// <summary>
        /// Creates and returns a new rectangle that has been shrunk/expanded as necessary, such that
        /// the x-value of minimum extent is changed to the specified value.
        /// </summary>
        /// <param name="x">The x-coordinate for the minimum extent of the new rectangle.</param>
        /// <returns>A new rectangle, with its <see cref="MinExtentX"/> adjusted to the specified value.</returns>
        public Rectangle WithMinExtentX(int x)
            => new Rectangle(new GridPosition2D(x, MinExtentY), MaxExtent);

        /// <summary>
        /// Creates and returns a new rectangle that has been shrunk/expanded as necessary, such that
        /// the y-value of minimum extent is changed to the specified value.
        /// </summary>
        /// <param name="y">The y-coordinate for the minimum extent of the new rectangle.</param>
        /// <returns>A new rectangle, with its <see cref="MinExtentY"/> adjusted to the specified value.</returns>
        /// &gt;
        public Rectangle WithMinExtentY(int y)
            => new Rectangle(new GridPosition2D(MinExtentX, y), MaxExtent);

        /// <summary>
        /// Creates and returns a new rectangle whose position is the same as the current one, but
        /// has the specified width and height.
        /// </summary>
        /// <param name="width">The width for the new rectangle.</param>
        /// <param name="height">The height for the new rectangle.</param>
        /// <returns>A new rectangle with the given width and height.</returns>
        public Rectangle WithSize(int width, int height)
            => new Rectangle(X, Y, width, height);

        /// <summary>
        /// Creates and returns a new rectangle whose position is the same as the current one, but
        /// has the specified width and height.
        /// </summary>
        /// <param name="size">Vector (width, height) specifying the width/height of the new rectangle.</param>
        /// <returns>A new rectangle with the given width and height.</returns>
        public Rectangle WithSize(GridPosition2D size)
            => new Rectangle(X, Y, size.X, size.Y);

        /// <summary>
        /// Creates and returns a new rectangle that is exactly the same as the current one, but with
        /// the width changed to the given value.
        /// </summary>
        /// <param name="width">The width for the new rectangle.</param>
        /// <returns>A new rectangle with its <see cref="Width"/> changed to the given value.</returns>
        public Rectangle WithWidth(int width) => new Rectangle(X, Y, width, Height);

        public override string ToString()
        {
            return $"({X}, {Y}, {Width}, {Height})";
        }

        /// <summary>
        /// Creates and returns a new rectangle whose position has been moved by the given
        /// delta-change values.
        /// </summary>
        /// <param name="deltaChange">Delta-x and delta-y values by which to move the new rectangle.</param>
        /// <returns>
        /// A new rectangle, whose position has been moved by the given delta-change values.
        /// </returns>
        public Rectangle Translate(GridPosition2D deltaChange)
            => new Rectangle(X + deltaChange.X, Y + deltaChange.Y, Width, Height);

        /// <summary>
        /// Creates and returns a new rectangle whose position has been moved by the given
        /// delta-change values.
        /// </summary>
        /// <param name="dx">Delta-x value by which to move the new rectangle.</param>
        /// <param name="dy">Delta-y value by which to move the new rectangle.</param>
        /// <returns>
        /// A new rectangle, whose position has been moved by the given delta-change values.
        /// </returns>
        public Rectangle Translate(int dx, int dy)
            => new Rectangle(X + dx, Y + dy, Width, Height);

        /// <summary>
        /// Creates and returns a new rectangle whose x-position has been moved by the given delta value.
        /// </summary>
        /// <param name="dx">Value by which to move the new rectangle's x-position.</param>
        /// <returns>A new rectangle, whose x-position has been moved by the given delta-x value.</returns>
        public Rectangle TranslateX(int dx)
            => new Rectangle(X + dx, Y, Width, Height);

        /// <summary>
        /// Creates and returns a new rectangle whose y-position has been moved by the given delta value.
        /// </summary>
        /// <param name="dy">Value by which to move the new rectangle's y-position.</param>
        /// <returns>A new rectangle, whose y-position has been moved by the given delta-y value.</returns>
        public Rectangle TranslateY(int dy)
            => new Rectangle(X, Y + dy, Width, Height);

        /// <summary>
        /// Implicitly converts a GoRogue Rectangle to an equivalent tuple of 4 integers (x, y, width, height).
        /// </summary>
        /// <param name="rect" />
        /// <returns />
        public static implicit operator (int x, int y, int width, int height)(Rectangle rect) =>
            (rect.X, rect.Y, rect.Width, rect.Height);

        /// <summary>
        /// Implicitly converts a tuple of 4 integers (x, y, width, height) to an equivalent GoRogue Rectangle.
        /// </summary>
        /// <param name="tuple" />
        /// <returns />
        public static implicit operator Rectangle((int x, int y, int width, int height) tuple) =>
            new Rectangle(tuple.x, tuple.y, tuple.width, tuple.height);

        /// <summary>
        /// Adds support for C# Deconstruction syntax.
        /// </summary>
        /// <param name="x" />
        /// <param name="y" />
        /// <param name="width" />
        /// <param name="height" />
        public void Deconstruct(out int x, out int y, out int width, out int height)
        {
            x = X;
            y = Y;
            width = Width;
            height = Height;
        }

        /// <summary>
        /// True if the two rectangles represent the same area.
        /// </summary>
        /// <param name="r1"></param>
        /// <param name="r2"></param>
        /// <returns>True if the two rectangles are equal, false if not.</returns>
        public static bool operator ==(Rectangle r1, (int x, int y, int width, int height) r2)
        {
            return r1.X == r2.x && r1.Y == r2.y && r1.Width == r2.width && r1.Height == r2.height;
        }

        /// <summary>
        /// True if any of the rectangles' x/y/width/height values are not equal.
        /// </summary>
        /// <param name="r1"></param>
        /// <param name="r2"></param>
        /// <returns>
        /// True if any of the x/y/width/height values are not equal, false if they are all equal.
        /// </returns>
        public static bool operator !=(Rectangle r1, (int x, int y, int width, int height) r2) => !(r1 == r2);

        /// <summary>
        /// True if the two rectangles represent the same area.
        /// </summary>
        /// <param name="r1"></param>
        /// <param name="r2"></param>
        /// <returns>True if the two rectangles are equal, false if not.</returns>
        public static bool operator ==((int x, int y, int width, int height) r1, Rectangle r2)
        {
            return r1.x == r2.X && r1.y == r2.Y && r1.width == r2.Width && r1.height == r2.Height;
        }

        /// <summary>
        /// True if any of the rectangles' x/y/width/height values are not equal.
        /// </summary>
        /// <param name="r1"></param>
        /// <param name="r2"></param>
        /// <returns>
        /// True if any of the x/y/width/height values are not equal, false if they are all equal.
        /// </returns>
        public static bool operator !=((int x, int y, int width, int height) r1, Rectangle r2) => !(r1 == r2);

        /// <summary>
        /// True if the given position has equal x and y values to the current one.
        /// </summary>
        /// <param name="other">Point to compare.</param>
        /// <returns>True if the two positions are equal, false if not.</returns>
        public bool Equals((int x, int y, int width, int height) other)
            => X == other.x && Y == other.y && Width == other.width && Height == other.height;

        /// <summary>
        /// Gets all positions that reside on the inner perimeter of the rectangle.
        /// </summary>
        /// <returns>IEnumerable of all positions that reside on the inner perimeter of the rectangle.</returns>
        public RectangleBoundary PerimeterPositions()
        {
            return new RectangleBoundary(this);
        }

        /// <summary>
        /// Gets all positions that reside on the min-y line of the rectangle.
        /// </summary>
        /// <returns>IEnumerable of all positions that lie on the min-y line of the rectangle.</returns>
        public RectangleBoundary MinYPositions()
        {
            return new RectangleBoundary(new Rectangle(X, Y, Width, 1));
        }

        /// <summary>
        /// Gets all positions that reside on the max-y line of the rectangle.
        /// </summary>
        /// <returns>IEnumerable of all positions that lie on the max-y line of the rectangle.</returns>
        public RectangleBoundary MaxYPositions()
        {
            return new RectangleBoundary(new Rectangle(X, MaxExtentY, Width, 1));
        }

        /// <summary>
        /// Gets all positions that reside on the min-x line of the rectangle.
        /// </summary>
        /// <returns>IEnumerable of all positions that lie on the min-x line of the rectangle.</returns>
        public RectangleBoundary MinXPositions()
        {
            return new RectangleBoundary(new Rectangle(X, Y, 1, Height));
        }

        /// <summary>
        /// Gets all positions that reside on the max-x line of the rectangle.
        /// </summary>
        /// <returns>IEnumerable of all positions that lie on the max-x line of the rectangle.</returns>
        public RectangleBoundary MaxXPositions()
        {
            return new RectangleBoundary(new Rectangle(MaxExtentX, Y, 1, Height));
        }

        /// <summary>
        /// Gets an IEnumerable of all positions that line on the inner perimeter of the rectangle,
        /// on the given side of the rectangle.
        /// </summary>
        /// <param name="side">Side to get positions for.</param>
        /// <returns>IEnumerable of all positions that line on the inner perimeter of the rectangle on the given side.</returns>
        public RectangleBoundary PositionsOnSide(Direction side)
        {
            switch (side)
            {
                case Direction.Up:
                    return MinYPositions();
                case Direction.Right:
                    return MaxXPositions();
                case Direction.Down:
                    return MaxYPositions();
                case Direction.Left:
                    return MinXPositions();
                default:
                    throw new Exception("Cannot retrieve positions on a non-cardinal side of a rectangle.");
            }
        }

        public RectangleContents Contents => new RectangleContents(this);
    }

    public static class RectangleExtensions
    {
        /// <summary>
        /// Gets a sequence of positions representing every location in <paramref name="rect1"/> that
        /// is NOT in <paramref name="rect2"/>.
        /// </summary>
        /// <param name="rect1"/>
        /// <param name="rect2"/>
        /// <returns>A sequence of positions representing every location in <paramref name="rect1"/> that
        /// is NOT in <paramref name="rect2"/>.</returns>
        public static IEnumerable<GridPosition2D> GetDifference(this Rectangle rect1, Rectangle rect2)
        {
            foreach (var pos in rect1.Contents)
            {
                if (rect2.Contains(pos))
                    continue;

                yield return pos;
            }
        }

        /// <summary>
        /// Gets a sequence of positions representing the exact union of the specified rectangles, eg.
        /// a MapArea containing all locations from either rectangle.
        /// </summary>
        /// <param name="r1"/>
        /// <param name="r2"/>
        /// <returns>A sequence of positions containing every position in either rectangle.</returns>
        public static IEnumerable<GridPosition2D> GetExactUnion(this Rectangle r1, Rectangle r2)
        {
            for (int x = r1.X; x <= r1.MaxExtentX; x++)
            for (int y = r1.Y; y <= r1.MaxExtentY; y++)
                yield return new GridPosition2D(x, y);

            for (int x = r2.X; x <= r2.MaxExtentX; x++)
            for (int y = r2.Y; y <= r2.MaxExtentY; y++)
            {
                if (r1.Contains(x, y)) continue;
                yield return new GridPosition2D(x, y);
            }
        }

        /// <summary>
        /// Returns the rectangle that represents the intersection of the two rectangles specified,
        /// or the empty rectangle if the specified rectangles do not intersect.
        /// </summary>
        /// <param name="r1"/>
        /// <param name="r2"/>
        /// <returns>
        /// Rectangle representing the intersection of <paramref name="r1"/> and <paramref name="r2"/>, or
        /// the empty rectangle if the two rectangles do not intersect.
        /// </returns>
        public static Rectangle GetIntersection(this Rectangle r1, in Rectangle r2)
        {
            if (r1.Intersects(r2))
            {
                int minX = Math.Max(r1.X, r2.X);
                int minY = Math.Max(r1.Y, r2.Y);

                int exclusiveMaxX = Math.Min(r1.X + r1.Width, r2.X + r2.Width);
                int exclusiveMaxY = Math.Min(r1.Y + r1.Height, r2.Y + r2.Height);

                return new Rectangle(minX, minY, exclusiveMaxX - minX, exclusiveMaxY - minY);
            }

            return Rectangle.Empty;
        }

        /// <summary>
        /// Gets the smallest possible rectangle that includes the entire area of both <paramref name="r1"/> and
        /// <paramref name="r2"/>.
        /// </summary>
        /// <param name="r1"/>
        /// <param name="r2"/>
        /// <returns>
        /// The smallest possible rectangle that includes the entire area of both <paramref name="r1"/> and
        /// <paramref name="r2"/>.
        /// </returns>
        public static Rectangle GetUnion(this Rectangle r1, in Rectangle r2)
        {
            int x = Math.Min(r1.X, r2.X);
            int y = Math.Min(r1.Y, r2.Y);

            return new Rectangle(x, y, Math.Max(r1.X + r1.Width, r2.X + r2.Width) - x,
                                 Math.Max(r1.Y + r1.Height, r2.Y + r2.Height) - y);
        }
    }
}
