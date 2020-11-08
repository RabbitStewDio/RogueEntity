using RogueEntity.Core.Directionality;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Utils.Algorithms;
using RogueEntity.Core.Utils.DataViews;

namespace RogueEntity.Core.Sensing.Resistance.Directions
{
    public class SensoryResistanceDirectionalitySystem<TSense> : AdjacencyGridTransformSystem<SensoryResistance<TSense>>, ISensoryResistanceDirectionView<TSense>
    {
        public SensoryResistanceDirectionalitySystem(IReadOnlyDynamicDataView3D<SensoryResistance<TSense>> sourceData) : base(sourceData)
        {
        }

        public void ProcessSystem<TGameContext>(TGameContext x) => Process();

        protected override bool IsMoveAllowed(in (IReadOnlyDynamicDataView2D<SensoryResistance<TSense>> sourceData,
                                                  IReadOnlyBoundedDataView<SensoryResistance<TSense>> sourceTile, int z) parameterData,
                                              in Position2D pos,
                                              Direction d)
        {
            var (cx, cy) = d.ToCoordinates();
            var (x, y) = pos;
            
            if (d.IsCardinal())
            {
                var moveData = Query(in parameterData, x + cx, y + cy);
                var isMoveAllowed = moveData.BlocksSense < 1;
                return isMoveAllowed;
            }

            var moveDataHorizontal = Query(in parameterData, x + cx, y);
            var canMoveHorizontal = moveDataHorizontal.BlocksSense < 1;
            
            var moveDataVertical = Query(in parameterData, x, y + cy);
            var canMoveVertical = moveDataVertical.BlocksSense < 1;

            // if both cardinal directions are blocked at the same time, we cannot walk diagonally.
            return canMoveHorizontal || canMoveVertical;
        }
        
        static SensoryResistance<TSense> Query(in (IReadOnlyDynamicDataView2D<SensoryResistance<TSense>> sourceData,
                                                   IReadOnlyBoundedDataView<SensoryResistance<TSense>> sourceTile, int z) parameterData,
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