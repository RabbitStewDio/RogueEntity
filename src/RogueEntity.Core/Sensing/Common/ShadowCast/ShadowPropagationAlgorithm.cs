using System;
using GoRogue;
using JetBrains.Annotations;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Sensing.Common.Physics;
using RogueEntity.Core.Utils.Algorithms;
using RogueEntity.Core.Utils.Maps;
using static RogueEntity.Core.Sensing.Common.ShadowCast.ShadowPropagationAlgorithmHelpers;

namespace RogueEntity.Core.Sensing.Common.ShadowCast
{
    public class ShadowPropagationAlgorithm: ISensePropagationAlgorithm
    {
        readonly ISensePhysics sensePhysics;

        public ShadowPropagationAlgorithm([NotNull] ISensePhysics sensePhysics)
        {
            this.sensePhysics = sensePhysics ?? throw new ArgumentNullException(nameof(sensePhysics));
        }

        public SenseSourceData Calculate<TResistanceMap>(SenseSourceDefinition sense,
                                                         Position2D position,
                                                         TResistanceMap resistanceMap,
                                                         SenseSourceData data = null)
            where TResistanceMap : IReadOnlyView2D<float>
        {
            var radius = (int)Math.Ceiling(sensePhysics.SignalRadiusForIntensity(sense.Intensity));
            if (data == null || data.Radius != radius)
            {
                data = new SenseSourceData(radius);
            }
            else
            {
                data.Reset();
            }

            data.Write(new Position2D(0, 0), sense.Intensity, SenseDirection.None, SenseDataFlags.SelfIlluminating);
            foreach (var d in DiagonalDirectionsOfNeighbors)
            {
                var delta = d.ToCoordinates();
                ShadowCast(1, 1.0f, 0.0f, new PropagationDirection(0, delta.X, delta.Y, 0), in resistanceMap, in sense, in position, data);
                ShadowCast(1, 1.0f, 0.0f, new PropagationDirection(delta.X, 0, 0, delta.Y), in resistanceMap, in sense, in position, data);
            }

            return data;
        }

        void ShadowCast<TResistanceMap>(int row,
                                        float start,
                                        float end,
                                        in PropagationDirection pd,
                                        in TResistanceMap resistanceMap,
                                        in SenseSourceDefinition sense,
                                        in Position2D origin,
                                        in SenseSourceData data)
            where TResistanceMap : IReadOnlyView2D<float>
        {
            var newStart = 0f;
            if (start < end)
            {
                return;
            }

            var intensity = sense.Intensity;
            var dist = sense.DistanceCalculation;
            var maxRadius = sensePhysics.SignalRadiusForIntensity(intensity);
            var radius = (int)Math.Ceiling(maxRadius);
            
            var blocked = false;
            for (var distance = row; !blocked && distance <= radius; distance++)
            {
                var deltaY = -distance;
                for (var deltaX = -distance; deltaX <= 0; deltaX++)
                {
                    var currentX = deltaX * pd.xx + deltaY * pd.xy;
                    var currentY = deltaX * pd.yx + deltaY * pd.yy;

                    var globalCurrentX = origin.X + currentX;
                    var globalCurrentY = origin.Y + currentY;
                    var leftSlope = (deltaX - 0.5f) / (deltaY + 0.5f);
                    var rightSlope = (deltaX + 0.5f) / (deltaY - 0.5f);

                    if (start < rightSlope)
                    {
                        continue;
                    }

                    if (end > leftSlope)
                    {
                        break;
                    }

                    var distanceFromOrigin = (float) dist.Calculate(deltaX, deltaY, 0);
                    var resistance = resistanceMap[globalCurrentX, globalCurrentY];
                    var fullyBlocked = IsFullyBlocked(resistance);
                    if (distanceFromOrigin <= radius)
                    {
                        var strengthAtDistance = sensePhysics.SignalStrengthAtDistance(distanceFromOrigin, maxRadius);
                        var bright = intensity * strengthAtDistance * (1 - resistance);
                        data.Write(new Position2D(currentX, currentY), bright,
                                   SenseDirectionStore.From(currentX, currentY).Direction,
                                   fullyBlocked ? SenseDataFlags.Obstructed : SenseDataFlags.None);
                    }

                    if (blocked) // Previous cell was blocked
                    {
                        if (fullyBlocked)
                        {
                            newStart = rightSlope;
                        }
                        else
                        {
                            blocked = false;
                            start = newStart;
                        }
                    }
                    else if (fullyBlocked && distance < radius) // Wall within FOV
                    {
                        blocked = true;
                        newStart = rightSlope;
                        ShadowCast(distance + 1, start, leftSlope, in pd, in resistanceMap, in sense, in origin, in data);
                    }
                }
            }
            
        }
    }
}