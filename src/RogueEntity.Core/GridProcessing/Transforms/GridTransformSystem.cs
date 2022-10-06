using RogueEntity.Core.GridProcessing.LayerAggregation;
using RogueEntity.Core.Utils.DataViews;
using System;

namespace RogueEntity.Core.GridProcessing.Transforms
{
    public abstract class GridTransformSystem<TSourceData, TTargetData>: GridTransformSystemBase<TSourceData, TTargetData>
    {
        readonly GridAggregateDataView3D targetData;

        protected GridTransformSystem(IReadOnlyDynamicDataView3D<TSourceData> sourceData): base(sourceData.ToConfiguration())
        {
            this.SourceData = sourceData;
            this.targetData = new GridAggregateDataView3D(sourceData.ToConfiguration());
        }

        protected override IReadOnlyDynamicDataView3D<TSourceData> SourceData { get; }
        protected override IDynamicDataView3D<TTargetData> TargetData => targetData;

        public IAggregateDynamicDataView3D<TTargetData> ResultView => targetData;

        protected override void RemoveTargetDataLayer(int z)
        {
            targetData.RemoveView(z);
        }

        protected override void FireViewProcessedEvent(int zPosition, 
                                                       IReadOnlyDynamicDataView2D<TTargetData> sourceLayer, 
                                                       IBoundedDataView<TTargetData> resultTile)
        {
            targetData.FireViewProcessedEvent(this, new AggregationViewProcessedEvent<TTargetData>(targetData, zPosition, sourceLayer, resultTile));
        }

        class GridAggregateDataView3D : DynamicDataView3D<TTargetData>, IAggregateDynamicDataView3D<TTargetData>
        {
            public GridAggregateDataView3D(DynamicDataViewConfiguration config) : base(config)
            {
            }

            public event EventHandler<AggregationViewProcessedEvent<TTargetData>>? ViewProcessed;
            
            internal void FireViewProcessedEvent(GridTransformSystem<TSourceData, TTargetData> source, AggregationViewProcessedEvent<TTargetData> evt)
            {
                ViewProcessed?.Invoke(source, evt);
            }
        }
    }
}