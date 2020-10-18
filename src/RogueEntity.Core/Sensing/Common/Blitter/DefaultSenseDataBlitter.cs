using GoRogue;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Utils.Maps;

namespace RogueEntity.Core.Sensing.Common.Blitter
{
    public class DefaultSenseDataBlitter: ISenseDataBlitter
    {
        public void Blit(Rectangle bounds, 
                         Position2D sensePosition, 
                         SenseSourceData senseSource, 
                         BoundedDataView<float> brightnessTarget, 
                         BoundedDataView<byte> directionTarget)
        {
            for (int y = bounds.Y; y < bounds.Y + bounds.Height; y += 1)
            for (int x = bounds.X; x < bounds.X + bounds.Width; x += 1)
            {
                //var pos = new Position2D(x, y);
                if (senseSource.TryQuery(x, y, out var intensity, out var dir))
                {
                    brightnessTarget[x, y] += intensity;
                    directionTarget[x, y] |= dir.RawData;
                }
            }
        }
    }
}