using System;
using System.Threading;
using JetBrains.Annotations;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Sensing.Common.Physics;
using RogueEntity.Core.Utils.Maps;

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
                                           [NotNull] IReadOnlyView2D<float> resistanceMap)
        {
            if (dataStore.IsValueCreated)
            {
                var value = dataStore.Value;
                value.Configure(in sense, intensity, in origin, sensePhysics, resistanceMap);
                return value;
            }
            else
            {
                var v = new FloodFillWorkingData(in sense, intensity, in origin, sensePhysics, resistanceMap);
                dataStore.Value = v;
                return v;
            }
        }
    }
}