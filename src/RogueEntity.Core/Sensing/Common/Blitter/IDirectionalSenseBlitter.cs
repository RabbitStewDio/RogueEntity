using RogueEntity.Core.Positioning;
using RogueEntity.Core.Utils.Maps;
using Rectangle = RogueEntity.Core.Utils.Rectangle;

namespace RogueEntity.Core.Sensing.Common.Blitter
{
    public interface IDirectionalSenseBlitter
    {
        void Blit(Rectangle bounds,
                  Position2D sensePosition,
                  Position2D receptorPosition,
                  SenseSourceData senseSource,
                  BoundedDataView<float> brightnessTarget,
                  BoundedDataView<byte> directionTarget);

    }
}