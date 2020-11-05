using System;
using JetBrains.Annotations;
using RogueEntity.Core.Directionality;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Sensing.Common.Physics;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.Algorithms;
using RogueEntity.Core.Utils.DataViews;
using static RogueEntity.Core.Sensing.Common.ShadowCast.ShadowPropagationAlgorithmHelpers;

namespace RogueEntity.Core.Sensing.Common.ShadowCast
{
    public class RestrictedShadowPropagationAlgorithm : ISensePropagationAlgorithm
    {
        readonly ISensePhysics sensePhysics;

        public RestrictedShadowPropagationAlgorithm([NotNull] ISensePhysics sensePhysics)
        {
            this.sensePhysics = sensePhysics ?? throw new ArgumentNullException(nameof(sensePhysics));
        }

        public SenseSourceData Calculate<TResistanceMap>(in SenseSourceDefinition sense,
                                                         float intensity,
                                                         in Position2D position,
                                                         in TResistanceMap resistanceMap,
                                                         IReadOnlyView2D<DirectionalityInformation> readOnlyView2D,
                                                         SenseSourceData data = null)
            where TResistanceMap : IReadOnlyView2D<float>
        {
            var radius = (int)Math.Ceiling(sensePhysics.SignalRadiusForIntensity(intensity));
            if (data == null ||
                data.Radius != radius)
            {
                data = new SenseSourceData(radius);
            }
            else
            {
                data.Reset();
            }

            ShadowCastLimited(1, 1.0f, 0.0f, new PropagationDirection(0, 1, 1, 0), resistanceMap,
                              sense, intensity, position, data);
            ShadowCastLimited(1, 1.0f, 0.0f, new PropagationDirection(1, 0, 0, 1), resistanceMap,
                              sense, intensity, position, data);

            ShadowCastLimited(1, 1.0f, 0.0f, new PropagationDirection(0, -1, 1, 0), resistanceMap,
                              sense, intensity, position, data);

            ShadowCastLimited(1, 1.0f, 0.0f, new PropagationDirection(-1, 0, 0, 1), resistanceMap,
                              sense, intensity, position, data);

            ShadowCastLimited(1, 1.0f, 0.0f, new PropagationDirection(0, -1, -1, 0), resistanceMap,
                              sense, intensity, position, data);

            ShadowCastLimited(1, 1.0f, 0.0f, new PropagationDirection(-1, 0, 0, -1), resistanceMap,
                              sense, intensity, position, data);

            ShadowCastLimited(1, 1.0f, 0.0f, new PropagationDirection(0, 1, -1, 0), resistanceMap,
                              sense, intensity, position, data);

            ShadowCastLimited(1, 1.0f, 0.0f, new PropagationDirection(1, 0, 0, -1), resistanceMap,
                              sense, intensity, position, data);

            return data;
        }


        void ShadowCastLimited<TResistanceMap>(int row,
                                               float start,
                                               float end,
                                               in PropagationDirection p,
                                               TResistanceMap resistanceMap,
                                               in SenseSourceDefinition sense,
                                               float intensity,
                                               in Position2D pos,
                                               in SenseSourceData light)
            where TResistanceMap : IReadOnlyView2D<float>
        {
            if (start < end)
            {
                return;
            }

            var angle = sense.Angle * MathHelpers.DegreePctOfCircle;
            var span = sense.Span * MathHelpers.DegreePctOfCircle;

            float newStart = 0;

            var dist = sense.DistanceCalculation;
            var maxRadius = sensePhysics.SignalRadiusForIntensity(intensity);
            var radius = (int)Math.Ceiling(maxRadius);
            
            var blocked = false;
            for (var distance = row; distance <= radius && !blocked; distance++)
            {
                var deltaY = -distance;
                for (var deltaX = -distance; deltaX <= 0; deltaX++)
                {
                    var currentX = deltaX * p.xx + deltaY * p.xy;
                    var currentY = deltaX * p.yx + deltaY * p.yy;
                    var gCurrentX = pos.X + currentX;
                    var gCurrentY = pos.Y + currentY;

                    float leftSlope = (deltaX - 0.5f) / (deltaY + 0.5f);
                    float rightSlope = (deltaX + 0.5f) / (deltaY - 0.5f);

                    if (start < rightSlope)
                    {
                        continue;
                    }

                    if (end > leftSlope)
                    {
                        break;
                    }

                    var resistance = resistanceMap[gCurrentX, gCurrentY];
                    var fullyBlocked = IsFullyBlocked(resistance); 
                    if (blocked) // Previous cell was blocked
                    {
                        if (fullyBlocked) // Hit a wall...
                        {
                            newStart = rightSlope;
                        }
                        else
                        {
                            blocked = false;
                            start = newStart;
                        }
                    }
                    else
                    {
                        var at2 = Math.Abs(angle - MathHelpers.ScaledAtan2Approx(currentY, currentX));
                        var distanceFromOrigin = (float) dist.Calculate(deltaX, deltaY, 0);
                        if (distanceFromOrigin <= radius && (at2 <= span * 0.5 || at2 >= 1.0 - span * 0.5))
                        {
                            var strengthAtDistance = sensePhysics.SignalStrengthAtDistance(distanceFromOrigin, maxRadius);
                            var bright = intensity * strengthAtDistance * (1 - resistance);
                            light.Write(new Position2D(currentX, currentY), bright,
                                        SenseDirectionStore.From(currentX, currentY).Direction,
                                        fullyBlocked ? SenseDataFlags.Obstructed : SenseDataFlags.None);
                        }

                        if (fullyBlocked && distance < radius) // Wall within FOV
                        {
                            blocked = true;
                            ShadowCastLimited(distance + 1, start, leftSlope, p, resistanceMap, sense, intensity, pos, light);
                            newStart = rightSlope;
                        }
                    }
                }
            }
            
        }
    }
}