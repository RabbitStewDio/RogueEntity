using System;
using JetBrains.Annotations;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Utils.Maps;

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

        public SenseSourceData Calculate<TResistanceMap>(SenseSourceDefinition sense, 
                                                         Position2D position, 
                                                         TResistanceMap resistanceMap, 
                                                         SenseSourceData data = null)
            where TResistanceMap : IReadOnlyView2D<float>
        {
            var radius = (int)Math.Ceiling(sensePhysics.SignalRadiusForIntensity(sense.Intensity));
            if (data == null || data.Radius != radius)
            {
                data = new SenseSourceData(radius);
            }
            else
            {
                data.Reset();
            }

            data.Write(new Position2D(0, 0), sense.Intensity, SenseDirection.None, SenseDataFlags.SelfIlluminating);

            var ndata = dataSource.Create(sense, position, sensePhysics, resistanceMap);
            ndata.ResultMap.Compute(data);
            return data;
        }
    }
}