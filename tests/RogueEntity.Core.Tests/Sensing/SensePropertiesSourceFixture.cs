using System.Collections.Generic;
using RogueEntity.Core.Sensing.Resistance;
using RogueEntity.Core.Sensing.Resistance.Maps;
using RogueEntity.Core.Utils.Maps;

namespace RogueEntity.Core.Tests.Sensing
{
    public class SensePropertiesSourceFixture: ISensePropertiesSource
    {
        readonly Dictionary<int, DynamicDataView<SensoryResistance>> backend;

        public SensePropertiesSourceFixture()
        {
            backend = new Dictionary<int, DynamicDataView<SensoryResistance>>();
        }

        public DynamicDataView<SensoryResistance> GetOrCreate(int z)
        {
            if (backend.TryGetValue(z, out var v))
            {
                return v;
            }
            
            v = new DynamicDataView<SensoryResistance>(0, 0, 64, 64);
            backend[z] = v;
            return v;
        }
        
        public bool TryGet(int z, out IReadOnlyView2D<SensoryResistance> data)
        {
            if (backend.TryGetValue(z, out var x))
            {
                data = x;
                return true;
            }

            data = default;
            return false;
        }
    }
}