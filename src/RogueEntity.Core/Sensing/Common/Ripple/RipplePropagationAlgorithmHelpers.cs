using System;
using System.Collections.Generic;
using System.Linq;
using GoRogue;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.Maps;

namespace RogueEntity.Core.Sensing.Common.Ripple
{
    public static class RipplePropagationAlgorithmHelpers
    {
        public static readonly ReadOnlyListWrapper<Direction> EightWayDirectionsOfNeighbors = AdjacencyRule.EightWay.DirectionsOfNeighbors().ToList();

        public static bool IsFullyBlocked(float v)
        {
            return v >= 1;
        }

        static List<Position2D> CollectNeighbours(List<Position2D> neighbors, 
                                                  int x,
                                                  int y,
                                                  in SenseSourceDefinition sense)
        {
            neighbors.Clear();
            
            var distanceCalc = sense.DistanceCalculation;
            foreach (var di in EightWayDirectionsOfNeighbors)
            {
                var delta = di.ToCoordinates();
                var x2 = x + delta.X;
                var y2 = y + delta.Y;

                var tmpDistance = distanceCalc.Calculate(x2, y2);
                var idx = 0;

                for (var i = 0; i < neighbors.Count; i++)
                {
                    var c = neighbors[i];
                    var testDistance = distanceCalc.Calculate(c.X, c.Y);
                    if (tmpDistance < testDistance)
                    {
                        break;
                    }

                    idx++;
                }

                neighbors.Insert(idx, new Position2D(x2, y2));
            }

            return neighbors;
        }

        public static float NearRippleLight<TResistanceMap>(int x,
                                                            int y,
                                                            int globalX,
                                                            int globalY,
                                                            int rippleNeighbors,
                                                            in TResistanceMap resistanceMap,
                                                            in SenseSourceDefinition sense,
                                                            in Position2D pos,
                                                            in SenseSourceData light,
                                                            in RippleSenseData nearLight)
            where TResistanceMap : IReadOnlyView2D<float>
        {
            if (x == 0 && y == 0)
            {
                return sense.Intensity;
            }

            var radius = (int)sense.Radius;
            var distanceCalc = sense.DistanceCalculation;
            var neighbors = CollectNeighbours(nearLight.NeighbourBuffer, x, y, sense);
            if (neighbors.Count == 0)
            {
                return 0;
            }

            float curLight = 0;
            var lit = 0;
            var indirects = 0;
            var maxIdx = Math.Min(neighbors.Count, rippleNeighbors);
            for (var index = 0; index < maxIdx; index++)
            {
                var posNeighborLocal = neighbors[index];
                var posNeighborGlobal = pos - radius + posNeighborLocal;
                var lightLevel = light[posNeighborLocal.X, posNeighborLocal.Y];
                if (lightLevel <= 0)
                {
                    continue;
                }

                lit++;
                if (nearLight[posNeighborLocal.X, posNeighborLocal.Y])
                {
                    indirects++;
                }

                float dist = distanceCalc.Calculate(x, y, posNeighborLocal.X, posNeighborLocal.Y);
                float resistance = resistanceMap[posNeighborGlobal.X, posNeighborGlobal.Y];
                if (posNeighborGlobal == pos)
                {
                    resistance = 0.0f;
                }

                var lightRaw = lightLevel - dist * sense.Decay;
                curLight = Math.Max(curLight, lightRaw * (1 - resistance));
            }

            if (IsFullyBlocked(resistanceMap[globalX, globalY]) || indirects >= lit)
            {
                nearLight[x, y] = true;
            }

            return curLight;
        }
    }
}