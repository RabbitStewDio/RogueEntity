using RogueEntity.Core.Directionality;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.Algorithms;
using RogueEntity.Core.Utils.DataViews;

namespace RogueEntity.Core.Movement.CostModifier.Directions
{
    public class MovementResistanceDirectionalitySystem<TMovementMode> : AdjacencyGridTransformSystem<float>, IMovementResistanceDirectionView<TMovementMode>
    {
        public MovementResistanceDirectionalitySystem(IReadOnlyDynamicDataView3D<float> sourceData,
                                                      AdjacencyRule adjacencyRule = AdjacencyRule.EightWay) : base(sourceData, adjacencyRule)
        {
        }

        public void ProcessSystem<TGameContext>(TGameContext x) => Process();

        protected override bool IsMoveAllowed(in (IReadOnlyDynamicDataView2D<float> sourceData,
                                                  IReadOnlyBoundedDataView<float> sourceTile, int z) parameterData,
                                              in Position2D pos,
                                              Direction d)
        {
            var c = d.ToCoordinates();
            if (d.IsCardinal())
            {
                var moveData = Query(in parameterData, pos.X + c.X, pos.Y + c.Y);
                var isMoveAllowed = moveData > 0;
                return isMoveAllowed;
            }

            var moveDataHorizontal = Query(in parameterData, pos.X + c.X, pos.Y);
            var canMoveHorizontal = moveDataHorizontal > 0;
            
            var moveDataVertical = Query(in parameterData, pos.X, pos.Y + c.Y);
            var canMoveVertical = moveDataVertical > 0;

            // if both cardinal directions are blocked, we cannot walk diagonally.
            if (!canMoveHorizontal && !canMoveVertical)
            {
                return false;
            }
            
            return true;
        }

        static float Query(in (IReadOnlyDynamicDataView2D<float> sourceData,
                                                           IReadOnlyBoundedDataView<float> sourceTile, int z) parameterData,
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