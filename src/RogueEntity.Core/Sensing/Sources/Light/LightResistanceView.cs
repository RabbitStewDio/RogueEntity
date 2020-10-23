using RogueEntity.Core.Sensing.Resistance;
using RogueEntity.Core.Utils.Maps;

namespace RogueEntity.Core.Sensing.Sources.Light
{
    public class LightResistanceView : IReadOnlyView2D<float>
    {
        readonly IReadOnlyView2D<SensoryResistance> cellProperties;

        public LightResistanceView(IReadOnlyView2D<SensoryResistance> cellProperties)
        {
            this.cellProperties = cellProperties;
        }

        public bool TryGet(int x, int y, out float data)
        {
            if (cellProperties.TryGet(x, y, out var raw))
            {
                data = raw.BlocksLight;
                return true;
            }

            data = default;
            return false;
        }

        public float this[int x, int y]
        {
            get
            {
                return cellProperties[x, y].BlocksLight;
            }
        }
    }
}