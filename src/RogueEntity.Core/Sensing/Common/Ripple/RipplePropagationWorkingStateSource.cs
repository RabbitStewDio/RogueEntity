using System.Threading;

namespace RogueEntity.Core.Sensing.Common.Ripple
{
    public class RipplePropagationWorkingStateSource: IRipplePropagationWorkingStateSource
    {
        readonly ThreadLocal<RippleSenseData> dataStore;

        public RippleSenseData CreateData(int radius)
        {
            if (dataStore.IsValueCreated)
            {
                var s =  dataStore.Value;
                if (s.Radius >= radius)
                {
                    return s;
                }
            }
            
            var data = new RippleSenseData(radius);
            dataStore.Value = data;
            return data;
        }

        public void Dispose()
        {
            dataStore?.Dispose();
        }
    }
}