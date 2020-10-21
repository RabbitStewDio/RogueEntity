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

        public float this[int x, int y]
        {
            get
            {
                return cellProperties[x, y].BlocksHeat;
            }
        }
    }
}