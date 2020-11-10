using RogueEntity.Core.Directionality;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Utils.Algorithms;
using RogueEntity.Core.Utils.DataViews;

namespace RogueEntity.Core.Movement.CostModifier.Directions
{
    public class MovementResistanceDirectionalitySystem<TMovementMode> : AdjacencyGridTransformSystem<MovementCostModifier<TMovementMode>>, IMovementResistanceDirectionView<TMovementMode>
    {
        public MovementResistanceDirectionalitySystem(IReadOnlyDynamicDataView3D<MovementCostModifier<TMovementMode>> sourceData) : base(sourceData)
        {
        }

        public void ProcessSystem<TGameContext>(TGameContext x) => Process();

        protected override bool IsMoveAllowed(in (IReadOnlyDynamicDataView2D<MovementCostModifier<TMovementMode>> sourceData,
                                                  IReadOnlyBoundedDataView<MovementCostModifier<TMovementMode>> sourceTile, int z) parameterData,
                                              in Position2D pos,
                                              Direction d)
        {
            var c = d.ToCoordinates();
            if (d.IsCardinal())
            {
                var moveData = Query(in parameterData, pos.X + c.X, pos.Y + c.Y);
                var isMoveAllowed = moveData.CostModifier < 1;
                return isMoveAllowed;
            }

            var moveDataHorizontal = Query(in parameterData, pos.X + c.X, pos.Y);
            var canMoveHorizontal = moveDataHorizontal.CostModifier < 1;
            
            var moveDataVertical = Query(in parameterData, pos.X, pos.Y + c.Y);
            var canMoveVertical = moveDataVertical.CostModifier < 1;

            // if both cardinal directions are blocked, we cannot walk diagonally.
            if (!canMoveHorizontal && !canMoveVertical)
            {
                return false;
            }
            
            return true;
        }

        static MovementCostModifier<TMovementMode> Query(in (IReadOnlyDynamicDataView2D<MovementCostModifier<TMovementMode>> sourceData,
                                                           IReadOnlyBoundedDataView<MovementCostModifier<TMovementMode>> sourceTile, int z) parameterData,
                                                       int x,
                                                       int y)
        {
            if (!parameterData.sourceTile.TryGet(x, y, out var moveData))
            {
                moveData = parameterData.sourceData[x, y];
            }

            return moveData;
        }
    }
}