using JetBrains.Annotations;
using Microsoft.Extensions.ObjectPool;
using RogueEntity.Core.Movement.CostModifier.Directions;
using RogueEntity.Core.Movement.CostModifier.Map;
using RogueEntity.Core.Sensing.Common.ShadowCast;
using System;

namespace RogueEntity.Core.MovementPlaning.Pathfinding.SingleLevel
{
    public class LineOfSightTargetEvaluatorPolicy<TMovementMode>: PooledObjectPolicy<LineOfSightTargetEvaluator<TMovementMode>>
    {
        [NotNull] readonly ShadowPropagationResistanceDataSource data;
        [NotNull] readonly IRelativeMovementCostSystem<TMovementMode> resistanceMap;
        [NotNull] readonly IOutboundMovementDirectionView<TMovementMode> directionMap;
        readonly Action<LineOfSightTargetEvaluator<TMovementMode>> returnToPoolAction;

        public LineOfSightTargetEvaluatorPolicy([NotNull] ShadowPropagationResistanceDataSource data,
                                                [NotNull] IRelativeMovementCostSystem<TMovementMode> resistanceMap,
                                                [NotNull] IOutboundMovementDirectionView<TMovementMode> directionMap,
                                                [NotNull] Action<LineOfSightTargetEvaluator<TMovementMode>> returnToPoolAction)
        {
            this.data = data ?? throw new ArgumentNullException(nameof(data));
            this.resistanceMap = resistanceMap ?? throw new ArgumentNullException(nameof(resistanceMap));
            this.directionMap = directionMap ?? throw new ArgumentNullException(nameof(directionMap));
            this.returnToPoolAction = returnToPoolAction ?? throw new ArgumentNullException(nameof(returnToPoolAction));
        }

        public override LineOfSightTargetEvaluator<TMovementMode> Create()
        {
            return new LineOfSightTargetEvaluator<TMovementMode>(data, resistanceMap, directionMap, returnToPoolAction);
        }

        public override bool Return(LineOfSightTargetEvaluator<TMovementMode> obj)
        {
            return true;
        }
    }
}