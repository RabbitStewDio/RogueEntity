using System;
using GoRogue;
using JetBrains.Annotations;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.Algorithms;
using RogueEntity.Core.Utils.Maps;

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
        
        FloodFillDijkstraMap(in Rectangle bounds) : base(in bounds)
        {
            this.directions = ReadOnlyListWrapper<Direction>.Empty;
            valid = false;
        }

        public static FloodFillDijkstraMap Create(in SenseSourceDefinition sense,
                                                  in Position2D origin,
                                                  [NotNull] ISensePhysics sensePhysics,
                                                  [NotNull] IReadOnlyView2D<float> resistanceMap)
        {
            Console.WriteLine("Using Origin: " + origin + " Radius: " + sense.Radius);
            
            var radiusInt = (int)Math.Ceiling(sense.Radius);
            var bounds = new Rectangle(origin.X - radiusInt, origin.Y - radiusInt, 2 * radiusInt + 1, 2 * radiusInt + 1);
            var result = new FloodFillDijkstraMap(in bounds);
            result.Configure(in sense, in origin, sensePhysics, resistanceMap);
            
            Console.WriteLine("Using bounds " + bounds);
            
            return result;
        }
        
        public void Configure(in SenseSourceDefinition sense,
                              in Position2D origin,
                              [NotNull] ISensePhysics sensePhysics,
                              [NotNull] IReadOnlyView2D<float> resistanceMap)
        {
            this.origin = origin;
            this.Sense = sense;
            this.SensePhysics = sensePhysics ?? throw new ArgumentNullException(nameof(sensePhysics));
            this.ResistanceMap = resistanceMap ?? throw new ArgumentNullException(nameof(resistanceMap));
            var radiusInt = (int)Math.Ceiling(sense.Radius);
            this.Resize(new Rectangle(origin.X - radiusInt, origin.Y - radiusInt, 2 * radiusInt + 1, 2 * radiusInt + 1));
            this.directions = Sense.DistanceCalculation.AsAdjacencyRule().DirectionsOfNeighborsList();
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
            base.EnqueueStartingNode(in origin, Math.Abs(Sense.Intensity));
            base.RescanMap(out _, Bounds.Width * Bounds.Height);

            var sgn = Math.Sign(Sense.Intensity);
            var bounds = Bounds;
            for (var y = bounds.MinExtentY; y <= bounds.MaxExtentY; y += 1)
            for (var x = bounds.MinExtentX; x <= bounds.MaxExtentX; x += 1)
            {
                var pos = new Position2D(x, y);
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
                if (pos == default)
                {
                    flags |= SenseDataFlags.SelfIlluminating;
                }
                data.Write(pos - origin, sgn * cost, senseDirection, flags);
                
            }
        }

        protected override ReadOnlyListWrapper<Direction> AdjacencyRule => directions;

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
            var signalStrength = SensePhysics.SignalStrengthAtDistance((Sense.Intensity - stepOriginCost) + distanceForStep, Sense.Radius);
            var totalCost = Sense.Intensity * signalStrength * (1 - resistance);
            cost = stepOriginCost - totalCost;
            return true;
        }
    }
}