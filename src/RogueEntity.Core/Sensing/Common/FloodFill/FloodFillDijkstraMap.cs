using System;
using System.Diagnostics.CodeAnalysis;
using EnTTSharp.Entities;
using JetBrains.Annotations;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Directionality;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Algorithms;
using RogueEntity.Core.Sensing.Common.Physics;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.DataViews;

namespace RogueEntity.Core.Sensing.Common.FloodFill
{
    public class FloodFillDijkstraMap : DijkstraGridBase<Unit>
    {
        bool valid;
        IReadOnlyDynamicDataView2D<float> resistanceMap;
        IReadOnlyDynamicDataView2D<DirectionalityInformation> directionalityView;
        IReadOnlyBoundedDataView<float> resistanceTile;
        IReadOnlyBoundedDataView<DirectionalityInformation> directionalityTile;
        ReadOnlyListWrapper<Direction>[] directionData;

        SenseSourceDefinition Sense { get; set; }
        ISensePhysics SensePhysics { get; set; }
        Position2D origin;
        float radius;
        float intensity;

        FloodFillDijkstraMap(int extendX, int extendY) : base(Rectangle.WithRadius(0, 0, extendX, extendY))
        {
            valid = false;
        }

        public static FloodFillDijkstraMap Create(in SenseSourceDefinition sense,
                                                  float intensity,
                                                  in Position2D origin, 
                                                  [NotNull] ISensePhysics sensePhysics,
                                                  [NotNull] IReadOnlyDynamicDataView2D<float> resistanceMap,
                                                  [NotNull] IReadOnlyDynamicDataView2D<DirectionalityInformation> directionalityView)
        {
            var radius = sensePhysics.SignalRadiusForIntensity(intensity);
            var radiusInt = (int)Math.Ceiling(radius);
            var result = new FloodFillDijkstraMap(radiusInt, radiusInt);
            result.Configure(in sense, intensity, in origin, sensePhysics, resistanceMap, directionalityView);
            return result;
        }
        
        [SuppressMessage("ReSharper", "ParameterHidesMember")]
        public void Configure(in SenseSourceDefinition sense,
                              float intensity,
                              in Position2D origin, 
                              [NotNull] ISensePhysics sensePhysics,
                              [NotNull] IReadOnlyDynamicDataView2D<float> resistanceMap,
                              [NotNull] IReadOnlyDynamicDataView2D<DirectionalityInformation> directionalityView)
        {
            this.intensity = intensity;
            this.radius = sensePhysics.SignalRadiusForIntensity(intensity);
            this.directionalityView = directionalityView ?? throw new ArgumentNullException(nameof(directionalityView));
            this.origin = origin;
            this.Sense = sense;
            this.SensePhysics = sensePhysics ?? throw new ArgumentNullException(nameof(sensePhysics));
            this.resistanceMap = resistanceMap ?? throw new ArgumentNullException(nameof(resistanceMap));
            var radiusInt = (int)Math.Ceiling(radius);
            this.Resize(Rectangle.WithRadius(0, 0, radiusInt, radiusInt));
            this.directionData = DirectionalityLookup.Get(Sense.AdjacencyRule);
            this.valid = true;
        }

        public void Reset()
        {
            this.resistanceTile = null;
            this.directionalityTile = null;
            this.Sense = default;
            this.SensePhysics = null;
            this.resistanceMap = null;
            this.directionData = null;
            this.valid = false;
        }

        public void Compute(SenseSourceData data)
        {
            try
            {
                base.PrepareScan();
                base.EnqueueStartingNode(new ShortPosition2D(), Math.Abs(intensity));
                base.RescanMap(Bounds.Width * Bounds.Height);

                var sgn = Math.Sign(intensity);
                foreach (var pos in Bounds.Contents)
                {
                    var shortPos = new ShortPosition2D(pos.X, pos.Y);
                    if (!TryGetCumulativeCost(shortPos, out var cost))
                    {
                        continue;
                    }

                    var flags = SenseDataFlags.None;
                    var senseDirection = SenseDirection.None;
                    if (TryGetPreviousStep(shortPos, out var prev))
                    {
                        var delta = pos - prev;
                        senseDirection = SenseDirectionStore.From(delta.X, delta.Y).Direction;

                        var tx = origin.X + pos.X;
                        var ty = origin.Y + pos.Y;

                        var resistance = resistanceMap.TryGetMapValue(ref resistanceTile, tx, ty, 1);
                        flags |= resistance >= 1 ? SenseDataFlags.Obstructed : SenseDataFlags.None;
                    }

                    if (pos == default)
                    {
                        flags |= SenseDataFlags.SelfIlluminating;
                    }

                    data.Write(pos, sgn * cost, senseDirection, flags);
                }
            }
            finally
            {
                this.resistanceTile = null;
                this.directionalityTile = null;
            }
        }

        protected override ReadOnlyListWrapper<Direction> PopulateTraversableDirections(ShortPosition2D basePos)
        {
            var targetPosX = basePos.X + origin.X;
            var targetPosY = basePos.Y + origin.Y;
            var allowedMovements = directionalityView.TryGetMapValue(ref directionalityTile, targetPosX, targetPosY, DirectionalityInformation.All);

            return directionData[(int)allowedMovements];
        }

        protected override void UpdateNode(in ShortPosition2D nextNodePos, Unit nodeInfo)
        {
        }

        protected override bool EdgeCostInformation(in ShortPosition2D stepOrigin, in Direction d, float stepOriginCost, out float totalPathCost, out Unit nodeInfo)
        {
            var targetPoint = origin + stepOrigin + d.ToCoordinates();
            if (!valid)
            {
                totalPathCost = default;
                return false;
            }

            var resistance = resistanceMap.TryGetMapValue(ref resistanceTile, targetPoint.X, targetPoint.Y, 1).Clamp(0, 1);
            if (resistance >= 1)
            {
                totalPathCost = default;
                return false;
            }

            var distanceForStep = Sense.DistanceCalculation.Calculate2D(d.ToCoordinates());
            var signalStrength = SensePhysics.SignalStrengthAtDistance((float) ((intensity - stepOriginCost) + distanceForStep), radius);
            var totalCost = intensity * signalStrength * (1 - resistance);
            totalPathCost = totalCost;
            return true;
        }
    }
}