using RogueEntity.Api.Utils;
using RogueEntity.Core.GridProcessing.Transforms;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.Algorithms;
using RogueEntity.Core.Utils.DataViews;

namespace RogueEntity.Core.Directionality
{
    public abstract class AdjacencyGridTransformSystem<TSourceData>: GridTransformSystem<TSourceData, DirectionalityInformation>
    {
        readonly ReadOnlyListWrapper<Direction> neighbors;

        protected AdjacencyGridTransformSystem(IReadOnlyDynamicDataView3D<TSourceData> sourceData,
                                               AdjacencyRule adjacencyRule = AdjacencyRule.EightWay): base(sourceData)
        {
            this.neighbors = adjacencyRule.DirectionsOfNeighbors();
        }

        protected override void ProcessTile(ProcessingParameters args)
        {
            var (bounds, z, sourceLayer, sourceTile, resultTile) = args;
            var parameterData = (sourceLayer, sourceTile, z);
            foreach (var pos in bounds.Contents)
            {
                var x = DirectionalityInformation.None;
                foreach (var d in neighbors)
                {
                    if (IsMoveAllowed(in parameterData, in pos, d))
                    {
                        x = x.With(d);
                    }
                }

                resultTile[pos.X, pos.Y] = x;
            }
        }

        protected abstract bool IsMoveAllowed(in (IReadOnlyDynamicDataView2D<TSourceData> sourceData,
                                                  IReadOnlyBoundedDataView<TSourceData> sourceTile,
                                                  int z) parameterData,
                                              in Position2D pos,
                                              Direction d);
    }
}