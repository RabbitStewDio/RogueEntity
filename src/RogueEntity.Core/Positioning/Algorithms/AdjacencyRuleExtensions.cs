using System;
using System.Collections.Generic;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Utils;

namespace RogueEntity.Core.Positioning.Algorithms
{
    public static class AdjacencyRuleExtensions
    {
        static readonly ReadOnlyListWrapper<Direction> cardinalNeighbours;
        static readonly ReadOnlyListWrapper<Direction> diagonalNeighbours;
        static readonly ReadOnlyListWrapper<Direction> eightWayNeighbours;

        static AdjacencyRuleExtensions()
        {
            cardinalNeighbours = new List<Direction>()
            {
                Direction.Up, Direction.Left, Direction.Down, Direction.Right
            };
            diagonalNeighbours = new List<Direction>()
            {
                Direction.UpLeft, Direction.UpRight, Direction.DownRight, Direction.DownLeft
            };
            eightWayNeighbours = new List<Direction>()
            {
                Direction.Up, Direction.Left, Direction.Down, Direction.Right, 
                Direction.UpLeft, Direction.UpRight, Direction.DownRight, Direction.DownLeft
            };
        }

        /// <summary>
        /// Gets directions leading to neighboring locations, according to the current adjacency
        /// method. Appropriate directions are returned in clockwise order from the given starting
        /// direction.
        /// </summary>
        /// <param name="type"></param>
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
        /// <param name="type"></param>
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
        /// <param name="r"></param>
        /// <param name="startingLocation">Location to return neighbors for.</param>
        /// <returns>All neighbors of the given location.</returns>
        public static IEnumerable<GridPosition2D> Neighbors(this AdjacencyRule r, GridPosition2D startingLocation)
        {
            foreach (var dir in r.DirectionsOfNeighbors())
                yield return startingLocation + dir;
        }

        /// <summary>
        /// Gets all neighbors of the specified location, based on the current adjacency method.
        /// Cardinals are returned before any diagonals.
        /// </summary>
        /// <param name="r"></param>
        /// <param name="startingX">X-Position2D of location to return neighbors for.</param>
        /// <param name="startingY">Y-Position2D of location to return neighbors for.</param>
        /// <returns>All neighbors of the given location.</returns>
        public static IEnumerable<GridPosition2D> Neighbors(this AdjacencyRule r, int startingX, int startingY) => r.Neighbors(new GridPosition2D(startingX, startingY));

        /// <summary>
        /// Gets all neighbors of the specified location, based on the current adjacency method.
        /// Neighbors are returned in clockwise order, starting with the neighbor in the given
        /// starting direction.
        /// </summary>
        /// <param name="r"></param>
        /// <param name="startingLocation">Location to return neighbors for.</param>
        /// <param name="startingDirection">
        /// The neighbor in this direction will be returned first, proceeding clockwise.
        /// If null or <see cref="Direction.None"/> is specified, the default starting direction
        /// is used, which is UP for CARDINALS/EIGHT_WAY, and UP_RIGHT for DIAGONALS.
        /// </param>
        /// <returns>All neighbors of the given location.</returns>
        public static IEnumerable<GridPosition2D> NeighborsClockwise(this AdjacencyRule r, GridPosition2D startingLocation, Direction startingDirection = default)
        {
            foreach (var dir in r.DirectionsOfNeighborsClockwise(startingDirection))
                yield return startingLocation + dir;
        }

        /// <summary>
        /// Gets all neighbors of the specified location, based on the current adjacency method.
        /// Neighbors are returned in clockwise order, starting with the neighbor in the given
        /// starting direction.
        /// </summary>
        /// <param name="r"></param>
        /// <param name="startingX">X-Position2D of location to return neighbors for.</param>
        /// <param name="startingY">Y-Position2D of location to return neighbors for.</param>
        /// <param name="startingDirection">
        /// The neighbor in this direction will be returned first, proceeding clockwise.
        /// If null or <see cref="Direction.None"/> is specified, the default starting direction
        /// is used, which is UP for CARDINALS/EIGHT_WAY, and UP_RIGHT for DIAGONALS.
        /// </param>
        /// <returns>All neighbors of the given location.</returns>
        public static IEnumerable<GridPosition2D> NeighborsClockwise(this AdjacencyRule r, int startingX, int startingY, Direction startingDirection = default)
            => r.NeighborsClockwise(new GridPosition2D(startingX, startingY), startingDirection);

        /// <summary>
        /// Gets all neighbors of the specified location, based on the current adjacency method.
        /// Neighbors are returned in counter-clockwise order, starting with the neighbor in the given
        /// starting direction.
        /// </summary>
        /// <param name="r"></param>
        /// <param name="startingLocation">Location to return neighbors for.</param>
        /// <param name="startingDirection">
        /// The neighbor in this direction will be returned first, proceeding counter-clockwise.
        /// If null or <see cref="Direction.None"/> is specified, the default starting direction
        /// is used, which is UP for CARDINALS/EIGHT_WAY, and UP_LEFT for DIAGONALS.
        /// </param>
        /// <returns>All neighbors of the given location.</returns>
        public static IEnumerable<GridPosition2D> NeighborsCounterClockwise(this AdjacencyRule r, GridPosition2D startingLocation, Direction startingDirection = default)
        {
            foreach (var dir in r.DirectionsOfNeighborsCounterClockwise(startingDirection))
                yield return startingLocation + dir;
        }

        /// <summary>
        /// Gets all neighbors of the specified location, based on the current adjacency method.
        /// Neighbors are returned in counter-clockwise order, starting with the neighbor in the given
        /// starting direction.
        /// </summary>
        /// <param name="r"></param>
        /// <param name="startingX">X-Position2D of location to return neighbors for.</param>
        /// <param name="startingY">Y-Position2D of location to return neighbors for.</param>
        /// <param name="startingDirection">
        /// The neighbor in this direction will be returned first, proceeding counter-clockwise.
        /// If null or <see cref="Direction.None"/> is specified, the default starting direction
        /// is used, which is UP for CARDINALS/EIGHT_WAY, and UP_LEFT for DIAGONALS.
        /// </param>
        /// <returns>All neighbors of the given location.</returns>
        public static IEnumerable<GridPosition2D> NeighborsCounterClockwise(this AdjacencyRule r, int startingX, int startingY, Direction startingDirection = default)
            => r.NeighborsCounterClockwise(new GridPosition2D(startingX, startingY), startingDirection);

        /// <summary>
        /// Gets directions leading to neighboring locations, according to the current adjacency
        /// method. Appropriate directions are returned in cardinal direction first, diagonal directions afterwards,
        /// both in clockwise order.
        /// </summary>
        /// <remarks>
        ///   This makes sure that search algorithms like Dijkstra and A-Star prefer cardinal movement to
        ///   diagonal movement when faced with the same cost. This just looks better than a drunken walk
        ///   swaying away from a direct line. 
        /// </remarks>
        /// <param name="type"></param>
        /// <returns>Directions that lead to neighboring locations.</returns>
        public static ReadOnlyListWrapper<Direction> DirectionsOfNeighbors(this AdjacencyRule type)
        {
            switch (type)
            {
                case AdjacencyRule.Cardinals:
                {
                    return cardinalNeighbours;
                }

                case AdjacencyRule.Diagonals:
                {
                    return diagonalNeighbours;
                }

                case AdjacencyRule.EightWay:
                {
                    return eightWayNeighbours;
                }
                default:
                    throw new ArgumentException();
            }
        }
    }
}