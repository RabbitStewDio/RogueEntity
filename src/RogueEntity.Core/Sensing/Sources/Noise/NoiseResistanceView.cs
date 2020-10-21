using RogueEntity.Core.Sensing.Resistance;
using RogueEntity.Core.Utils.Maps;

namespace RogueEntity.Core.Sensing.Sources.Noise
{
    public class NoiseResistanceView: IReadOnlyView2D<float>
    {
        readonly IReadOnlyView2D<SensoryResistance> cellProperties;

        public NoiseResistanceView(IReadOnlyView2D<SensoryResistance> cellProperties)
        {
            this.cellProperties = cellProperties;
        }

        public float this[int x, int y]
        {
            get
            {
                return cellProperties[x, y].BlocksSound;
            }
        }
        
    }
}