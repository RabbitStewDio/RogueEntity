using GoRogue;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Utils.Maps;

namespace RogueEntity.Core.Sensing.Common.Blitter
{
    public interface ISenseDataBlitter
    {
        void Blit(Rectangle bounds,
                  Position2D sensePosition,
                  SenseSourceData senseSource,
                  BoundedDataView<float> brightnessTarget,
                  BoundedDataView<byte> directionTarget);
    }
}