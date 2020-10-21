using System.Collections.Generic;
using GoRogue;

namespace RogueEntity.Core.Utils.Maps
{
    public static class MapPartitions
    {
        public static int TileSplit(int pos, int offset, int tileSize)
        {
            var a = pos - offset;
            var d = a >= 0 ? a / tileSize : (a - tileSize + 1) / tileSize;
            return d;
        }

        public static List<Rectangle> PartitionBy(this Rectangle bounds,
                                                  int tileWidth,
                                                  int tileHeight,
                                                  List<Rectangle> data = null)
        {
            return bounds.PartitionBy(0, 0, tileWidth, tileHeight, data);
        }
        
        public static List<Rectangle> PartitionBy(this Rectangle bounds,
                                                  int offsetX,
                                                  int offsetY,
                                                  int tileWidth,
                                                  int tileHeight,
                                                  List<Rectangle> data = null)
        {
            if (data == null)
            {
                data = new List<Rectangle>();
            }
            else
            {
                data.Clear();
            }
            
            var minX = TileSplit(bounds.MinExtentX, offsetX, tileWidth);
            var minY = TileSplit(bounds.MinExtentY, offsetY, tileHeight);
            var maxX = TileSplit(bounds.MaxExtentX, offsetX, tileWidth);
            var maxY = TileSplit(bounds.MaxExtentY, offsetY, tileHeight);
            for (int dy = minY; dy <= maxY; dy += 1)
            for (int dx = minX; dx <= maxX; dx += 1)
            {
                var partition = new Rectangle(dx * tileWidth + offsetX, dy * tileHeight + offsetY, tileWidth, tileHeight);
                data.Add(partition);
            }

            return data;
        }
    }
}