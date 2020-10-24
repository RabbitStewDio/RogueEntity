using System;
using System.Collections.Generic;
using RogueEntity.Core.Positioning;

namespace RogueEntity.Core.Utils.Algorithms
{
    public static class AdjacencyRuleExtensions
    {
        static readonly ReadOnlyListWrapper<Direction> CardinalNeighbours;
        static readonly ReadOnlyListWrapper<Direction> DiagonalNeighbours;
        static readonly ReadOnlyListWrapper<Direction> EightWayNeighbours;

        static AdjacencyRuleExtensions()
        {
            CardinalNeighbours = new List<Direction>()
            {
                Direction.Up, Direction.Left, Direction.Down, Direction.Right
            };
            EightWayNeighbours = new List<Direction>()
            {
                Direction.Up, Direction.UpLeft, Direction.Left, Direction.DownLeft, Direction.Down, Direction.DownRight, Direction.Right, Direction.UpRight
            };
            DiagonalNeighbours = new List<Direction>()
            {
                Direction.UpLeft, Direction.UpRight, Direction.DownRight, Direction.DownLeft
            };
        }

        /// <summary>
        /// Gets directions leading to neighboring locations, according to the current adjacency
        /// method. Appropriate directions are returned in clockwise order from the given starting
        /// direction.
        /// </summary>
        /// <param name="startingDirection">The direction to start with.  null or <see cref="Direction.None"/>
        /// causes the default starting direction to be used, which is UP for CARDINALS/EIGHT_WAY, and UP_RIGHT
        /// for diagonals.</param>
        /// <returns>Directions that lead to neighboring locations.</returns>
        public static IEnumerable<Direction> DirectionsOfNeighborsClockwise(this AdjacencyRule type, Direction startingDirection = default)
        {
            switch (type)
            {
                case AdjacencyRule.Cardinals:
                {
                    if (startingDirection == Direction.None)
                    {
                        startingDirection = Direction.Up;
                    }
                    else if (!startingDirection.IsCardinal())
                    {
                        startingDirection.MoveClockwise(); // Make it a cardinal
                    }

                    yield return startingDirection;
                    yield return startingDirection.MoveClockwise(2);
                    yield return startingDirection.MoveClockwise(4);
                    yield return startingDirection.MoveClockwise(6);
                    break;
                }
                case AdjacencyRule.Diagonals:
                {
                    if (startingDirection == Direction.None)
                    {
                        startingDirection = Direction.UpRight;
                    }
                    else if (startingDirection.IsCardinal())
                    {
                        startingDirection = startingDirection.MoveClockwise(); // Make it a diagonal
                    }

                    yield return startingDirection;
                    yield return startingDirection.MoveClockwise(2);
                    yield return startingDirection.MoveClockwise(4);
                    yield return startingDirection.MoveClockwise(6);
                    break;
                }
                case AdjacencyRule.EightWay:
                {
                    if (startingDirection == Direction.None)
                    {
                        startingDirection = Direction.Up;
                    }

                    yield return startingDirection;
                    yield return startingDirection.MoveClockwise();
                    yield return startingDirection.MoveClockwise(2);
                    yield return startingDirection.MoveClockwise(3);
                    yield return startingDirection.MoveClockwise(4);
                    yield return startingDirection.MoveClockwise(5);
                    yield return startingDirection.MoveClockwise(6);
                    yield return startingDirection.MoveClockwise(7);
                    break;
                }
                default:
                    throw new ArgumentException();
            }
        }

        /// <summary>
        /// Gets directions leading to neighboring locations, according to the current adjacency
        /// method. Appropriate directions are returned in counter-clockwise order from the given
        /// starting direction.
        /// </summary>
        /// <param name="startingDirection">The direction to start with.  null or <see cref="Direction.None"/>
        /// causes the default starting direction to be used, which is UP for CARDINALS/EIGHT_WAY, and UP_LEFT
        /// for diagonals.</param>
        /// <returns>Directions that lead to neighboring locations.</returns>
        public static IEnumerable<Direction> DirectionsOfNeighborsCounterClockwise(this AdjacencyRule type, Direction startingDirection = default)
        {
            switch (type)
            {
                case AdjacencyRule.Cardinals:
                {
                    if (startingDirection == Direction.None)
                    {
                        startingDirection = Direction.Up;
                    }
                    else if (!startingDirection.IsCardinal())
                    {
                        startingDirection = startingDirection.MoveClockwise(); // Make it a cardinal
                    }

                    yield return startingDirection;
                    yield return startingDirection.MoveCounterClockwise(2);
                    yield return startingDirection.MoveCounterClockwise(4);
                    yield return startingDirection.MoveCounterClockwise(6);
                    break;
                }

                case AdjacencyRule.Diagonals:
                {
                    if (startingDirection == Direction.None)
                    {
                        startingDirection = Direction.UpLeft;
                    }
                    else if (startingDirection.IsCardinal())
                    {
                        startingDirection = startingDirection.MoveClockwise(); // Make it a diagonal
                    }

                    yield return startingDirection;
                    yield return startingDirection.MoveCounterClockwise(2);
                    yield return startingDirection.MoveCounterClockwise(4);
                    yield return startingDirection.MoveCounterClockwise(6);
                    break;
                }

                case AdjacencyRule.EightWay:
                {
                    if (startingDirection == Direction.None)
                    {
                        startingDirection = Direction.Up;
                    }

                    yield return startingDirection;
                    yield return startingDirection.MoveCounterClockwise();
                    yield return startingDirection.MoveCounterClockwise(2);
                    yield return startingDirection.MoveCounterClockwise(3);
                    yield return startingDirection.MoveCounterClockwise(4);
                    yield return startingDirection.MoveCounterClockwise(5);
                    yield return startingDirection.MoveCounterClockwise(6);
                    yield return startingDirection.MoveCounterClockwise(7);
                    break;
                }
                default:
                    throw new ArgumentException();
            }
        }


