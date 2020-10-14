using System;
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
                var pos = new Position2D(x, y);
                if (senseSource.TryQuery(x, y, out var intensity, out var dir, out var flags))
                {
                    brightnessTarget[in pos] += intensity;
                    directionTarget[in pos] |= SenseDirectionStore.From(dir, flags).RawData;
                }
            }
        }
    }
}