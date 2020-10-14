using System;
using System.Collections.Generic;
using GoRogue;

namespace RogueEntity.Core.Utils
{
    public static class AdjacencyRuleExtensions2
    {
        static readonly ReadOnlyListWrapper<Direction> CardinalNeighbours;
        static readonly ReadOnlyListWrapper<Direction> DiagonalNeighbours;
        static readonly ReadOnlyListWrapper<Direction> EightWayNeighbours;

        static AdjacencyRuleExtensions2()
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
        
        public static ReadOnlyListWrapper<Direction> DirectionsOfNeighborsList(this AdjacencyRule type)
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