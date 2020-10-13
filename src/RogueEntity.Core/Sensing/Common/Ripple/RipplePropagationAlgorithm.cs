using System;
using GoRogue;
using JetBrains.Annotations;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Utils.Maps;
using static RogueEntity.Core.Sensing.Common.Ripple.RipplePropagationAlgorithmHelpers;

namespace RogueEntity.Core.Sensing.Common.Ripple
{
    public class RipplePropagationAlgorithm : ISensePropagationAlgorithm
    {
        readonly IRipplePropagationWorkingStateSource source;
        readonly int rippleValue;

        public RipplePropagationAlgorithm(int rippleValue, [NotNull] IRipplePropagationWorkingStateSource source)
        {
            this.rippleValue = rippleValue;
            this.source = source ?? throw new ArgumentNullException(nameof(source));
        }

        public SenseSourceData Calculate<TResistanceMap>(SenseSourceDefinition sense, Position2D position, TResistanceMap resistanceMap, SenseSourceData data = null)
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

            var nearData = source.CreateData(radius);
            nearData.Reset();
            DoRippleFOV(rippleValue, resistanceMap, sense, position, data, nearData);
            return data;
        }

        void DoRippleFOV<TResistanceMap>(int ripple,
                                         in TResistanceMap resistanceMap,
                                         in SenseSourceDefinition sense,
                                         in Position2D pos,
                                         in SenseSourceData light,
                                         in RippleSenseData nearLight)
            where TResistanceMap : IReadOnlyView2D<float>
        {
            var radius = (int)sense.Radius;
            var distanceCalc = sense.DistanceCalculation;
            var dq = nearLight.OpenNodes;
            dq.Clear();
            dq.Enqueue(new Position2D(0, 0)); // Add starting point
            while (dq.Count != 0)
            {
                var p = dq.Dequeue();

                if (light[p.X, p.Y] <= 0 || 
                    nearLight[p.X, p.Y])
                {
                    continue; // Nothing left to spread!
                }

                foreach (var dir in EightWayDirectionsOfNeighbors)
                {
                    var delta = dir.ToCoordinates();
                    var x2 = p.X + delta.X;
                    var y2 = p.Y + delta.Y;
                    var globalX2 = pos.X - radius + x2;
                    var globalY2 = pos.Y - radius + y2;

                    if (distanceCalc.Calculate(x2, y2) > radius) // +1 covers starting tile at least
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