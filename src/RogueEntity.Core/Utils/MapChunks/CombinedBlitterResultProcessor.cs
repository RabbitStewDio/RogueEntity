using System;
using System.Collections.Generic;
using GoRogue;

namespace RogueEntity.Core.Utils.MapChunks
{
    public class CombinedBlitterResultProcessor<TGameContext, TChunkProcessor> : ChunkProcessor<TGameContext> 
        where TChunkProcessor: ICachableChunkProcessor<TGameContext>, IByteBlitterDataSource
    {
        readonly byte[] data;
        readonly IAddByteBlitter blitter;
        readonly List<TChunkProcessor> dependencies;

        public CombinedBlitterResultProcessor(int width, 
                                              int height, 
                                              int wordSize,
                                              int spanSizeX, 
                                              int spanSizeY,
                                              byte[] data,
                                              IAddByteBlitter blitter) :
            base(width, height, spanSizeX, spanSizeY)
        {
            this.WordSize = wordSize;
            this.data = data;
            this.blitter = blitter;
            this.dependencies = new List<TChunkProcessor>();
        }
        
        public int WordSize { get; }

        public void Reset()
        {
            this.dependencies.Clear();
        }
        
        public void Add(TChunkProcessor processor)
        {
            dependencies.Add(processor);
        }

        public override bool CanProcess(int x, int y)
        {
            foreach (var processor in dependencies)
            {
                if (processor.CanProcess(x, y))
                    return true;
            }

            return false;
        }

        protected override void Process(TGameContext context, int yStart, int yEnd, int xStart, int xEnd)
        {
            var r = Rectangle.WithExtents(xStart, yStart, xEnd - 1, yEnd - 1);
            blitter.Process(data, Width, dependencies, in r);
        }
    }
}