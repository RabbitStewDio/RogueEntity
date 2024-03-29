using RogueEntity.Api.Utils;
using System;
using RogueEntity.Core.Sensing.Common;
using RogueEntity.Core.Utils;
using RogueEntity.Core.Utils.DataViews;
using Serilog;

namespace RogueEntity.Core.Sensing.Receptors
{
    public class DefaultRadiationSenseReceptorBlitter: IRadiationSenseReceptorBlitter
    {
        readonly ILogger logger = SLog.ForContext<DefaultRadiationSenseReceptorBlitter>();
        
        public void Blit(Rectangle bounds, 
                         GridPosition2D sensePosition, 
                         GridPosition2D receptorPosition,
                         SenseSourceData senseSource, 
                         BoundedDataView<float> receptorSenseIntensities, 
                         BoundedDataView<byte> receptorSenseDirections)
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

                if (dir.IsObstructed)
                {
                    // todo: Don't allow directly opposed stuff.
                    var directionFromReceptor = SenseDirectionStore.From(new GridPosition2D(x, y) - receptorPosition).Direction;

                    if (directionFromReceptor != dir.Direction)
                    {
                        if (SenseDirectionStore.IsViewBlocked(directionFromReceptor, dir.Direction))
                        {
                            // if there is no overlapping directionality in the light emitted by the sense source
                            // and the receptor, then the light shines on an obstructed view and is not visible.
                            logger.Verbose("{X}, {Y} is blocked outright", x, y);
                            continue;
                        }

                        var overlapping = directionFromReceptor & dir.Direction;
                        var dx = SenseDirectionStore.ToDirectionalMovement(overlapping);
                        if (dx.x != 0)
                        {
                            if (IsBlocked(senseSource, x - sensePosition.X - dx.x, y - sensePosition.Y))
                            {
                                logger.Verbose("{Overlapping} - {Position} is blocked horizontally at {BlockPosition}", overlapping, new GridPosition2D(x, y), new GridPosition2D(x - dx.x, y));
                                continue;
                            }
                        }
                        if (dx.y != 0)
                        {
                            if (IsBlocked(senseSource, x - sensePosition.X, y - sensePosition.Y - dx.y))
                            {
                                logger.Verbose("{Overlapping} - {Position} is blocked horizontally at {BlockPosition}", overlapping, new GridPosition2D(x, y), new GridPosition2D(x, y - dx.y));
                                continue;
                            }
                        }
                    }
                }
                
                if (Math.Abs(intensity) > Math.Abs(receptorSenseIntensities[x, y]))
                {
                    receptorSenseIntensities[x, y] = intensity;
                    receptorSenseDirections[x, y] = dir.RawData;
                }
            }
            
            
        }

        bool IsBlocked(SenseSourceData senseSource, int x, int y)
        {
            if (!senseSource.TryQuery(x, y, out var _, out var dir))
            {
                return true;
            }
            return dir.IsObstructed;
        }
    }
}