        /// <summary>
        /// Gets all neighbors of the specified location, based on the current adjacency method.
        /// Cardinals are returned before any diagonals.
        /// </summary>
        /// <param name="startingLocation">Location to return neighbors for.</param>
        /// <returns>All neighbors of the given location.</returns>
        public static IEnumerable<Position2D> Neighbors(this AdjacencyRule r, Position2D startingLocation)
        {
            foreach (var dir in r.DirectionsOfNeighbors())
                yield return startingLocation + dir;
        }

        /// <summary>
        /// Gets all neighbors of the specified location, based on the current adjacency method.
        /// Cardinals are returned before any diagonals.
        /// </summary>
        /// <param name="startingX">X-Position2D of location to return neighbors for.</param>
        /// <param name="startingY">Y-Position2D of location to return neighbors for.</param>
        /// <returns>All neighbors of the given location.</returns>
        public static IEnumerable<Position2D> Neighbors(this AdjacencyRule r, int startingX, int startingY) => r.Neighbors(new Position2D(startingX, startingY));

        /// <summary>
        /// Gets all neighbors of the specified location, based on the current adjacency method.
        /// Neighbors are returned in clockwise order, starting with the neighbor in the given
        /// starting direction.
        /// </summary>
        /// <param name="startingLocation">Location to return neighbors for.</param>
        /// <param name="startingDirection">
        /// The neighbor in this direction will be returned first, proceeding clockwise.
        /// If null or <see cref="Direction.None"/> is specified, the default starting direction
        /// is used, which is UP for CARDINALS/EIGHT_WAY, and UP_RIGHT for DIAGONALS.
        /// </param>
        /// <returns>All neighbors of the given location.</returns>
        public static IEnumerable<Position2D> NeighborsClockwise(this AdjacencyRule r, Position2D startingLocation, Direction startingDirection = default)
        {
            foreach (var dir in r.DirectionsOfNeighborsClockwise(startingDirection))
                yield return startingLocation + dir;
        }

        /// <summary>
        /// Gets all neighbors of the specified location, based on the current adjacency method.
        /// Neighbors are returned in clockwise order, starting with the neighbor in the given
        /// starting direction.
        /// </summary>
        /// <param name="startingX">X-Position2D of location to return neighbors for.</param>
        /// <param name="startingY">Y-Position2D of location to return neighbors for.</param>
        /// <param name="startingDirection">
        /// The neighbor in this direction will be returned first, proceeding clockwise.
        /// If null or <see cref="Direction.None"/> is specified, the default starting direction
        /// is used, which is UP for CARDINALS/EIGHT_WAY, and UP_RIGHT for DIAGONALS.
        /// </param>
        /// <returns>All neighbors of the given location.</returns>
        public static IEnumerable<Position2D> NeighborsClockwise(this AdjacencyRule r, int startingX, int startingY, Direction startingDirection = default)
            => r.NeighborsClockwise(new Position2D(startingX, startingY), startingDirection);

        /// <summary>
        /// Gets all neighbors of the specified location, based on the current adjacency method.
        /// Neighbors are returned in counter-clockwise order, starting with the neighbor in the given
        /// starting direction.
        /// </summary>
        /// <param name="startingLocation">Location to return neighbors for.</param>
        /// <param name="startingDirection">
        /// The neighbor in this direction will be returned first, proceeding counter-clockwise.
        /// If null or <see cref="Direction.None"/> is specified, the default starting direction
        /// is used, which is UP for CARDINALS/EIGHT_WAY, and UP_LEFT for DIAGONALS.
        /// </param>
        /// <returns>All neighbors of the given location.</returns>
        public static IEnumerable<Position2D> NeighborsCounterClockwise(this AdjacencyRule r, Position2D startingLocation, Direction startingDirection = default)
        {
            foreach (var dir in r.DirectionsOfNeighborsCounterClockwise(startingDirection))
                yield return startingLocation + dir;
        }

        /// <summary>
        /// Gets all neighbors of the specified location, based on the current adjacency method.
        /// Neighbors are returned in counter-clockwise order, starting with the neighbor in the given
        /// starting direction.
        /// </summary>
        /// <param name="startingX">X-Position2D of location to return neighbors for.</param>
        /// <param name="startingY">Y-Position2D of location to return neighbors for.</param>
        /// <param name="startingDirection">
        /// The neighbor in this direction will be returned first, proceeding counter-clockwise.
        /// If null or <see cref="Direction.None"/> is specified, the default starting direction
        /// is used, which is UP for CARDINALS/EIGHT_WAY, and UP_LEFT for DIAGONALS.
        /// </param>
        /// <returns>All neighbors of the given location.</returns>
        public static IEnumerable<Position2D> NeighborsCounterClockwise(this AdjacencyRule r, int startingX, int startingY, Direction startingDirection = default)
            => r.NeighborsCounterClockwise(new Position2D(startingX, startingY), startingDirection);

        public static ReadOnlyListWrapper<Direction> DirectionsOfNeighbors(this AdjacencyRule type)
        {
            switch (type)
            {
                case AdjacencyRule.Cardinals:
                {
                    return CardinalNeighbours;
                }

                case AdjacencyRule.Diagonals:
                {
                    return DiagonalNeighbours;
                }

                case AdjacencyRule.EightWay:
                {
                    return EightWayNeighbours;
                }
                default:
                    throw new ArgumentException();
            }
        }
    }
}