using RogueEntity.Api.Utils;
using RogueEntity.Core.Movement.CostModifier.Directions;
using RogueEntity.Core.Movement.CostModifier.Map;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Algorithms;
using RogueEntity.Core.Positioning.Grid;
using RogueEntity.Core.Positioning.MapLayers;
using RogueEntity.Core.Sensing.Common;
using RogueEntity.Core.Sensing.Common.Physics;
using RogueEntity.Core.Sensing.Common.ShadowCast;
using RogueEntity.Core.Utils;
using System;

namespace RogueEntity.Core.MovementPlaning.Pathfinding.SingleLevel
{
    public class LineOfSightTargetEvaluator<TMovementMode> : IPathFinderTargetEvaluator
    {
        readonly Physics physics;
        readonly ShadowPropagationAlgorithm algo;

        int targetDistance;
        int zLevel;
        GridPosition2D targetPosition;
        GridPosition2D sourcePosition;
        SenseSourceData? result;
        
        readonly IRelativeMovementCostSystem<TMovementMode> resistanceMap;
        readonly IOutboundMovementDirectionView<TMovementMode> directionMap;
        readonly Action<LineOfSightTargetEvaluator<TMovementMode>>? returnToPoolAction;

        public LineOfSightTargetEvaluator(ShadowPropagationResistanceDataSource data,
                                          IRelativeMovementCostSystem<TMovementMode> resistanceMap,
                                          IOutboundMovementDirectionView<TMovementMode> directionMap,
                                          Action<LineOfSightTargetEvaluator<TMovementMode>>? returnToPoolAction = null)
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
            where TPosition : IPosition<TPosition>
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
            where TPosition : IPosition<TPosition>
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

        public bool IsTargetNode(int z, in GridPosition2D pos)
        {
            Assert.NotNull(result);
            
            if (z == zLevel && pos == targetPosition) return true;
            return result[pos.X - sourcePosition.X, pos.Y - sourcePosition.Y] > 0;
        }

        public BufferList<EntityGridPosition> CollectTargets(BufferList<EntityGridPosition>? buffer = null)
        {
            buffer = BufferList.PrepareBuffer(buffer);
            buffer.Add(EntityGridPosition.Of(MapLayer.Indeterminate, targetPosition.X, targetPosition.Y, zLevel));
            return buffer;
        }

        public float TargetHeuristic(int z, in GridPosition2D pos)
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