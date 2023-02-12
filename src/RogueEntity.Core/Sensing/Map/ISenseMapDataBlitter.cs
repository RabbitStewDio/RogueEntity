using RogueEntity.Core.Sensing.Common;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.DataViews;

namespace RogueEntity.Core.Sensing.Map
{
    public interface ISenseMapDataBlitter
    {
        void Blit(Rectangle bounds,
                  GridPosition2D sensePosition,
                  SenseSourceData senseSource,
                  BoundedDataView<float> brightnessTarget,
                  BoundedDataView<byte> directionTarget);
    }
}