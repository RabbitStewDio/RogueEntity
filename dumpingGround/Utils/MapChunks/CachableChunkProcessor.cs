using System;

namespace RogueEntity.Core.Utils.MapChunks
{
    public abstract class CachableChunkProcessor<TContext> : ChunkProcessor<TContext>, ICachableChunkProcessor<TContext>
    {
        public readonly bool[,] DirtyFlags;

        protected CachableChunkProcessor(int width, int height, 
                                         int spanSizeX, int spanSizeY) : base(width, height, spanSizeX, spanSizeY)
        {
            var flagsWidth = (int) Math.Ceiling(width / (float) SpanSizeX);
            var flagsHeight = (int) Math.Ceiling(height / (float) SpanSizeY);
            DirtyFlags = new bool[flagsWidth, flagsHeight];
            DataLineWidth = width;

            for (int y = 0; y < flagsHeight; y += 1)
            {
                for (int x = 0; x < flagsWidth; x += 1)
                {
                    DirtyFlags[x, y] = true;
                }
            }
        }

        public int DataLineWidth { get; }

        public override bool CanProcess(int x, int y)
        {
            return DirtyFlags[x / SpanSizeX, y / SpanSizeY];
        }

        public void MarkDirty(int x, int y, int radius)
        {
            var minX = Math.Max(0, (x - radius) / SpanSizeX);
            var minY = Math.Max(0, (y - radius) / SpanSizeY);
            var maxX = Math.Min(Width, x + radius) / SpanSizeX;
            var maxY = Math.Min(Width, y + radius) / SpanSizeY;

            for (var sy = minY; sy <= maxY; sy += 1)
            {
                for (var sx = minX; sx <= maxX; sx += 1)
                {
                    DirtyFlags[sx, sy] = true;
                }
            }
        }

        public void MarkDirty(int x, int y)
        {
            DirtyFlags[x / SpanSizeX, y / SpanSizeY] = true;
        }

        public void MarkClean(int x, int y)
        {
            DirtyFlags[x / SpanSizeX, y / SpanSizeY] = false;
        }
        
        public void ResetDirtyFlags()
        {
            Array.Clear(DirtyFlags, 0, DirtyFlags.Length);
        }

    }
}