using System;
using RogueEntity.Core.Sensing.Common;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.DataViews;

namespace RogueEntity.Core.Sensing.Receptors
{
    /// <summary>
    ///   Follows a trail of a sense from the current position to the target. We do not have to check
    ///   for obstructions as the underlying flood-fill algorithm should have taken care of that for
    ///   us. Flood fill simulates air or water flow and thus never reports obstructed cells, as
    ///   fully occupied cells have no space for air or water. 
    /// </summary>
    public class DefaultDirectionalSenseReceptorBlitter : IDirectionalSenseReceptorBlitter
    {
        public void Blit(Rectangle bounds, 
                         GridPosition2D sensePosition, 
                         GridPosition2D receptorPosition, 
                         SenseSourceData senseSource, 
                         BoundedDataView<float> receptorSenseIntensities, 
                         BoundedDataView<byte> receptorSenseDirections)
        {
            var currentPos = receptorPosition;
            while (bounds.Contains(currentPos.X, currentPos.Y))
            {
                var sensePosRelative = currentPos - sensePosition;
                if (!senseSource.TryQuery(sensePosRelative.X, sensePosRelative.Y, out var intensity, out var dir))
                {
                    return;
                }
                
                if (Math.Abs(intensity) > Math.Abs(receptorSenseIntensities[currentPos]))
                {
                    receptorSenseIntensities.TrySet(currentPos, intensity);
                    receptorSenseDirections.TrySet(currentPos, dir.RawData);
                }
                
                var d = dir.ToDirectionalMovement();
                if (d.x == 0 && d.y == 0)
                {
                    return;
                }

                // inverse movement.
                currentPos -= d;
            }
        }
    }
}