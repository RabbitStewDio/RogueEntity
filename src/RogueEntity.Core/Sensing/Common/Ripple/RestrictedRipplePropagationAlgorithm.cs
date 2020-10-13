using System;
using System.Collections.Generic;
using System.Linq;
using GoRogue;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Utils.Maps;
using static RogueEntity.Core.Sensing.Common.Ripple.RipplePropagationAlgorithmHelpers;

namespace RogueEntity.Core.Sensing.Common.Ripple
{
    public class RestrictedRipplePropagationAlgorithm
    {
        static readonly List<Direction> EightWayDirectionsOfNeighborsCounterClockwiseRight =
            AdjacencyRule.EightWay.DirectionsOfNeighborsCounterClockwise(Direction.Right).ToList();

        readonly int rippleValue;

        public RestrictedRipplePropagationAlgorithm(int rippleValue)
        {
            this.rippleValue = rippleValue;
        }

        public (SenseSourceData, RippleSenseData) Calculate<TResistanceMap>(in SenseSourceDefinition sense,
                                                                            in Position2D pos,
                                                                            in TResistanceMap resistanceMap,
                                                                            SenseSourceData data = null,
                                                                            RippleSenseData nearData = null)
            where TResistanceMap : IReadOnlyView2D<float>
        {
            var radius = (int)Math.Ceiling(sense.Radius);
            if (data == null ||
                data.Radius != radius)
            {
                data = new SenseSourceData(radius);
            }
            else
            {
                data.Reset();
            }

            if (nearData == null ||
                nearData.Radius != radius)
            {
                nearData = new RippleSenseData(radius);
            }
            else
            {
                nearData.Reset();
            }

            var angle = sense.Angle * MathHelpers.DegreePctOfCircle;
            var span = sense.Span * MathHelpers.DegreePctOfCircle;
            DoRippleFOV(rippleValue, in resistanceMap, angle, span, in sense, in pos, in data, in nearData);
            return (data, nearData);
        }


        void DoRippleFOV<TResistanceMap>(int ripple,
                                         in TResistanceMap resistanceMap,
                                         double angle,
                                         double span,
                                         in SenseSourceDefinition sense,
                                         in Position2D pos,
                                         in SenseSourceData light,
                                         in RippleSenseData nearLight)
            where TResistanceMap : IReadOnlyView2D<float>
        {
            var radius = (int)sense.Radius;
            var distanceCalc = sense.DistanceCalculation;
            var dq = nearLight.OpenNodes;
            dq.Enqueue(new Position2D(0, 0)); // Add starting point
            while (dq.Count != 0)
            {
                var p = dq.Dequeue();

                if (light[p.X, p.Y] <= 0 || nearLight[p.X, p.Y])
                {
                    continue; // Nothing left to spread!
                }

                foreach (var dir in EightWayDirectionsOfNeighborsCounterClockwiseRight)
                {
                    var delta = dir.ToCoordinates();
                    var x2 = p.X + delta.X;
                    var y2 = p.Y + delta.Y;
                    var globalX2 = pos.X - radius + x2;
                    var globalY2 = pos.Y - radius + y2;

                    if (distanceCalc.Calculate(x2, y2) > radius)
                    {
                        // +1 covers starting tile at least
                        continue;
                    }

                    var at2 = Math.Abs(angle - MathHelpers.ScaledAtan2Approx(y2, x2));
                    if (at2 > span * 0.5 && at2 < 1.0 - span * 0.5)
                    {
                        continue;
                    }

                    var surroundingLight = NearRippleLight(x2, y2, globalX2, globalY2, ripple, resistanceMap, sense, pos, light, nearLight);
                    light.TryQuery(x2, y2, out var currentLight, out var directionality, out var flags);
                    if (currentLight < surroundingLight)
                    {
                        var fullyBlocked = IsFullyBlocked(resistanceMap[globalX2, globalY2]);
                        light.Write(new Position2D(x2, y2), surroundingLight, flags | (fullyBlocked ? SenseDataFlags.Obstructed : SenseDataFlags.None));
                        if (!fullyBlocked)
                        {
                            // Not fully blocking (like with an wall).
                            // Need to redo neighbors, since we just changed this entry's light.
                            dq.Enqueue(new Position2D(x2, y2));
                        }
                    }
                }
            }
        }
    }
}