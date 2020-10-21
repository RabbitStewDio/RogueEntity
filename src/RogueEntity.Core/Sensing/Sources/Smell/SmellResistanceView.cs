using RogueEntity.Core.Sensing.Resistance;
using RogueEntity.Core.Utils.Maps;

namespace RogueEntity.Core.Sensing.Sources.Smell
{
    public class SmellResistanceView: IReadOnlyView2D<float>
    {
        readonly IReadOnlyView2D<SensoryResistance> cellProperties;

        public SmellResistanceView(IReadOnlyView2D<SensoryResistance> cellProperties)
        {
            this.cellProperties = cellProperties;
        }

        public float this[int x, int y]
        {
            get
            {
                return cellProperties[x, y].BlocksSmell;
            }
        }

    }
}