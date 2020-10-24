using GoRogue;
using RogueEntity.Core.Positioning;
using RogueEntity.Core.Utils.Maps;
using Rectangle = RogueEntity.Core.Utils.Rectangle;

namespace RogueEntity.Core.Sensing.Common.Blitter
{
    /// <summary>
    ///   Follows a trail of a sense from the current position to the target.
    /// </summary>
    public class DefaultDirectionalSenseBlitter : IDirectionalSenseBlitter
    {
        public void Blit(Rectangle bounds, 
                         Position2D sensePosition, 
                         Position2D receptorPosition, 
                         SenseSourceData senseSource, 
                         BoundedDataView<float> brightnessTarget, 
                         BoundedDataView<byte> directionTarget)
        {
            var currentPos = receptorPosition;
            while (bounds.Contains(currentPos.X, currentPos.Y))
            {
                if (!senseSource.TryQuery(currentPos.X, currentPos.Y, out var brightness, out var dir))
                {
                    return;
                }
                
                var existingBrightness = brightnessTarget[currentPos];
                if (existingBrightness < brightness)
                {
                    brightnessTarget.TrySet(currentPos, brightness);
                    directionTarget.TrySet(currentPos, dir.RawData);
                }
                
                var d = dir.ToDirectionalMovement();
                if (d.x == 0 && d.y == 0)
                {
                    return;
                }

                currentPos += d;
            }
        }
    }
}