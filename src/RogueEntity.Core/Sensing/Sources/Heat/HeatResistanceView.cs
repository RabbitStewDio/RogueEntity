using RogueEntity.Core.Sensing.Resistance;
using RogueEntity.Core.Utils.Maps;

namespace RogueEntity.Core.Sensing.Sources.Heat
{
    public class HeatResistanceView: IReadOnlyView2D<float>
    {
        readonly IReadOnlyView2D<SensoryResistance> cellProperties;

        public HeatResistanceView(IReadOnlyView2D<SensoryResistance> cellProperties)
        {
            this.cellProperties = cellProperties;
        }

        public bool TryGet(int x, int y, out float data)
        {
            if (cellProperties.TryGet(x, y, out var raw))
            {
                data = raw.BlocksHeat;
                return true;
            }

            data = default;
            return false;
        }

        public float this[int x, int y]
        {
            get
            {
                return cellProperties[x, y].BlocksHeat;
            }
        }
    }
}