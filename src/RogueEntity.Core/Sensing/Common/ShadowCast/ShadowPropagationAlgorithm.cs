using System;
using JetBrains.Annotations;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Sensing.Common.Physics;
using RogueEntity.Core.Utils.Algorithms;
using RogueEntity.Core.Utils.Maps;
using static RogueEntity.Core.Sensing.Common.ShadowCast.ShadowPropagationAlgorithmHelpers;

namespace RogueEntity.Core.Sensing.Common.ShadowCast
{
    public class ShadowPropagationAlgorithm : ISensePropagationAlgorithm
    {
        readonly struct ShadowParameters<TResistanceMap>
            where TResistanceMap : IReadOnlyView2D<float>
        {
            public readonly TResistanceMap ResistanceMap;
            public readonly SenseSourceDefinition Sense;
            public readonly float Intensity;
            public readonly Position2D Origin;

            public ShadowParameters(in TResistanceMap resistanceMap, in SenseSourceDefinition sense, float intensity, in Position2D origin)
            {
                this.ResistanceMap = resistanceMap;
                this.Sense = sense;
                this.Intensity = intensity;
                this.Origin = origin;
            }
        }

        readonly ISensePhysics sensePhysics;
        readonly ShadowPropagationResistanceDataSource dataSource;

        public ShadowPropagationAlgorithm([NotNull] ISensePhysics sensePhysics, 
                                          [NotNull] ShadowPropagationResistanceDataSource dataSource)
        {
            this.sensePhysics = sensePhysics ?? throw new ArgumentNullException(nameof(sensePhysics));
            this.dataSource = dataSource ?? throw new ArgumentNullException(nameof(dataSource));
        }

        public SenseSourceData Calculate<TResistanceMap>(in SenseSourceDefinition sense,
                                                         float intensity,
                                                         in Position2D position,
                                                         in TResistanceMap resistanceMap,
                                                         SenseSourceData data = null)
            where TResistanceMap : IReadOnlyView2D<float>
        {
            var radius = (int)Math.Ceiling(sensePhysics.SignalRadiusForIntensity(intensity));
            if (data == null || data.Radius != radius)
            {
                data = new SenseSourceData(radius);
            }
            else
            {
                data.Reset();
            }

            var resistanceData = dataSource.Create(radius); 
            
            data.Write(new Position2D(0, 0), intensity, SenseDirection.None, SenseDataFlags.SelfIlluminating);
            resistanceData[0, 0] = 1;
            var shadowParam = new ShadowParameters<TResistanceMap>(resistanceMap, sense, intensity, position);
            foreach (var d in DiagonalDirectionsOfNeighbors)
            {
                var delta = d.ToCoordinates();
                Console.WriteLine("!START --------------------");
                ShadowCast(1, 1.0f, 0.0f, new PropagationDirection(0, delta.X, delta.Y, 0), in shadowParam, data, resistanceData);
                Console.WriteLine("xx");
                ShadowCast(1, 1.0f, 0.0f, new PropagationDirection(delta.X, 0, 0, delta.Y), in shadowParam, data, resistanceData);
                Console.WriteLine("!END --------------------");
            }

            return data;
        }

        void ShadowCast<TResistanceMap>(int row,
                                        float start,
                                        float end,
                                        in PropagationDirection pd,
                                        in ShadowParameters<TResistanceMap> p,
                                        in SenseSourceData data,
                                        ShadowPropagationResistanceData resistanceData)
            where TResistanceMap : IReadOnlyView2D<float>
        {
            var newStart = 0f;
            if (start < end)
            {
                return;
            }

            var dist = p.Sense.DistanceCalculation;
            var maxRadius = sensePhysics.SignalRadiusForIntensity(p.Intensity);
            var radius = (int)Math.Ceiling(maxRadius);
            var blocked = false;
            for (var distance = row; !blocked && distance <= radius; distance++)
            {
                var deltaY = -distance;
                for (var deltaX = -distance; deltaX <= 0; deltaX++)
                {
                    var currentX = deltaX * pd.xx + deltaY * pd.xy;
                    var currentY = deltaX * pd.yx + deltaY * pd.yy;

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

                    var globalCurrentX = p.Origin.X + currentX;
                    var globalCurrentY = p.Origin.Y + currentY;
                    int prevX;
                    int prevY;
                    if (deltaX != 0)
                    {
                        prevX = (deltaX + 1) * pd.xx + (deltaY + 1) * pd.xy;
                        prevY = (deltaX + 1) * pd.yx + (deltaY + 1) * pd.yy;
                    }
                    else
                    {
                        prevX = (deltaY + 1) * pd.xy;
                        prevY = (deltaY + 1) * pd.yy;
                    }

                    var signalStrength = resistanceData[prevX, prevY];
                    var distanceFromOrigin = (float)dist.Calculate(deltaX, deltaY, 0);
                    var resistance = p.ResistanceMap[globalCurrentX, globalCurrentY];
                    var fullyBlocked = IsFullyBlocked(resistance);
                    if (distanceFromOrigin <= radius)
                    {
                        var strengthAtDistance = sensePhysics.SignalStrengthAtDistance(distanceFromOrigin, maxRadius);
                        var bright = p.Intensity * strengthAtDistance * signalStrength;
                        data.Write(new Position2D(currentX, currentY), bright,
                                   SenseDirectionStore.From(currentX, currentY).Direction,
                                   fullyBlocked ? SenseDataFlags.Obstructed : SenseDataFlags.None);
                    }

                    resistanceData[currentX, currentY] = signalStrength * (1 - resistance);
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
                        ShadowCast(distance + 1, start, leftSlope, in pd, in p, in data, resistanceData);
                    }
                }
            }
        }
    }
}