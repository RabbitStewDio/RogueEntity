using RogueEntity.Api.Utils;
using RogueEntity.Core.GridProcessing.Transforms;
using RogueEntity.Core.Positioning.Algorithms;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.DataViews;

namespace RogueEntity.Core.GridProcessing.Directionality
{
    public abstract class AdjacencyGridTransformSystem<TSourceData>: GridTransformSystem<TSourceData, DirectionalityInformation>
    {
        protected readonly ReadOnlyListWrapper<Direction> Neighbors;

        protected AdjacencyGridTransformSystem(IReadOnlyDynamicDataView3D<TSourceData> sourceData,
                                               AdjacencyRule adjacencyRule = AdjacencyRule.EightWay): base(sourceData)
        {
            this.Neighbors = adjacencyRule.DirectionsOfNeighbors();
        }

        protected override void ProcessTile(ProcessingParameters args)
        {
            var (bounds, z, sourceLayer, sourceTile, resultTile) = args;
            var parameterData = (sourceLayer, sourceTile, z);
            foreach (var pos in bounds.Contents)
            {
                var x = DirectionalityInformation.None;
                for (var index = 0; index < Neighbors.Count; index++)
                {
                    var d = Neighbors[index];
                    if (IsMoveAllowed(in parameterData, in pos, d))
                    {
                        x = x.With(d);
                    }
                }

                resultTile.TrySet(pos.X, pos.Y, x);
            }
        }

        protected abstract bool IsMoveAllowed(in (IReadOnlyDynamicDataView2D<TSourceData> sourceData,
                                                  IReadOnlyBoundedDataView<TSourceData> sourceTile,
                                                  int z) parameterData,
                                              in GridPosition2D pos,
                                              Direction d);
    }
}