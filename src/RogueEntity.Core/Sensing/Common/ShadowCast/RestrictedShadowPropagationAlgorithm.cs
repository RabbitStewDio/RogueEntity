using System;
using GoRogue;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Utils.Maps;
using static RogueEntity.Core.Sensing.Common.ShadowCast.ShadowPropagationAlgorithmHelpers;

namespace RogueEntity.Core.Sensing.Common.ShadowCast
{
    public class RestrictedShadowPropagationAlgorithm : ISensePropagationAlgorithm
    {
        public SenseSourceData Calculate<TResistanceMap>(SenseSourceDefinition sense,
                                                         Position2D position,
                                                         TResistanceMap resistanceMap,
                                                         SenseSourceData data = null)
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

            ShadowCastLimited(1, 1.0f, 0.0f, new PropagationDirection(0, 1, 1, 0), resistanceMap,
                              sense, position, data);
            ShadowCastLimited(1, 1.0f, 0.0f, new PropagationDirection(1, 0, 0, 1), resistanceMap,
                              sense, position, data);

            ShadowCastLimited(1, 1.0f, 0.0f, new PropagationDirection(0, -1, 1, 0), resistanceMap,
                              sense, position, data);

            ShadowCastLimited(1, 1.0f, 0.0f, new PropagationDirection(-1, 0, 0, 1), resistanceMap,
                              sense, position, data);

            ShadowCastLimited(1, 1.0f, 0.0f, new PropagationDirection(0, -1, -1, 0), resistanceMap,
                              sense, position, data);

            ShadowCastLimited(1, 1.0f, 0.0f, new PropagationDirection(-1, 0, 0, -1), resistanceMap,
                              sense, position, data);

            ShadowCastLimited(1, 1.0f, 0.0f, new PropagationDirection(0, 1, -1, 0), resistanceMap,
                              sense, position, data);

            ShadowCastLimited(1, 1.0f, 0.0f, new PropagationDirection(1, 0, 0, -1), resistanceMap,
                              sense, position, data);

            return data;
        }


        void ShadowCastLimited<TResistanceMap>(int row,
                                               float start,
                                               float end,
                                               in PropagationDirection p,
                                               TResistanceMap resistanceMap,
                                               in SenseSourceDefinition sense,
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

            var radius = (int)sense.Radius;
            var intensity = sense.Intensity;
            var dist = sense.DistanceCalculation;
            var decay = sense.Decay;
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

                    var fullyBlocked = IsFullyBlocked(resistanceMap[gCurrentX, gCurrentY]); 
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
                        var deltaRadius = dist.Calculate(deltaX, deltaY);
                        var at2 = Math.Abs(angle - MathHelpers.ScaledAtan2Approx(currentY, currentX));
                        if (deltaRadius <= radius && (at2 <= span * 0.5 || at2 >= 1.0 - span * 0.5))
                        {
                            var bright = intensity - decay * deltaRadius;
                            light.Write(new Position2D(currentX, currentY), bright, fullyBlocked ? SenseDataFlags.Obstructed : SenseDataFlags.None);
                        }

                        if (fullyBlocked && distance < radius) // Wall within FOV
                        {
                            blocked = true;
                            ShadowCastLimited(distance + 1, start, leftSlope, p, resistanceMap, sense, pos, light);
                            newStart = rightSlope;
                        }
                    }
                }
            }
            
        }
    }
}