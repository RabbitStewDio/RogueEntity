using RogueEntity.Core.GridProcessing.Directionality;
using RogueEntity.Core.GridProcessing.LayerAggregation;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Algorithms;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.DataViews;
using System;

namespace RogueEntity.Core.Movement.CostModifier.Directions
{
    /// <summary>
    ///   Calculates the acceptable outbound movements into a given cell.
    /// </summary>
    /// <typeparam name="TMovementMode"></typeparam>
    public sealed class OutboundMovementDirectionalitySystem<TMovementMode> : AdjacencyGridTransformSystem<float>, 
                                                                              IOutboundMovementDirectionView<TMovementMode>,
                                                                              IDisposable
    {
        readonly IAggregationCacheControl cacheControl;

        public OutboundMovementDirectionalitySystem(IReadOnlyDynamicDataView3D<float> sourceData,
                                                    IAggregationCacheControl cacheControl = null,
                                                    AdjacencyRule adjacencyRule = AdjacencyRule.EightWay) : base(sourceData, adjacencyRule)
        {
            this.cacheControl = cacheControl;
            if (cacheControl != null)
            {
                cacheControl.PositionDirty += OnPositionDirty;
            }
        }

        void OnPositionDirty(object sender, PositionDirtyEventArgs e)
        {
            MarkDirty(e);
        }

        public void ProcessSystem() => Process();

        protected override bool IsMoveAllowed(in (IReadOnlyDynamicDataView2D<float> sourceData,
                                                  IReadOnlyBoundedDataView<float> sourceTile, int z) parameterData,
                                              in Position2D pos,
                                              Direction d)
        {
            var c = d.ToCoordinates();
            var moveData = QueryMovementCost(in parameterData, pos.X + c.X, pos.Y + c.Y);
            var isMoveAllowed = moveData > 0;

            if (!isMoveAllowed || d.IsCardinal())
            {
                return isMoveAllowed;
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

        public void Dispose()
        {
            if (cacheControl != null)
            {
                cacheControl.PositionDirty -= OnPositionDirty;
            }
        }
    }
}
