using System;
using JetBrains.Annotations;
using RogueEntity.Core.Directionality;
using RogueEntity.Core.Sensing.Common.Physics;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.DataViews;

namespace RogueEntity.Core.Sensing.Common.FloodFill
{
    public class FloodFillPropagationAlgorithm: ISensePropagationAlgorithm
    {
        readonly FloodFillWorkingDataSource dataSource;
        readonly ISensePhysics sensePhysics;

        public FloodFillPropagationAlgorithm([NotNull] ISensePhysics sensePhysics, [NotNull] FloodFillWorkingDataSource dataSource)
        {
            this.dataSource = dataSource ?? throw new ArgumentNullException(nameof(dataSource));
            this.sensePhysics = sensePhysics ?? throw new ArgumentNullException(nameof(sensePhysics));
        }

        public SenseSourceData Calculate<TResistanceMap>(in SenseSourceDefinition sense,
                                                         float intensity,
                                                         in Position2D position,
                                                         in TResistanceMap resistanceMap,
                                                         IReadOnlyView2D<DirectionalityInformation> directionalityView,
                                                         SenseSourceData data = null)
            where TResistanceMap : IReadOnlyView2D<float>
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
//            data.Write(new Position2D(0, 0), intensity, SenseDirection.None, SenseDataFlags.SelfIlluminating);
            return data;
        }
    }
}