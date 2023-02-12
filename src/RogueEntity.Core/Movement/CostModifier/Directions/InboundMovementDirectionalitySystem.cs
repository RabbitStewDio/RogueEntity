using RogueEntity.Core.GridProcessing.Directionality;
using RogueEntity.Core.GridProcessing.LayerAggregation;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Positioning.Algorithms;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.DataViews;
using System;
using System.Collections.Generic;
using Direction = RogueEntity.Core.Positioning.Algorithms.Direction;

namespace RogueEntity.Core.Movement.CostModifier.Directions
{
    /// <summary>
    ///   Calculates the acceptable inbound movements into a given cell.
    /// </summary>
    /// <typeparam name="TMovementMode"></typeparam>
    public sealed class InboundMovementDirectionalitySystem<TMovementMode> : AdjacencyGridTransformSystem<float>,
                                                                             IInboundMovementDirectionView<TMovementMode>,
                                                                             IDisposable
    {
        readonly IAggregationCacheControl? cacheControl;
        readonly List<Direction> optimizedDirections;

        public InboundMovementDirectionalitySystem(IReadOnlyDynamicDataView3D<float> sourceData,
                                                   IAggregationCacheControl? cacheControl = null,
                                                   AdjacencyRule adjacencyRule = AdjacencyRule.EightWay) : base(sourceData, adjacencyRule)
        {
            this.cacheControl = cacheControl;
            if (this.cacheControl != null)
            {
                this.cacheControl.PositionDirty += OnPositionDirty;
            }

            this.optimizedDirections = new List<Direction>();
            foreach (var n in Neighbors)
            {
                if (n.IsCardinal())
                {
                    continue;
                }

                optimizedDirections.Add(n);
            }
        }

        public void Dispose()
        {
            if (this.cacheControl != null)
            {
                this.cacheControl.PositionDirty -= OnPositionDirty;
            }
        }

        void OnPositionDirty(object sender, PositionDirtyEventArgs e)
        {
            MarkDirty(e);
        }

        public void ProcessSystem() => Process();


        protected override void ProcessTile(ProcessingParameters args)
        {
            var (bounds, z, sourceLayer, sourceTile, resultTile) = args;
            var parameterData = (sourceLayer, sourceTile, z);
            foreach (var pos in bounds.Contents)
            {
                var selfCost = QueryMovementCost(in parameterData, pos.X, pos.Y);
                if (selfCost <= 0)
                {
                    resultTile.TrySet(pos.X, pos.Y, DirectionalityInformation.None);
                    continue;
                }

                var x = DirectionalityInformation.Left | DirectionalityInformation.Right | DirectionalityInformation.Up | DirectionalityInformation.Down;
                for (var index = 0; index < optimizedDirections.Count; index++)
                {
                    var d = optimizedDirections[index];
                    if (IsMoveAllowed(in parameterData, in pos, d))
                    {
                        x = x.With(d);
                    }
                }

                resultTile.TrySet(pos.X, pos.Y, x);
            }
        }

        protected override bool IsMoveAllowed(in (IReadOnlyDynamicDataView2D<float> sourceData,
                                                  IReadOnlyBoundedDataView<float> sourceTile, int z) parameterData,
                                              in GridPosition2D pos,
                                              Direction d)
        {
            var c = d.ToCoordinates();
            
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
            if (!parameterData.sourceTile.TryGet(x, y, out var moveData) && 
                !parameterData.sourceData.TryGet(x, y, out moveData))
            {
                return 0;
            }

            return moveData;
        }
    }
}