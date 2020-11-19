using System;
using RogueEntity.Core.Sensing.Common;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.DataViews;

namespace RogueEntity.Core.Sensing.Map
{
    public class DefaultSenseMapDataBlitter: ISenseMapDataBlitter
    {
        public void Blit(Rectangle bounds, Position2D sensePosition, SenseSourceData senseSource, BoundedDataView<float> brightnessTarget, BoundedDataView<byte> directionTarget)
        {
            for (int y = bounds.Y; y < bounds.Y + bounds.Height; y += 1)
            for (int x = bounds.X; x < bounds.X + bounds.Width; x += 1)
            {
                if (!senseSource.TryQuery(x - sensePosition.X, y - sensePosition.Y, out var intensity, out var dir))
                {
                    continue;
                }

                if (Math.Abs(intensity) < 0.005f)
                {
                    continue;
                }

                var b = brightnessTarget[x, y];
                if (Math.Sign(intensity) == Math.Sign(b))
                {
                    if (Math.Abs(intensity) > Math.Abs(b))
                    {
                        brightnessTarget[x, y] = intensity;
                        directionTarget[x, y] |= dir.RawData;
                    }
                }
                else
                {
                    brightnessTarget[x, y] = intensity + b;
                    directionTarget[x, y] |= dir.RawData;
                }
            }
        }

    }
}