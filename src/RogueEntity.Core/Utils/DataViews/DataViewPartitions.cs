using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace RogueEntity.Core.Utils.DataViews
{
    public static class DataViewPartitions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int TileSplit(int pos, int offset, int tileSize)
        {
            var a = pos - offset;
            var d = a >= 0 ? a / tileSize : (a - tileSize + 1) / tileSize;
            return d;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static List<Rectangle> PartitionBy(in this Rectangle bounds,
                                                  int tileWidth,
                                                  int tileHeight,
                                                  List<Rectangle>? data = null)
        {
            return bounds.PartitionBy(0, 0, tileWidth, tileHeight, data);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static List<Rectangle> PartitionBy(in this Rectangle bounds,
                                                  in DynamicDataViewConfiguration config,
                                                  List<Rectangle>? data = null)
        {
            return PartitionBy(in bounds, config.OffsetX, config.OffsetY, config.TileSizeX, config.TileSizeY, data);
        }

        public static List<Rectangle> PartitionBy(in this Rectangle bounds,
                                                  int offsetX,
                                                  int offsetY,
                                                  int tileWidth,
                                                  int tileHeight,
                                                  List<Rectangle>? data = null)
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static List<BoundingBox> PartitionBy(in this BoundingBox bounds,
                                                    in DynamicDataViewConfiguration config,
                                                    List<BoundingBox>? data = null)
        {
            return PartitionBy(in bounds, config.OffsetX, config.OffsetY, config.TileSizeX, config.TileSizeY, data);
        }

        public static List<BoundingBox> PartitionBy(in this BoundingBox bounds,
                                                    int offsetX,
                                                    int offsetY,
                                                    int tileWidth,
                                                    int tileHeight,
                                                    List<BoundingBox>? data = null)
        {
            if (data == null)
            {
                data = new List<BoundingBox>();
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
                var left = dx * tileWidth + offsetX;
                var top = dy * tileHeight + offsetY;
                var partition = new BoundingBox(left, top, left + tileWidth, top + tileHeight);
                data.Add(partition);
            }

            return data;
        }
    }
}
