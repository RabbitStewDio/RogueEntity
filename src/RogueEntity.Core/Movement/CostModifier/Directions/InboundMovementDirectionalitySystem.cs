using RogueEntity.Core.Directionality;
using RogueEntity.Core.Positioning.Algorithms;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.DataViews;

namespace RogueEntity.Core.Movement.CostModifier.Directions
{
    /// <summary>
    ///   Calculates the acceptable inbound movements into a given cell.
    /// </summary>
    /// <typeparam name="TMovementMode"></typeparam>
    public sealed class InboundMovementDirectionalitySystem<TMovementMode> : AdjacencyGridTransformSystem<float>, IInboundMovementDirectionView<TMovementMode>
    {
        public InboundMovementDirectionalitySystem(IReadOnlyDynamicDataView3D<float> sourceData,
                                                   AdjacencyRule adjacencyRule = AdjacencyRule.EightWay) : base(sourceData, adjacencyRule)
        { }

        public void ProcessSystem<TGameContext>(TGameContext x) => Process();

        
        protected override void ProcessTile(ProcessingParameters args)
        {
            var (bounds, z, sourceLayer, sourceTile, resultTile) = args;
            var parameterData = (sourceLayer, sourceTile, z);
            foreach (var pos in bounds.Contents)
            {
                
                var selfCost = QueryMovementCost(in parameterData, pos.X, pos.Y);
                if (selfCost <= 0)
                {
                    resultTile[pos.X, pos.Y] = DirectionalityInformation.None;
                    continue;
                }

                var x = DirectionalityInformation.None;
                for (var index = 0; index < Neighbors.Count; index++)
                {
                    var d = Neighbors[index];
                    if (IsMoveAllowed(in parameterData, in pos, d))
                    {
                        x = x.With(d);
                    }
                }

                resultTile[pos.X, pos.Y] = x;
            }
        }

        protected override bool IsMoveAllowed(in (IReadOnlyDynamicDataView2D<float> sourceData,
                                                  IReadOnlyBoundedDataView<float> sourceTile, int z) parameterData,
                                              in Position2D pos,
                                              Direction d)
        {
            if (d.IsCardinal())
            {
                return true;
            }

            var c = d.ToCoordinates();
            var movementCost = QueryMovementCost(in parameterData, pos.X + c.X, pos.Y + c.Y);
            var traversableCell = movementCost > 0;

            if (!traversableCell)
            {
                return true;
            }
            
            var moveDataHorizontal = QueryMovementCost(in parameterData, pos.X + c.X, pos.Y);
            var canMoveHorizontal = moveDataHorizontal > 0;

            var moveDataVertical = QueryMovementCost(in parameterData, pos.X, pos.Y + c.Y);
            var canMoveVertical = moveDataVertical > 0;

            // if both cardinal directions are blocked, we cannot walk diagonally.
            if (!canMoveHorizontal && !canMoveVertical)
            {
                return false;
            }

            return true;
        }

        static float QueryMovementCost(in (IReadOnlyDynamicDataView2D<float> sourceData,
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
