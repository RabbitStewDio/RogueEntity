using System;
using RogueEntity.Core.GridProcessing.Directionality;
using RogueEntity.Core.Sensing.Common.Physics;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.DataViews;

namespace RogueEntity.Core.Sensing.Common.FloodFill
{
    public class FloodFillPropagationAlgorithm: ISensePropagationAlgorithm
    {
        readonly FloodFillWorkingDataSource dataSource;
        readonly ISensePhysics sensePhysics;

        public FloodFillPropagationAlgorithm(ISensePhysics sensePhysics, FloodFillWorkingDataSource dataSource)
        {
            this.dataSource = dataSource ?? throw new ArgumentNullException(nameof(dataSource));
            this.sensePhysics = sensePhysics ?? throw new ArgumentNullException(nameof(sensePhysics));
        }

        public SenseSourceData Calculate(in SenseSourceDefinition sense,
                                         float intensity,
                                         in GridPosition2D position,
                                         IReadOnlyDynamicDataView2D<float> resistanceMap,
                                         IReadOnlyDynamicDataView2D<DirectionalityInformation> directionalityView,
                                         SenseSourceData? data = null)
        {
            var radius = (int)Math.Ceiling(sensePhysics.SignalRadiusForIntensity(intensity));
            if (data == null || data.Radius != radius)
            {
                data = new SenseSourceData(radius);
            }
            else
            {
                data.Reset();
            }


            var ndata = dataSource.Create(sense, intensity, position, sensePhysics, resistanceMap, directionalityView);
            ndata.ResultMap.Compute(data);
            return data;
        }
    }
}