using System.Collections.Generic;
using RogueEntity.Core.Sensing.Resistance;
using RogueEntity.Core.Utils.DataViews;

namespace RogueEntity.Core.Tests.Sensing
{
    public class SensePropertiesSourceFixture<TSense>: IReadOnlyDynamicDataView3D<SensoryResistance<TSense>>
    {
        readonly Dictionary<int, DynamicDataView<SensoryResistance<TSense>>> backend;

        public SensePropertiesSourceFixture()
        {
            backend = new Dictionary<int, DynamicDataView<SensoryResistance<TSense>>>();
            OffsetX = 0;
            OffsetY = 0;
            TileSizeX = 64;
            TileSizeY = 64;
        }

        public int OffsetX { get; }
        public int OffsetY { get; }
        public int TileSizeX { get; }
        public int TileSizeY { get; }

        public DynamicDataView<SensoryResistance<TSense>> GetOrCreate(int z)
        {
            if (backend.TryGetValue(z, out var v))
            {
                return v;
            }
            
            v = new DynamicDataView<SensoryResistance<TSense>>(OffsetX, OffsetY, TileSizeX, TileSizeY);
            backend[z] = v;
            return v;
        }

        public List<int> GetActiveLayers(List<int> buffer = null)
        {
            if (buffer == null)
            {
                buffer = new List<int>();
            }
            else
            {
                buffer.Clear();
            }
            
            buffer.AddRange(backend.Keys);
            return buffer;
        }

        public bool TryGetView(int z, out IReadOnlyDynamicDataView2D<SensoryResistance<TSense>> data)
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