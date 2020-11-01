using System;
using System.Threading;

namespace RogueEntity.Core.Sensing.Common.ShadowCast
{
    public class ShadowPropagationResistanceDataSource : IDisposable
    {
        readonly ThreadLocal<ShadowPropagationResistanceData> dataStore;

        public ShadowPropagationResistanceDataSource()
        {
            this.dataStore = new ThreadLocal<ShadowPropagationResistanceData>();
        }

        public void Dispose()
        {
            dataStore?.Dispose();
        }

        public ShadowPropagationResistanceData Create(int radiusInt)
        {
            if (dataStore.IsValueCreated)
            {
                var value = dataStore.Value;
                value.Reset(radiusInt);
                return value;
            }
            else
            {
                var v = new ShadowPropagationResistanceData(radiusInt);
                dataStore.Value = v;
                return v;
            }
        }
    }
}