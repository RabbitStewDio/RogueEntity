using RogueEntity.Api.Utils;
using RogueEntity.Core.Positioning.Algorithms;
using System.Collections.Generic;

namespace RogueEntity.Core.GridProcessing.Directionality
{
    public static class DirectionalityLookup
    {
        static readonly Dictionary<AdjacencyRule, ReadOnlyListWrapper<Direction>[]> Data;

        static DirectionalityLookup()
        {
            Data = new Dictionary<AdjacencyRule, ReadOnlyListWrapper<Direction>[]>();
            foreach (var rule in new [] {AdjacencyRule.Cardinals, AdjacencyRule.Diagonals, AdjacencyRule.EightWay})
            {
                var availableDirections = rule.DirectionsOfNeighbors();
                var directionsSet = new ReadOnlyListWrapper<Direction>[256];
                for (int i = 0; i < 256; i += 1)
                {
                    var acceptableDirections = new List<Direction>();
                    var di = (DirectionalityInformation)i;

                    foreach (var d in availableDirections)
                    {
                        if (di.IsMovementAllowed(d))
                        {
                            acceptableDirections.Add(d);
                        }
                    }

                    directionsSet[i] = acceptableDirections;
                }

                Data[rule] = directionsSet;
            }
        }


        public static ReadOnlyListWrapper<Direction>[] Get(AdjacencyRule adjacencyRule)
        {
            return Data[adjacencyRule];
        }
    }
}