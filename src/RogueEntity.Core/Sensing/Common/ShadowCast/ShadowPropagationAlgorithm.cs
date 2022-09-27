using System;
using RogueEntity.Core.GridProcessing.Directionality;
using RogueEntity.Core.Positioning.Algorithms;
using RogueEntity.Core.Sensing.Common.Physics;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.DataViews;
using static RogueEntity.Core.Sensing.Common.ShadowCast.ShadowPropagationAlgorithmHelpers;

namespace RogueEntity.Core.Sensing.Common.ShadowCast
{
    public class ShadowPropagationAlgorithm : ISensePropagationAlgorithm
    {
        readonly struct ShadowParameters
        {
            public readonly IReadOnlyDynamicDataView2D<float> ResistanceMap;
            public readonly SenseSourceDefinition Sense;
            public readonly float Intensity;
            public readonly Position2D Origin;
            public readonly IReadOnlyDynamicDataView2D<DirectionalityInformation> DirectionalityView;

            public ShadowParameters(in IReadOnlyDynamicDataView2D<float> resistanceMap,
                                    in SenseSourceDefinition sense,
                                    float intensity,
                                    in Position2D origin,
                                    IReadOnlyDynamicDataView2D<DirectionalityInformation> directionalityView)
            {
                this.ResistanceMap = resistanceMap;
                this.Sense = sense;
                this.Intensity = intensity;
                this.Origin = origin;
                this.DirectionalityView = directionalityView;
            }
        }

        readonly ISensePhysics sensePhysics;
        readonly ShadowPropagationResistanceDataSource dataSource;
        IReadOnlyBoundedDataView<float>? resistanceTile;
        IReadOnlyBoundedDataView<DirectionalityInformation>? directionalityTile;

        public ShadowPropagationAlgorithm(ISensePhysics sensePhysics,
                                          ShadowPropagationResistanceDataSource dataSource)
        {
            this.sensePhysics = sensePhysics ?? throw new ArgumentNullException(nameof(sensePhysics));
            this.dataSource = dataSource ?? throw new ArgumentNullException(nameof(dataSource));
        }

        public SenseSourceData Calculate(in SenseSourceDefinition sense,
                                         float intensity,
                                         in Position2D position,
                                         IReadOnlyDynamicDataView2D<float> resistanceMap,
                                         IReadOnlyDynamicDataView2D<DirectionalityInformation> directionalityView,
                                         SenseSourceData? data = null)
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

            try
            {
                var resistanceData = dataSource.Create(radius);

                data.Write(new Position2D(0, 0), intensity, SenseDirection.None, SenseDataFlags.SelfIlluminating);
                resistanceData[0, 0] = 1;
                var shadowParam = new ShadowParameters(resistanceMap, sense, intensity, position, directionalityView);
                foreach (var d in DiagonalDirectionsOfNeighbors)
                {
                    var delta = d.ToCoordinates();
                    ShadowCast(1, 1.0f, 0.0f, new PropagationDirection(0, delta.X, delta.Y, 0), in shadowParam, data, resistanceData);
                    ShadowCast(1, 1.0f, 0.0f, new PropagationDirection(delta.X, 0, 0, delta.Y), in shadowParam, data, resistanceData);
                }

                return data;
            }
            finally
            {
                directionalityTile = null;
                resistanceTile = null;
            }
        }

        bool IsBlockedByDirectionalityMap(in ShadowParameters p,
                                          int prevX,
                                          int prevY,
                                          int currentX,
                                          int currentY)
        {
            var dirInfo = p.DirectionalityView.TryGetMapValue(ref directionalityTile, prevX + p.Origin.X, prevY + p.Origin.Y, DirectionalityInformation.None);
            if (dirInfo == DirectionalityInformation.None)
            {
                return true;
            }

            var direction = Directions.GetDirection(prevX, prevY, currentX, currentY);
            if (!dirInfo.IsMovementAllowed(direction))
            {
                return true;
            }

            return false;
        }

        void ShadowCast(int row,
                        float start,
                        float end,
                        in PropagationDirection pd,
                        in ShadowParameters p,
                        in SenseSourceData data,
                        ShadowPropagationResistanceData resistanceData)
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
                    var resistance = p.ResistanceMap.TryGetMapValue(ref resistanceTile, globalCurrentX, globalCurrentY, 1);

                    var fullyBlocked = IsFullyBlocked(resistance);
                    var blockedByDirections = IsBlockedByDirectionalityMap(in p, prevX, prevY, currentX, currentY);

                    if (distanceFromOrigin <= radius)
                    {
                        // filter out cases where 
                        if (!blockedByDirections || fullyBlocked)
                        {
                            var strengthAtDistance = sensePhysics.SignalStrengthAtDistance(distanceFromOrigin, maxRadius);
                            var bright = p.Intensity * strengthAtDistance * signalStrength;
                            if (bright > 0.0005f)
                            {
                                var senseDirection = SenseDirectionStore.From(currentX, currentY).Direction;
                                data.Write(new Position2D(currentX, currentY), bright, senseDirection,
                                           fullyBlocked ? SenseDataFlags.Obstructed : SenseDataFlags.None);
                            }
                            else
                            {
                                // Ignore all very weak signals. They do not add anything to the actual output
                                // but continue to keep the CPU busy for a few more turns. 
                                signalStrength = 0;
                            }
                        }
                        else
                        {
                            // eradicate all stray light signals
                            signalStrength = 0;
                        }
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