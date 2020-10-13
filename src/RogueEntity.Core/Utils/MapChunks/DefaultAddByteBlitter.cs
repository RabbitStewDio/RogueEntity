using System;
using System.Collections.Generic;
using GoRogue;

namespace RogueEntity.Core.Utils.MapChunks
{
    public class DefaultAddByteBlitter : IAddByteBlitter
    {
        static byte AddWithoutOverflow(byte b1, byte b2)
        {
            var added = b1 + b2;
            return (byte)Math.Min(255, added);
        }

        public int MinimumChunkSize => 1;

        public void Process<TBlitter>(byte[] targetData, 
                                      int lineWidth, 
                                      IReadOnlyList<TBlitter> sources, 
                                      in Rectangle area) 
            where TBlitter : IByteBlitterDataSource
        {
            var yStart = area.MinExtentY;
            var yEnd = yStart + area.Height;
            var xStart = area.MinExtentX;
            var xEnd = xStart + area.Width;

            for (var y = yStart; y < yEnd; y += 1)
            {
                for (var x = xStart; x < xEnd; x += 1)
                {
                    var targetOffset = (x + y * lineWidth) * 4;

                    var blockLight = (byte)0;
                    var blockSound = (byte)0;
                    var blockHeat = (byte)0;
                    var blockReserved = (byte)0;

                    for (var index = 0; index < sources.Count; index++)
                    {
                        var processor = sources[index];
                        var sourceOffset = (x + y * processor.DataLineWidth) * processor.WordSize;
                        var sourceData = processor.Data;
                        if (sourceOffset < sourceData.Length)
                        {
                            blockLight = AddWithoutOverflow(blockLight, sourceData[sourceOffset]);
                            blockSound = AddWithoutOverflow(blockSound, sourceData[sourceOffset + 1]);
                            blockHeat = AddWithoutOverflow(blockHeat, sourceData[sourceOffset + 2]);
                            blockReserved = AddWithoutOverflow(blockReserved, sourceData[sourceOffset + 3]);
                        }
                    }

                    targetData[targetOffset] = blockLight;
                    targetData[targetOffset + 1] = blockSound;
                    targetData[targetOffset + 2] = blockHeat;
                    targetData[targetOffset + 3] = blockReserved;
                }
            }
        }
    }
}