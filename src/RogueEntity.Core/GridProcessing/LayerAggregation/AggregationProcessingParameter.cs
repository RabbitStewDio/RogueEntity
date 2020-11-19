using RogueEntity.Api.Utils;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.DataViews;

namespace RogueEntity.Core.GridProcessing.LayerAggregation
{
    public readonly struct AggregationProcessingParameter<TAggregationData, TSourceData>
    {
        public readonly Rectangle Bounds;
        public readonly ReadOnlyListWrapper<IReadOnlyDynamicDataView2D<TSourceData>> DataViews;
        public readonly IBoundedDataView<TAggregationData> WritableTile;

        public AggregationProcessingParameter(Rectangle bounds,
                                              ReadOnlyListWrapper<IReadOnlyDynamicDataView2D<TSourceData>> dataViews,
                                              IBoundedDataView<TAggregationData> boundedDataView)
        {
            Bounds = bounds;
            DataViews = dataViews;
            WritableTile = boundedDataView;
        }
    }
}