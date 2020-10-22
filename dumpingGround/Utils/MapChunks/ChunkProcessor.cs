using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RogueEntity.Core.Utils.MapChunks
{
    public interface IChunkProcessor<TContext>
    {
        public bool CanProcess(int x, int y);
        public int Process(TContext context);
    }

    public abstract class ChunkProcessor<TContext>: IChunkProcessor<TContext>
    {
        public int SpanSizeX { get; }
        public int SpanSizeY { get; }
        public int Width { get; }
        public int Height { get; }
        readonly List<Task> tasks;

        protected ChunkProcessor(int width, int height, 
                                 int spanSizeX, int spanSizeY)
        {
            if (height <= 0) throw new ArgumentOutOfRangeException();
            if (width <= 0) throw new ArgumentOutOfRangeException();

            Width = width;
            Height = height;
            SpanSizeX = spanSizeX;
            SpanSizeY = spanSizeY;

            tasks = new List<Task>();
        }

        public virtual bool CanProcess(int x, int y) => true;

        public int Process(TContext context)
        {
            tasks.Clear();
            var dirtyCount = 0;
            var y = 0;
            while (y < Height)
            {
                var rowEnd = Math.Min(Height, y + SpanSizeY);
                var x = 0;
                while (x < Width)
                {
                    var colEnd = Math.Min(Width, x + SpanSizeX);
                    if (CanProcess(x, y))
                    {
                        dirtyCount += 1;

                        // Explicitly wrap the closure variables or strange side effects will happen.
                        var x1 = x;
                        var x2 = colEnd;
                        var y1 = y;
                        var y2 = rowEnd;
                        
                        var t = Task.Run(() => Process(context, y1, y2, x1, x2));
                        tasks.Add(t);
                        // Process(context, y1, y2, x1, x2);
                    }

                    x = colEnd;
                }

                y = rowEnd;
            }

            for (var i = tasks.Count - 1; i >= 0; i--)
            {
                var task = tasks[i];
                task.Wait();
                tasks[i] = null;
            }
            tasks.Clear();
            
            return dirtyCount;
        }

        protected abstract void Process(TContext context, int yStart, int yEnd, int xStart, int xEnd);
    }

    public static class ChunkProcessor
    {
        public static (int spanSizeX, int spanSizeY) ComputeSpanSize(IAddByteBlitter blitter, 
                                                                     int width, int height, 
                                                                     int processorCount = 0)
        {
            var chunkSize = blitter.MinimumChunkSize;
            processorCount = processorCount <= 0 ? Environment.ProcessorCount : processorCount;

            while (true)
            {
                var chunksX = (width / chunkSize);
                var chunksY = (height / chunkSize);
                if (chunksX < 2 || chunksY < 2)
                {
                    var blt = blitter.MinimumChunkSize;
                    var spanSizeX = width / 2;
                    if (spanSizeX < blt)
                    {
                        spanSizeX = width;
                    }
                    var spanSizeY = height / 2;
                    if (spanSizeY < blt)
                    {
                        spanSizeY = width;
                    }

                    return (spanSizeX, spanSizeY);
                }

                if (chunksX * chunksY <= processorCount * 2)
                {
                    return (width / chunksX, width / chunksY);
                }

                chunkSize *= 2;
            }

        }
    }
}