using RogueEntity.Core.Utils.DataViews;

namespace RogueEntity.Core.GridProcessing.Transforms
{
    public abstract class GridTransformSystem<TSourceData, TTargetData>: GridTransformSystemBase<TSourceData, TTargetData>
    {
        readonly DynamicDataView3D<TTargetData> targetData;

        protected GridTransformSystem(IReadOnlyDynamicDataView3D<TSourceData> sourceData): base(sourceData.ToConfiguration())
        {
            this.SourceData = sourceData;
            this.targetData = new DynamicDataView3D<TTargetData>(sourceData.ToConfiguration());
        }

        protected override IReadOnlyDynamicDataView3D<TSourceData> SourceData { get; }
        protected override IDynamicDataView3D<TTargetData> TargetData => targetData;

        public IReadOnlyDynamicDataView3D<TTargetData> ResultView => TargetData;

        protected override void RemoveTargetDataLayer(int z)
        {
            targetData.RemoveView(z);
        }
    }
}