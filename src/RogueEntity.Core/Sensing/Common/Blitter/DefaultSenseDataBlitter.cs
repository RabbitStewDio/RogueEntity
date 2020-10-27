using System;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Utils.Maps;
using Rectangle = RogueEntity.Core.Utils.Rectangle;

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
                if (senseSource.TryQuery(x - sensePosition.X, y - sensePosition.Y, out var intensity, out var dir) && intensity > 0)
                {
                    brightnessTarget[x, y] = Math.Max(intensity, brightnessTarget[x, y]);
                    directionTarget[x, y] |= dir.RawData;
                }
            }
        }
    }
}