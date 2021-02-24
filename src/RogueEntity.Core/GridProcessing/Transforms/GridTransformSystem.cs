using RogueEntity.Core.Utils.DataViews;

namespace RogueEntity.Core.GridProcessing.Transforms
{
    public abstract class GridTransformSystem<TSourceData, TTargetData>: GridTransformSystemBase<TSourceData, TTargetData>
    {
        protected GridTransformSystem(IReadOnlyDynamicDataView3D<TSourceData> sourceData): base(sourceData.ToConfiguration())
        {
            this.SourceData = sourceData;
            this.TargetData = new DynamicDataView3D<TTargetData>(sourceData.ToConfiguration());
        }

        protected override IReadOnlyDynamicDataView3D<TSourceData> SourceData { get; }
        protected override IDynamicDataView3D<TTargetData> TargetData { get; }

        public IReadOnlyDynamicDataView3D<TTargetData> ResultView => TargetData;
    }
}