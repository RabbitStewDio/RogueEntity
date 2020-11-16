using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using RogueEntity.Api.Utils;
using RogueEntity.Core.Directionality;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Sensing.Common.Physics;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.Algorithms;
using RogueEntity.Core.Utils.DataViews;
using Rectangle = RogueEntity.Core.Utils.Rectangle;

namespace RogueEntity.Core.Sensing.Common.FloodFill
{
    public class FloodFillDijkstraMap : DijkstraGridBase
    {
        bool valid;
        IReadOnlyView2D<float> ResistanceMap { get; set; }
        SenseSourceDefinition Sense { get; set; }
        ISensePhysics SensePhysics { get; set; }
        ReadOnlyListWrapper<Direction> directions;
        Position2D origin;
        float radius;
        float intensity;
        IReadOnlyView2D<DirectionalityInformation> directionalityView;

        FloodFillDijkstraMap(in Rectangle bounds) : base(in bounds)
        {
            this.directions = ReadOnlyListWrapper<Direction>.Empty;
            valid = false;
        }

        public static FloodFillDijkstraMap Create(in SenseSourceDefinition sense,
                                                  float intensity,
                                                  in Position2D origin, 
                                                  [NotNull] ISensePhysics sensePhysics,
                                                  [NotNull] IReadOnlyView2D<float> resistanceMap,
                                                  [NotNull] IReadOnlyView2D<DirectionalityInformation> directionalityView)
        {
            var radius = sensePhysics.SignalRadiusForIntensity(intensity);
            Console.WriteLine("Using Origin: " + origin + " Radius: " + radius);
            
            var radiusInt = (int)Math.Ceiling(radius);
            var bounds = new Rectangle(origin.X - radiusInt, origin.Y - radiusInt, 2 * radiusInt + 1, 2 * radiusInt + 1);
            var result = new FloodFillDijkstraMap(in bounds);
            result.Configure(in sense, intensity, in origin, sensePhysics, resistanceMap, directionalityView);
            
            Console.WriteLine("Using bounds " + bounds);
            
            return result;
        }
        
        [SuppressMessage("ReSharper", "ParameterHidesMember")]
        public void Configure(in SenseSourceDefinition sense,
                              float intensity,
                              in Position2D origin, 
                              [NotNull] ISensePhysics sensePhysics,
                              [NotNull] IReadOnlyView2D<float> resistanceMap,
                              [NotNull] IReadOnlyView2D<DirectionalityInformation> directionalityView)
        {
            this.intensity = intensity;
            this.radius = sensePhysics.SignalRadiusForIntensity(intensity);
            this.directionalityView = directionalityView ?? throw new ArgumentNullException(nameof(directionalityView));
            this.origin = origin;
            this.Sense = sense;
            this.SensePhysics = sensePhysics ?? throw new ArgumentNullException(nameof(sensePhysics));
            this.ResistanceMap = resistanceMap ?? throw new ArgumentNullException(nameof(resistanceMap));
            var radiusInt = (int)Math.Ceiling(radius);
            this.Resize(new Rectangle(origin.X - radiusInt, origin.Y - radiusInt, 2 * radiusInt + 1, 2 * radiusInt + 1));
            this.directions = Sense.AdjacencyRule.DirectionsOfNeighbors();
            this.valid = true;
        }

        public void Reset()
        {
            this.Sense = default;
            this.SensePhysics = null;
            this.ResistanceMap = null;
            this.directions = ReadOnlyListWrapper<Direction>.Empty;
            this.valid = false;
        }

        public void Compute(SenseSourceData data)
        {
            base.PrepareScan();
            base.EnqueueStartingNode(in origin, Math.Abs(intensity));
            base.RescanMap(out _, Bounds.Width * Bounds.Height);

            var sgn = Math.Sign(intensity);
            foreach (var pos in Bounds.Contents)
            {
                if (!TryGetCumulativeCost(pos, out var cost))
                {
                    continue;
                }

                var flags = SenseDataFlags.None;
                var senseDirection = SenseDirection.None;
                if (TryGetPreviousStep(pos, out var prev))
                {
                    var delta = pos - prev;
                    senseDirection = SenseDirectionStore.From(delta.X, delta.Y).Direction;
                    flags |= ResistanceMap[pos.X, pos.Y] >= 1 ? SenseDataFlags.Obstructed : SenseDataFlags.None;
                }
                if (pos == origin)
                {
                    flags |= SenseDataFlags.SelfIlluminating;
                }
                data.Write(pos - origin, sgn * cost, senseDirection, flags);
                
            }
        }

        protected override void PopulateDirections(Position2D basePosition, List<Direction> buffer)
        {
            buffer.Clear();
            if (!directionalityView.TryGet(basePosition.X, basePosition.Y, out var dir))
            {
                dir = DirectionalityInformation.All;
            }
            
            foreach (var d in directions)
            {
                if (dir.IsMovementAllowed(d))
                {
                    buffer.Add(d);
                }
            }
        }

        protected override bool EdgeCostInformation(in Position2D stepOrigin, in Direction d, float stepOriginCost, out float cost)
        {
            var targetPoint = stepOrigin + d.ToPosition2D();
            if (!valid)
            {
                cost = default;
                return false;
            }

            var resistance = ResistanceMap[targetPoint.X, targetPoint.Y].Clamp(0, 1);
            if (resistance >= 1)
            {
                cost = default;
                return false;
            }

            var distanceForStep = Sense.DistanceCalculation.Calculate(d.ToCoordinates());
            var signalStrength = SensePhysics.SignalStrengthAtDistance((float) ((intensity - stepOriginCost) + distanceForStep), radius);
            var totalCost = intensity * signalStrength * (1 - resistance);
            cost = stepOriginCost - totalCost;
            return true;
        }
    }
}