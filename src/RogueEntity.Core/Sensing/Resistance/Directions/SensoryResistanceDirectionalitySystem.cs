using RogueEntity.Core.Directionality;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.Algorithms;
using RogueEntity.Core.Utils.DataViews;

namespace RogueEntity.Core.Sensing.Resistance.Directions
{
    public class SensoryResistanceDirectionalitySystem<TGameContext, TSense> : AdjacencyGridTransformSystem<float>, ISensoryResistanceDirectionView<TSense>
    {
        public SensoryResistanceDirectionalitySystem(IReadOnlyDynamicDataView3D<float> sourceData) : base(sourceData)
        {
        }

        public void ProcessSystem(TGameContext x) => Process();

        protected override bool IsMoveAllowed(in (IReadOnlyDynamicDataView2D<float> sourceData,
                                                  IReadOnlyBoundedDataView<float> sourceTile, int z) parameterData,
                                              in Position2D pos,
                                              Direction d)
        {
            var (cx, cy) = d.ToCoordinates();
            var (x, y) = pos;

            if (d.IsCardinal())
            {
                var moveData = Query(in parameterData, x + cx, y + cy);
                var isMoveAllowed = moveData < 1;
                return isMoveAllowed;
            }

            var moveDataHorizontal = Query(in parameterData, x + cx, y);
            var canMoveHorizontal = moveDataHorizontal < 1;

            var moveDataVertical = Query(in parameterData, x, y + cy);
            var canMoveVertical = moveDataVertical < 1;

            // if both cardinal directions are blocked at the same time, we cannot walk diagonally.
            return canMoveHorizontal || canMoveVertical;
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