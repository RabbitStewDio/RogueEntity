using System;
using System.Diagnostics;
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
            var c = d.ToCoordinates();
            if (d.IsCardinal())
            {
                var isMoveAllowed = parameterData.sourceData[pos.X + c.X, pos.Y + c.Y].BlocksSense < 1;
                return isMoveAllowed;
            }

            var canMoveHorizontal = parameterData.sourceData[pos.X + c.X, pos.Y].BlocksSense < 1;
            var canMoveVertical = parameterData.sourceData[pos.X, pos.Y + c.Y].BlocksSense < 1;

            // if both cardinal directions are blocked, we cannot walk diagonally.
            if (!canMoveHorizontal && !canMoveVertical)
            {
                return false;
            }
            
            return true;
        }
    }
}