using System;
using JetBrains.Annotations;
using RogueEntity.Core.Movement.CostModifier.Directions;
using RogueEntity.Core.Movement.CostModifier.Map;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Sensing.Common;
using RogueEntity.Core.Sensing.Common.Physics;
using RogueEntity.Core.Sensing.Common.ShadowCast;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.Algorithms;

namespace RogueEntity.Core.Movement.Pathfinding.SingleLevel
{
    public class LineOfSightTargetEvaluator<TMovementMode> : IPathFinderTargetEvaluator
    {
        readonly Physics physics;
        readonly ShadowPropagationAlgorithm algo;

        int targetDistance;
        int zLevel;
        Position2D targetPosition;
        Position2D sourcePosition;
        SenseSourceData result;
        
        readonly IRelativeMovementCostSystem<TMovementMode> resistanceMap;
        readonly IMovementResistanceDirectionView<TMovementMode> directionMap;
        readonly Action<LineOfSightTargetEvaluator<TMovementMode>> returnToPoolAction;

        public LineOfSightTargetEvaluator([NotNull] ShadowPropagationResistanceDataSource data,
                                          [NotNull] IRelativeMovementCostSystem<TMovementMode> resistanceMap,
                                          [NotNull] IMovementResistanceDirectionView<TMovementMode> directionMap,
                                          [CanBeNull] Action<LineOfSightTargetEvaluator<TMovementMode>> returnToPoolAction = null)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            this.resistanceMap = resistanceMap ?? throw new ArgumentNullException(nameof(resistanceMap));
            this.directionMap = directionMap ?? throw new ArgumentNullException(nameof(directionMap));
            this.returnToPoolAction = returnToPoolAction;
            this.physics = new Physics(DistanceCalculation.Euclid);
            this.algo = new ShadowPropagationAlgorithm(physics, data);
        }

        public LineOfSightTargetEvaluator<TMovementMode> WithTarget<TPosition>(TPosition position, int distance)
            where TPosition : IPosition
        {
            this.zLevel = position.GridZ;
            this.targetPosition = position.ToGridXY();
            this.targetDistance = Math.Max(1, distance);
            return this;
        }

        public void Dispose()
        {
            returnToPoolAction?.Invoke(this);
        }

        public void Activate()
        {
            targetDistance = 1;
        }

        public bool Initialize<TPosition>(in TPosition origin, DistanceCalculation c)
            where TPosition : IPosition
        {
            this.sourcePosition = origin.ToGridXY();
            this.physics.DistanceMeasurement = c;
            
            var sourceDef = new SenseSourceDefinition(physics.DistanceMeasurement, physics.AdjacencyRule, targetDistance);

            if (resistanceMap.ResultView.TryGetView(zLevel, out var resistanceView) &&
                directionMap.ResultView.TryGetView(zLevel, out var directionView))
            {
                result = algo.Calculate(sourceDef, targetDistance, targetPosition, resistanceView, directionView, result);
                return true;
            }


            return false;
        }

        public bool IsTargetNode(int z, in Position2D pos)
        {
            if (z == zLevel && pos == targetPosition) return true;
            return result[pos.X - sourcePosition.X, pos.Y - sourcePosition.Y] > 0;
        }

        public float TargetHeuristic(int z, in Position2D pos)
        {
            return (float) physics.DistanceMeasurement.Calculate2D(targetPosition, pos);
        }
        
        class Physics : ISensePhysics
        {
            public AdjacencyRule AdjacencyRule => DistanceMeasurement.AsAdjacencyRule();
            public DistanceCalculation DistanceMeasurement { get; set; }

            public Physics(DistanceCalculation distanceMeasurement)
            {
                DistanceMeasurement = distanceMeasurement;
            }

            public float SignalRadiusForIntensity(float intensity)
            {
                return (float)Math.Ceiling(intensity);
            }

            public float SignalStrengthAtDistance(float distance, float maxRadius)
            {
                if (distance > maxRadius)
                {
                    return 0;
                }
                return 1;
            }
        }
    }
}