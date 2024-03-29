using System;
using System.Threading;
using RogueEntity.Core.GridProcessing.Directionality;
using RogueEntity.Core.Sensing.Common.Physics;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.DataViews;

namespace RogueEntity.Core.Sensing.Common.FloodFill
{
    public class FloodFillWorkingDataSource : IDisposable
    {
        readonly ThreadLocal<FloodFillWorkingData> dataStore;

        public FloodFillWorkingDataSource()
        {
            this.dataStore = new ThreadLocal<FloodFillWorkingData>();
        }

        public void Dispose()
        {
            dataStore.Dispose();
        }

        public FloodFillWorkingData Create(in SenseSourceDefinition sense,
                                           float intensity,
                                           in GridPosition2D origin,
                                           ISensePhysics sensePhysics,
                                           IReadOnlyDynamicDataView2D<float> resistanceMap,
                                           IReadOnlyDynamicDataView2D<DirectionalityInformation> directionalityView)
        {
            if (dataStore.IsValueCreated)
            {
                var value = dataStore.Value;
                value.Configure(in sense, intensity, in origin, sensePhysics, resistanceMap, directionalityView);
                return value;
            }

            var v = new FloodFillWorkingData(in sense, intensity, in origin, sensePhysics, resistanceMap, directionalityView);
            dataStore.Value = v;
            return v;
        }
    }
}