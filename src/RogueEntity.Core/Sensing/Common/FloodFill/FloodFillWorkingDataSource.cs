using System;
using System.Threading;
using JetBrains.Annotations;
using RogueEntity.Core.Directionality;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Sensing.Common.Physics;
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
            dataStore?.Dispose();
        }

        public FloodFillWorkingData Create(in SenseSourceDefinition sense,
                                           float intensity,
                                           in Position2D origin,
                                           [NotNull] ISensePhysics sensePhysics,
                                           [NotNull] IReadOnlyView2D<float> resistanceMap,
                                           [NotNull] IReadOnlyView2D<DirectionalityInformation> directionalityView)
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